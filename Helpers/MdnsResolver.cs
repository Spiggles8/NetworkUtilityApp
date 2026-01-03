using System.Net;
using System.Net.Sockets;

namespace NetworkUtilityApp.Helpers
{
    /// <summary>
    /// Very small mDNS (multicast DNS) helper used by discovery.
    ///
    /// It sends a handful of PTR queries for common service types over
    /// 224.0.0.251:5353 and then tries to infer a human-friendly hostname
    /// from any response coming back from the target IP.
    ///
    /// This is intentionally heuristic and shallow – just enough to surface
    /// something readable in the UI without pulling in a full mDNS stack.
    /// </summary>
    internal static class MdnsResolver
    {
        // Service PTRs we probe for. Many devices answer at least one of these.
        private static readonly string[] ServiceQueries =
        [
            "_workstation._tcp.local",
            "_device-info._tcp.local",
            "_googlecast._tcp.local",
            "_androidtvremote._tcp.local",
            "_airplay._tcp.local",
            "_companion-link._tcp.local"
        ];

        /// <summary>
        /// Try to obtain a hostname for the specified IP using mDNS.
        /// Returns empty string on failure/timeout.
        /// </summary>
        /// <param name="ip">Target IPv4/IPv6 address as string.</param>
        /// <param name="timeoutMs">Overall receive timeout for mDNS replies.</param>
        /// <param name="localBindIp">
        /// Optional local IP to bind the UDP socket to. Useful when
        /// multiple active interfaces exist and we want a specific one.
        /// </param>
        public static string TryGetHostname(string ip, int timeoutMs = 1500, string? localBindIp = null)
        {
            try
            {
                using var udp = new UdpClient();
                udp.Client.ReceiveTimeout = timeoutMs;

                // Optionally pin the query to a specific local interface
                if (!string.IsNullOrWhiteSpace(localBindIp) && IPAddress.TryParse(localBindIp, out var bindAddr))
                {
                    udp.Client.Bind(new IPEndPoint(bindAddr, 0));
                    try
                    {
                        udp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, bindAddr.GetAddressBytes());
                    }
                    catch { }
                }

                var mcast = IPAddress.Parse("224.0.0.251"); // standard mDNS v4 group
                try { udp.JoinMulticastGroup(mcast); } catch { }
                var endPoint = new IPEndPoint(mcast, 5353);

                // Send multiple service PTR queries to increase response likelihood.
                foreach (var svc in ServiceQueries)
                {
                    var q = BuildDnsQuery(svc, type: 12 /* PTR */);
                    udp.Send(q, q.Length, endPoint);
                }

                var remote = new IPEndPoint(IPAddress.Any, 0);
                var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                while (DateTime.UtcNow < deadline)
                {
                    byte[] resp;
                    try { resp = udp.Receive(ref remote); }
                    catch { break; }

                    if (resp == null || resp.Length < 12) continue; // too short to be DNS

                    // Only consider responses that claim to originate from the target IP.
                    if (remote.Address.ToString() != ip) continue;

                    // Parse hostname-ish candidates from SRV/A/PTR records.
                    var host = ParseHostnameFromMdns(resp);
                    if (!string.IsNullOrWhiteSpace(host)) return host;
                }
            }
            catch { }

            return string.Empty;
        }

        // Build a small DNS query for the given QNAME/QTYPE.
        private static byte[] BuildDnsQuery(string qname, ushort type)
        {
            var id = (ushort)Random.Shared.Next(0, 0xFFFF);

            // DNS header: ID, flags, QDCOUNT=1, AN/NS/AR=0
            var bytes = new List<byte>(128)
            {
                (byte)(id >> 8), (byte)id,
                0x00, 0x00, // flags
                0x00, 0x01, // QDCOUNT
                0x00, 0x00, // ANCOUNT
                0x00, 0x00, // NSCOUNT
                0x00, 0x00  // ARCOUNT
            };

            // QNAME encoded as length-prefixed labels: [len][label]...[0]
            foreach (var label in qname.Split('.'))
            {
                var lb = System.Text.Encoding.ASCII.GetBytes(label);
                bytes.Add((byte)lb.Length);
                bytes.AddRange(lb);
            }
            bytes.Add(0x00); // end of QNAME

            // QTYPE/QCLASS (IN)
            bytes.Add((byte)(type >> 8)); bytes.Add((byte)type);
            bytes.Add(0x00); bytes.Add(0x01);
            return [.. bytes];
        }

        /// <summary>
        /// Extract a best-guess hostname from an mDNS response.
        /// Looks at PTR, SRV and A/AAAA owner names across answer,
        /// authority and additional sections.
        /// </summary>
        private static string ParseHostnameFromMdns(byte[] resp)
        {
            try
            {
                int qd = (resp[4] << 8) | resp[5]; // question count
                int an = (resp[6] << 8) | resp[7]; // answer count
                int ns = (resp[8] << 8) | resp[9]; // authority count
                int ar = (resp[10] << 8) | resp[11]; // additional count
                int off = 12; // start of question section

                // Skip over all questions (QNAME + QTYPE/QCLASS)
                for (int i = 0; i < qd; i++)
                    off = SkipName(resp, off) + 4;

                var candidates = new List<string>();
                int total = an + ns + ar;

                // Walk all RRs across answer/authority/additional
                for (int i = 0; i < total; i++)
                {
                    off = SkipName(resp, off); // owner NAME
                    if (off + 10 > resp.Length) break;

                    ushort type = (ushort)((resp[off] << 8) | resp[off + 1]);
                    ushort _class = (ushort)((resp[off + 2] << 8) | resp[off + 3]);
                    int rdlength = (resp[off + 8] << 8) | resp[off + 9];
                    off += 10;

                    if (_class != 1) // we only care about IN class
                    {
                        off += rdlength;
                        continue;
                    }

                    if (type == 12) // PTR -> target service instance name
                    {
                        var name = ReadName(resp, off);
                        if (!string.IsNullOrWhiteSpace(name)) candidates.Add(name);
                    }
                    else if (type == 33) // SRV -> concrete hostname
                    {
                        // priority(2) weight(2) port(2) followed by target NAME
                        int srvOff = off + 6;
                        var target = ReadName(resp, srvOff);
                        if (!string.IsNullOrWhiteSpace(target)) candidates.Add(target);
                    }
                    else if (type == 1 || type == 28) // A / AAAA -> owner NAME is interesting
                    {
                        var owner = ReadOwnerName(resp, off - 10); // rewind to start of RR
                        if (!string.IsNullOrWhiteSpace(owner)) candidates.Add(owner);
                    }

                    off += rdlength;
                }

                // Prefer .local hostnames (typical for mDNS), else fall back
                var host = candidates.FirstOrDefault(c => c.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
                           ?? candidates.FirstOrDefault();
                return host?.TrimEnd('.') ?? string.Empty;
            }
            catch { }

            return string.Empty;
        }

        /// <summary>
        /// Skip over a DNS NAME (handles compression pointers) and
        /// return the offset immediately following it.
        /// </summary>
        private static int SkipName(byte[] buf, int off)
        {
            int i = off;
            while (i < buf.Length)
            {
                byte len = buf[i++];
                if (len == 0) break;           // end of labels
                if ((len & 0xC0) == 0xC0)      // compression pointer
                {
                    i++;                       // skip pointer offset byte
                    break;
                }
                i += len;                      // skip label payload
            }
            return i;
        }

        /// <summary>
        /// Read a DNS NAME at the given offset, following up to a small
        /// number of compression jumps to avoid infinite loops.
        /// </summary>
        private static string ReadName(byte[] buf, int off)
        {
            var labels = new List<string>();
            int i = off;
            int jumps = 0;

            while (i < buf.Length && jumps < 10)
            {
                byte len = buf[i++];
                if (len == 0) break;

                if ((len & 0xC0) == 0xC0) // compression pointer
                {
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

        /// <summary>
        /// Heuristic helper that tries to recover the owner NAME for an
        /// A/AAAA RR given the start of the record.
        ///
        /// In a full DNS parser we would track offsets precisely; here we
        /// simply walk backwards to a plausible NAME boundary and call
        /// <see cref="ReadName"/> from there.
        /// </summary>
        private static string ReadOwnerName(byte[] buf, int rrStart)
        {
            int i = rrStart;
            // Walk backwards until we hit 0-length label or a pointer boundary
            while (i > 0 && buf[i - 1] != 0 && (buf[i - 1] & 0xC0) != 0xC0)
                i--;

            return ReadName(buf, i);
        }
    }
}
