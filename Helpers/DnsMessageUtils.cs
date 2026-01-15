using System.Net;

namespace NetworkUtilityApp.Helpers
{
    /// <summary>
    /// Shared DNS message utilities for building queries and parsing names.
    /// </summary>
    internal static class DnsMessageUtils
    {
        // Build a minimal DNS query packet for the given name and record type.
        public static byte[] BuildDnsQuery(string qname, ushort type)
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

        // Skip over a DNS name (handles compression pointers) and return the offset after it.
        public static int SkipName(byte[] buf, int off)
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

        // Read a DNS name following limited compression jumps.
        public static string ReadName(byte[] buf, int off, int maxJumps = 10)
        {
            var labels = new List<string>();
            int i = off;
            int jumps = 0;
            while (i < buf.Length && jumps < maxJumps)
            {
                byte len = buf[i++];
                if (len == 0) break;
                if ((len & 0xC0) == 0xC0)
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
    }
}
