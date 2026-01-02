using System.Net;
using System.Net.Sockets;

namespace NetworkUtilityApp.Helpers
{
    internal static class MdnsResolver
    {
        private static readonly string[] ServiceQueries = new[]
        {
            "_workstation._tcp.local",
            "_device-info._tcp.local",
            "_googlecast._tcp.local",
            "_androidtvremote._tcp.local",
            "_airplay._tcp.local",
            "_companion-link._tcp.local"
        };

        public static string TryGetHostname(string ip, int timeoutMs = 1500, string? localBindIp = null)
        {
            try
            {
                using var udp = new UdpClient();
                udp.Client.ReceiveTimeout = timeoutMs;

                if (!string.IsNullOrWhiteSpace(localBindIp) && IPAddress.TryParse(localBindIp, out var bindAddr))
                {
                    udp.Client.Bind(new IPEndPoint(bindAddr, 0));
                    try { udp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, bindAddr.GetAddressBytes()); } catch { }
                }

                var mcast = IPAddress.Parse("224.0.0.251");
                try { udp.JoinMulticastGroup(mcast); } catch { }
                var endPoint = new IPEndPoint(mcast, 5353);

                // Send multiple service queries to increase response likelihood
                foreach (var svc in ServiceQueries)
                {
                    var q = BuildDnsQuery(svc, type: 12 /*PTR*/);
                    udp.Send(q, q.Length, endPoint);
                }

                var remote = new IPEndPoint(IPAddress.Any, 0);
                var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                while (DateTime.UtcNow < deadline)
                {
                    byte[] resp;
                    try { resp = udp.Receive(ref remote); }
                    catch { break; }
                    if (resp == null || resp.Length < 12) continue;
                    if (remote.Address.ToString() != ip) continue;

                    // Parse hostname from SRV/A/PTR records
                    var host = ParseHostnameFromMdns(resp);
                    if (!string.IsNullOrWhiteSpace(host)) return host;
                }
            }
            catch { }
            return string.Empty;
        }

        private static byte[] BuildDnsQuery(string qname, ushort type)
        {
            var id = (ushort)Random.Shared.Next(0, 0xFFFF);
            var bytes = new List<byte>(128);
            bytes.Add((byte)(id >> 8)); bytes.Add((byte)id);
            bytes.Add(0x00); bytes.Add(0x00); // flags
            bytes.Add(0x00); bytes.Add(0x01); // QDCOUNT
            bytes.Add(0x00); bytes.Add(0x00); // ANCOUNT
            bytes.Add(0x00); bytes.Add(0x00); // NSCOUNT
            bytes.Add(0x00); bytes.Add(0x00); // ARCOUNT
            foreach (var label in qname.Split('.'))
            {
                var lb = System.Text.Encoding.ASCII.GetBytes(label);
                bytes.Add((byte)lb.Length);
                bytes.AddRange(lb);
            }
            bytes.Add(0x00);
            bytes.Add((byte)(type >> 8)); bytes.Add((byte)type);
            bytes.Add(0x00); bytes.Add(0x01);
            return bytes.ToArray();
        }

        private static string ParseHostnameFromMdns(byte[] resp)
        {
            try
            {
                int qd = (resp[4] << 8) | resp[5];
                int an = (resp[6] << 8) | resp[7];
                int ns = (resp[8] << 8) | resp[9];
                int ar = (resp[10] << 8) | resp[11];
                int off = 12;
                for (int i = 0; i < qd; i++) off = SkipName(resp, off) + 4;

                // Collect potential hostnames from answers and additional records
                var candidates = new List<string>();

                int total = an + ns + ar;
                for (int i = 0; i < total; i++)
                {
                    off = SkipName(resp, off);
                    if (off + 10 > resp.Length) break;
                    ushort type = (ushort)((resp[off] << 8) | resp[off + 1]);
                    ushort _class = (ushort)((resp[off + 2] << 8) | resp[off + 3]);
                    int rdlength = (resp[off + 8] << 8) | resp[off + 9];
                    off += 10;
                    if (_class != 1) { off += rdlength; continue; }

                    if (type == 12) // PTR -> target service name
                    {
                        var name = ReadName(resp, off);
                        if (!string.IsNullOrWhiteSpace(name)) candidates.Add(name);
                    }
                    else if (type == 33) // SRV -> target hostname
                    {
                        // priority(2) weight(2) port(2), then target NAME
                        int srvOff = off + 6;
                        var target = ReadName(resp, srvOff);
                        if (!string.IsNullOrWhiteSpace(target)) candidates.Add(target);
                    }
                    else if (type == 1 || type == 28) // A/AAAA -> owner NAME
                    {
                        var owner = ReadOwnerName(resp, off - 10); // go back to RR owner
                        if (!string.IsNullOrWhiteSpace(owner)) candidates.Add(owner);
                    }
                    off += rdlength;
                }

                // Prefer .local hosts
                var host = candidates.FirstOrDefault(c => c.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
                           ?? candidates.FirstOrDefault();
                return host?.TrimEnd('.') ?? string.Empty;
            }
            catch { }
            return string.Empty;
        }

        private static int SkipName(byte[] buf, int off)
        {
            int i = off;
            while (i < buf.Length)
            {
                byte len = buf[i++];
                if (len == 0) break;
                if ((len & 0xC0) == 0xC0) { i++; break; }
                i += len;
            }
            return i;
        }
        private static string ReadName(byte[] buf, int off)
        {
            var labels = new List<string>();
            int i = off;
            int jumps = 0;
            while (i < buf.Length && jumps < 10)
            {
                byte len = buf[i++];
                if (len == 0) break;
                if ((len & 0xC0) == 0xC0)
                {
                    int ptr = ((len & 0x3F) << 8) | buf[i++];
                    i = ptr; jumps++;
                    continue;
                }
                if (i + len > buf.Length) break;
                labels.Add(System.Text.Encoding.ASCII.GetString(buf, i, len));
                i += len;
            }
            return string.Join('.', labels);
        }
        private static string ReadOwnerName(byte[] buf, int rrStart)
        {
            // Owner NAME starts at rrStart - we need to backtrack to beginning of NAME label
            // For simplicity in this heuristic parser we re-walk from the nearest earlier zero/pointer boundary.
            // Fall back to empty when parsing is ambiguous.
            // In most mDNS responses owner name appears just before TYPE.
            int i = rrStart;
            // Find previous zero-length or pointer start
            while (i > 0 && buf[i - 1] != 0 && (buf[i - 1] & 0xC0) != 0xC0) i--;
            return ReadName(buf, i);
        }
    }
}
