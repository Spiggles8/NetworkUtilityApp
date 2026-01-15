using System.Net;

namespace NetworkUtilityApp.Helpers
{
    /// <summary>
    /// ValidationHelper partial: basic IPv4 validations and utilities.
    /// </summary>
    public static partial class ValidationHelper
    {
        // True if the string is a valid IPv4 dotted-quad (0-255 per octet).
        public static bool IsValidIPv4(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;
            if (!IPAddress.TryParse(ip, out var addr)) return false;
            return addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }

        // True if each string represents an octet 0..255 (expects exactly four strings).
        public static bool AreValidOctets(params string?[] octets)
        {
            if (octets is null || octets.Length != 4) return false;
            foreach (var o in octets)
            {
                if (!IsValidOctet(o)) return false;
            }
            return true;
        }

        // Validates a single IPv4 octet (0..255).
        public static bool IsValidOctet(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (!int.TryParse(s, out var v)) return false;
            return v >= 0 && v <= 255;
        }

        // Joins four octet strings into a dotted IP (assumes already validated) and trims leading zeros.
        public static string JoinOctets(string o1, string o2, string o3, string o4)
            => $"{TrimLeadingZeros(o1)}.{TrimLeadingZeros(o2)}.{TrimLeadingZeros(o3)}.{TrimLeadingZeros(o4)}";

        private static string TrimLeadingZeros(string s)
            => int.TryParse(s, out var n) ? n.ToString() : s ?? string.Empty;

        // Optional conversion helpers
        public static string UIntToIPv4(uint value)
        {
            var b1 = (value >> 24) & 0xFF;
            var b2 = (value >> 16) & 0xFF;
            var b3 = (value >> 8) & 0xFF;
            var b4 = (value) & 0xFF;
            return $"{b1}.{b2}.{b3}.{b4}";
        }
    }
}
