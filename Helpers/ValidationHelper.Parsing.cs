using System.Net;

namespace NetworkUtilityApp.Helpers
{
    /// <summary>
    /// ValidationHelper partial: parsing helpers.
    /// </summary>
    public static partial class ValidationHelper
    {
        // Parse dotted IPv4 to UInt32 (network order to uint).
        public static bool TryParseIPv4(string dotted, out uint value)
        {
            value = 0;
            if (!IPAddress.TryParse(dotted, out var ip) ||
                ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return false;
            var b = ip.GetAddressBytes();
            value = ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
            return true;
        }

        // Converts UInt32 to dotted IPv4.
        public static string ToIPv4(uint v)
        {
            var b1 = (byte)((v >> 24) & 0xFF);
            var b2 = (byte)((v >> 16) & 0xFF);
            var b3 = (byte)((v >> 8) & 0xFF);
            var b4 = (byte)(v & 0xFF);
            return new IPAddress([b1, b2, b3, b4]).ToString();
        }

        // Parses a comma/space/semicolon-separated list of ports (1..65535), deduped & sorted.
        public static List<int> ParsePortList(string? input)
        {
            var list = new List<int>();
            if (string.IsNullOrWhiteSpace(input)) return list;
            foreach (var part in input.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part.Trim(), out var p) && p >= 1 && p <= 65535)
                    list.Add(p);
            }
            return [.. list.Distinct().OrderBy(p => p)];
        }
    }
}
