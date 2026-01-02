using System.Net;
using System.Net.Sockets;

namespace NetworkUtilityApp.Helpers
{
    internal static class LlmnrResolver
    {
        public static string TryGetHostname(string ip, int timeoutMs = 1200, string? localBindIp = null)
        {
            try
            {
                if (!IPAddress.TryParse(ip, out var addr) || addr.AddressFamily != AddressFamily.InterNetwork)
                    return string.Empty;
                var reverseName = ToArpa(addr);
                using var udp = new UdpClient();
                udp.Client.ReceiveTimeout = timeoutMs;

                if (!string.IsNullOrWhiteSpace(localBindIp) && IPAddress.TryParse(localBindIp, out var bindAddr))
                {
                    udp.Client.Bind(new IPEndPoint(bindAddr, 0));
                    try { udp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, bindAddr.GetAddressBytes()); } catch { }
                }

                var mcast = IPAddress.Parse("224.0.0.252");
                try { udp.JoinMulticastGroup(mcast); } catch { }
                var endPoint = new IPEndPoint(mcast, 5355);

                var ptr = BuildDnsQuery(reverseName, type: 12);
                udp.Send(ptr, ptr.Length, endPoint);

                var a = BuildDnsQuery("_workstation._tcp.local", type: 1);
                udp.Send(a, a.Length, endPoint);

                var remote = new IPEndPoint(IPAddress.Any, 0);
                var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                while (DateTime.UtcNow < deadline)
                {
                    byte[] resp;
                    try { resp = udp.Receive(ref remote); } catch { break; }
                    if (resp == null || resp.Length < 12) continue;
                    if (remote.Address.ToString() != ip) continue;
                    var name = ParsePtrHostname(resp);
                    if (!string.IsNullOrWhiteSpace(name)) return name;
                }
            }
            catch { return string.Empty; }
            return string.Empty;
        }

        private static byte[] BuildDnsQuery(string qname, ushort type)
        {
            var id = (ushort)Random.Shared.Next(0, 0xFFFF);
            var bytes = new List<byte>(128)
            {
                (byte)(id >> 8), (byte)id,
                0x00, 0x00,
                0x00, 0x01,
                0x00, 0x00,
                0x00, 0x00,
                0x00, 0x00
            };
            foreach (var label in qname.Split('.'))
            {
                var lb = System.Text.Encoding.ASCII.GetBytes(label);
                bytes.Add((byte)lb.Length);
                bytes.AddRange(lb);
            }
            bytes.Add(0x00);
            bytes.AddRange([(byte)(type >> 8), (byte)type, 0x00, 0x01]);
            return [.. bytes];
        }

        private static string ParsePtrHostname(byte[] resp)
        {
            try
            {
                if (resp == null || resp.Length < 12) return string.Empty;
                int qd = (resp[4] << 8) | resp[5];
                int an = (resp[6] << 8) | resp[7];
                int off = 12;
                for (int i = 0; i < qd; i++) off = SkipName(resp, off) + 4;
                for (int i = 0; i < an; i++)
                {
                    off = SkipName(resp, off);
                    if (off + 10 > resp.Length) break;
                    ushort type = (ushort)((resp[off] << 8) | resp[off + 1]);
                    ushort _class = (ushort)((resp[off + 2] << 8) | resp[off + 3]);
                    int rdlength = (resp[off + 8] << 8) | resp[off + 9];
                    off += 10;
                    if (type == 12 && _class == 1)
                    {
                        var name = ReadName(resp, off);
                        if (!string.IsNullOrWhiteSpace(name)) return name.TrimEnd('.');
                    }
                    off += rdlength;
                }
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
            while (i < buf.Length && jumps < 5)
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

        private static string ToArpa(IPAddress ip)
        {
            var b = ip.GetAddressBytes();
            return $"{b[3]}.{b[2]}.{b[1]}.{b[0]}.in-addr.arpa";
        }
    }
}
