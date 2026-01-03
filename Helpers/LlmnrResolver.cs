using System.Net;
using System.Net.Sockets;

namespace NetworkUtilityApp.Helpers
{
    /// <summary>
    /// Lightweight LLMNR (Link-Local Multicast Name Resolution) helper.
    /// Sends a reverse-DNS PTR query over multicast (224.0.0.252:5355)
    /// for an IPv4 address and attempts to parse the hostname from
    /// any matching response.
    ///
    /// This intentionally keeps the implementation small and self-contained
    /// so it can be called from discovery code without extra dependencies.
    /// </summary>
    internal static class LlmnrResolver
    {
        /// <summary>
        /// Try to resolve a hostname for the given IPv4 address using LLMNR.
        /// Returns an empty string on failure or timeout.
        /// </summary>
        /// <param name="ip">Target IPv4 address (string form).</param>
        /// <param name="timeoutMs">Receive timeout in milliseconds.</param>
        /// <param name="localBindIp">
        /// Optional local IPv4 to bind the UDP socket to (useful when multiple
        /// interfaces are present).
        /// </param>
        public static string TryGetHostname(string ip, int timeoutMs = 1200, string? localBindIp = null)
        {
            try
            {
                // Basic guard: only proceed for valid IPv4 addresses
                if (!IPAddress.TryParse(ip, out var addr) || addr.AddressFamily != AddressFamily.InterNetwork)
                    return string.Empty;

                var reverseName = ToArpa(addr);
                using var udp = new UdpClient();
                udp.Client.ReceiveTimeout = timeoutMs;

                // Optionally bind to a specific local interface
                if (!string.IsNullOrWhiteSpace(localBindIp) && IPAddress.TryParse(localBindIp, out var bindAddr))
                {
                    udp.Client.Bind(new IPEndPoint(bindAddr, 0));
                    try
                    {
                        // Hint the stack which interface to use for multicast
                        udp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, bindAddr.GetAddressBytes());
                    }
                    catch { }
                }

                var mcast = IPAddress.Parse("224.0.0.252");
                try { udp.JoinMulticastGroup(mcast); } catch { }
                var endPoint = new IPEndPoint(mcast, 5355);

                // Send a PTR query for the reverse name
                var ptr = BuildDnsQuery(reverseName, type: 12);
                udp.Send(ptr, ptr.Length, endPoint);

                // Also send a simple A query for a generic workstation label
                // to increase chances of getting some response on busy networks.
                var a = BuildDnsQuery("_workstation._tcp.local", type: 1);
                udp.Send(a, a.Length, endPoint);

                var remote = new IPEndPoint(IPAddress.Any, 0);
                var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                while (DateTime.UtcNow < deadline)
                {
                    byte[] resp;
                    try { resp = udp.Receive(ref remote); }
                    catch { break; }

                    if (resp == null || resp.Length < 12) continue;
                    // Only accept responses claiming to originate from the target IP
                    if (remote.Address.ToString() != ip) continue;

                    var name = ParsePtrHostname(resp);
                    if (!string.IsNullOrWhiteSpace(name)) return name;
                }
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }

        // Build a minimal DNS query packet for the given name and record type.
        private static byte[] BuildDnsQuery(string qname, ushort type)
        {
            var id = (ushort)Random.Shared.Next(0, 0xFFFF);
            // DNS header: ID, flags, QDCOUNT=1, AN/NS/AR=0
            var bytes = new List<byte>(128)
            {
                (byte)(id >> 8), (byte)id,
                0x00, 0x00,
                0x00, 0x01,
                0x00, 0x00,
                0x00, 0x00,
                0x00, 0x00
            };

            // QNAME as sequence of labels: [len][label]...[0]
            foreach (var label in qname.Split('.'))
            {
                var lb = System.Text.Encoding.ASCII.GetBytes(label);
                bytes.Add((byte)lb.Length);
                bytes.AddRange(lb);
            }
            bytes.Add(0x00);

            // QTYPE / QCLASS (IN)
            bytes.AddRange([(byte)(type >> 8), (byte)type, 0x00, 0x01]);
            return [.. bytes];
        }

        // Parse a PTR hostname from a DNS response buffer, if present.
        private static string ParsePtrHostname(byte[] resp)
        {
            try
            {
                if (resp == null || resp.Length < 12) return string.Empty;

                int qd = (resp[4] << 8) | resp[5]; // question count
                int an = (resp[6] << 8) | resp[7]; // answer count
                int off = 12;                      // start of question section

                // Skip all question entries
                for (int i = 0; i < qd; i++)
                    off = SkipName(resp, off) + 4; // QTYPE/QCLASS

                // Walk answer records and return the first PTR name we find
                for (int i = 0; i < an; i++)
                {
                    off = SkipName(resp, off); // owner name
                    if (off + 10 > resp.Length) break;

                    ushort type = (ushort)((resp[off] << 8) | resp[off + 1]);
                    ushort _class = (ushort)((resp[off + 2] << 8) | resp[off + 3]);
                    int rdlength = (resp[off + 8] << 8) | resp[off + 9];
                    off += 10;

                    if (type == 12 && _class == 1) // PTR, IN
                    {
                        var name = ReadName(resp, off);
                        if (!string.IsNullOrWhiteSpace(name))
                            return name.TrimEnd('.');
                    }

                    off += rdlength;
                }
            }
            catch { }

            return string.Empty;
        }

        //Skip over a DNS name (handling compression pointers) and
        //return the offset immediately following it.
        private static int SkipName(byte[] buf, int off)
        {
            int i = off;
            while (i < buf.Length)
            {
                byte len = buf[i++];
                if (len == 0) break;          // end of name
                if ((len & 0xC0) == 0xC0)     // compression pointer
                {
                    i++;                      // skip pointer target byte
                    break;
                }
                i += len;                     // skip label payload
            }
            return i;
        }

        // Read a DNS name at the specified offset, following up to a few
        // compression pointers to avoid loops.
        private static string ReadName(byte[] buf, int off)
        {
            var labels = new List<string>();
            int i = off;
            int jumps = 0;

            while (i < buf.Length && jumps < 5)
            {
                byte len = buf[i++];
                if (len == 0) break;

                if ((len & 0xC0) == 0xC0)
                {
                    // Compression pointer: high 2 bits set, remaining 14 bits are offset
                    int ptr = ((len & 0x3F) << 8) | buf[i++];
                    i = ptr;
                    jumps++;
                    continue;
                }

                if (i + len > buf.Length) break;
                labels.Add(System.Text.Encoding.ASCII.GetString(buf, i, len));
                i += len;
            }

            return string.Join('.', labels);
        }

        // Convert an IPv4 address into its reverse-DNS (in-addr.arpa) form.
        private static string ToArpa(IPAddress ip)
        {
            var b = ip.GetAddressBytes();
            return $"{b[3]}.{b[2]}.{b[1]}.{b[0]}.in-addr.arpa";
        }
    }
}
