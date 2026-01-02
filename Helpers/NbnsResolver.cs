using System.Net;
using System.Net.Sockets;

namespace NetworkUtilityApp.Helpers
{
    internal static class NbnsResolver
    {
        // Sends NBNS Node Status (NBSTAT) request to UDP/137 and parses UNIQUE <00> name.
        public static string TryGetHostname(string ip, int timeoutMs = 1200, string? localBindIp = null)
        {
            try
            {
                var endpoint = new IPEndPoint(IPAddress.Parse(ip), 137);
                using var udp = new UdpClient();
                udp.Client.ReceiveTimeout = timeoutMs;
                // Bind to specific adapter if provided
                if (!string.IsNullOrWhiteSpace(localBindIp) && IPAddress.TryParse(localBindIp, out var bindAddr))
                {
                    udp.Client.Bind(new IPEndPoint(bindAddr, 0));
                }
                udp.Connect(endpoint);

                // Build NBNS packet: Transaction ID, Flags, QDCount=1, ANCOUNT=0, NSCOUNT=0, ARCOUNT=0
                // Question: NAME='*' (NetBIOS encoded), TYPE=0x0021 (NBSTAT), CLASS=0x0001
                var txId = (ushort)Random.Shared.Next(0, 0xFFFF);
                var packet = new List<byte>(64);
                packet.Add((byte)(txId >> 8));
                packet.Add((byte)(txId & 0xFF));
                // Flags: 0x0000 for request
                packet.Add(0x00); packet.Add(0x00);
                // QDCOUNT=1
                packet.Add(0x00); packet.Add(0x01);
                // ANCOUNT, NSCOUNT, ARCOUNT = 0
                packet.Add(0x00); packet.Add(0x00);
                packet.Add(0x00); packet.Add(0x00);
                packet.Add(0x00); packet.Add(0x00);

                // Encode NetBIOS name '*'
                // NetBIOS name is 16 bytes. '*' followed by 15 spaces. Then encoded using RFC 1002 (two ASCII per nibble).
                var nameBytes = new byte[16];
                nameBytes[0] = (byte)'*';
                for (int i = 1; i < 16; i++) nameBytes[i] = (byte)' ';
                var encoded = EncodeNetbiosName(nameBytes);
                // Prepend length and append 0x00 terminator
                packet.Add((byte)encoded.Length);
                packet.AddRange(encoded);
                packet.Add(0x00);

                // TYPE NBSTAT (0x0021) and CLASS IN (0x0001)
                packet.Add(0x00); packet.Add(0x21);
                packet.Add(0x00); packet.Add(0x01);

                udp.Send(packet.ToArray(), packet.Count);
                var remote = new IPEndPoint(IPAddress.Any, 0);
                var resp = udp.Receive(ref remote);
                if (resp == null || resp.Length < 12) return string.Empty;

                // Validate transaction ID and that it is a response
                if (resp[0] != (byte)(txId >> 8) || resp[1] != (byte)(txId & 0xFF))
                    return string.Empty;
                // Flags bit 15 should be 1 for response
                bool isResponse = (resp[2] & 0x80) != 0;
                if (!isResponse) return string.Empty;

                // Parse NBSTAT name table for UNIQUE <00>
                for (int i = 0; i + 2 < resp.Length; i++)
                {
                    byte count = resp[i];
                    if (count == 0 || count > 50) continue;
                    int entriesStart = i + 1;
                    if (entriesStart + count * 18 > resp.Length) continue;
                    for (int k = 0; k < count; k++)
                    {
                        int off = entriesStart + k * 18;
                        var nameChars = new byte[15];
                        Array.Copy(resp, off, nameChars, 0, 15);
                        string rawName = System.Text.Encoding.ASCII.GetString(nameChars).Trim();
                        byte suffix = resp[off + 15];
                        ushort flags = (ushort)((resp[off + 16] << 8) | resp[off + 17]);
                        bool unique = (flags & 0x8000) != 0;
                        if (unique && suffix == 0x00)
                        {
                            if (!string.IsNullOrWhiteSpace(rawName) && !IsGeneric(rawName))
                                return rawName;
                        }
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool IsGeneric(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            var n = name.Trim();
            return n.Equals("Ethernet", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("__MSBROWSE__", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("*") || n.Equals("GROUP", StringComparison.OrdinalIgnoreCase);
        }

        private static byte[] EncodeNetbiosName(byte[] name16)
        {
            // RFC 1002: each 4-bit nibble of the 16-byte name maps to a letter 'A'..'P'
            var outBuf = new byte[32];
            int j = 0;
            for (int i = 0; i < 16; i++)
            {
                byte b = name16[i];
                byte high = (byte)((b >> 4) & 0x0F);
                byte low = (byte)(b & 0x0F);
                outBuf[j++] = (byte)('A' + high);
                outBuf[j++] = (byte)('A' + low);
            }
            return outBuf;
        }
    }
}
