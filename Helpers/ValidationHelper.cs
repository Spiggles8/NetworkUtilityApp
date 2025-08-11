using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NetworkUtilityApp.Helpers
{
    /// <summary>
    /// IPv4 and network-related validation & parsing helpers.
    /// </summary>
    public static class ValidationHelper
    {
        // =========================
        // Basic IPv4 validations
        // =========================

        /// <summary>
        /// True if the string is a valid IPv4 dotted-quad (0-255 per octet).
        /// </summary>
        public static bool IsValidIPv4(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;
            if (!IPAddress.TryParse(ip, out var addr)) return false;
            return addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }

        /// <summary>
        /// True if each string represents an octet 0..255.
        /// </summary>
        public static bool AreValidOctets(params string?[] octets)
        {
            if (octets is null || octets.Length != 4) return false;
            foreach (var o in octets)
            {
                if (!IsValidOctet(o)) return false;
            }
            return true;
        }

        /// <summary>
        /// Validates a single IPv4 octet (0..255).
        /// </summary>
        public static bool IsValidOctet(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (!int.TryParse(s, out var v)) return false;
            return v >= 0 && v <= 255;
        }

        /// <summary>
        /// Joins four octet strings into a dotted IP (assumes already validated).
        /// </summary>
        public static string JoinOctets(string o1, string o2, string o3, string o4)
            => $"{TrimLeadingZeros(o1)}.{TrimLeadingZeros(o2)}.{TrimLeadingZeros(o3)}.{TrimLeadingZeros(o4)}";

        private static string TrimLeadingZeros(string s)
            => int.TryParse(s, out var n) ? n.ToString() : s ?? string.Empty;

        // =========================
        // Subnet mask & gateway
        // =========================

        /// <summary>
        /// True if mask is a valid dotted-quad AND is a contiguous subnet mask (e.g., 255.255.255.0).
        /// </summary>
        public static bool IsValidSubnetMask(string? mask)
        {
            if (!IsValidIPv4(mask)) return false;
            if (!TryParseIPv4(mask!, out var m)) return false;

            // Count leading ones, then ensure the rest are zero.
            int prefix = 0;
            for (int i = 31; i >= 0; i--)
            {
                if (((m >> i) & 1) == 1) prefix++;
                else break;
            }
            uint expected = prefix == 0 ? 0u : 0xFFFFFFFFu << (32 - prefix);
            return m == expected;
        }

        /// <summary>
        /// Gateway is optional: returns true when empty OR valid IPv4.
        /// </summary>
        public static bool IsValidGateway(string? gateway)
            => string.IsNullOrWhiteSpace(gateway) || IsValidIPv4(gateway);

        // =========================
        // CIDR & ranges
        // =========================

        /// <summary>
        /// Validates CIDR like "192.168.1.0/24". Returns false if invalid.
        /// </summary>
        public static bool IsValidCidr(string? cidr)
            => TryParseCidr(cidr, out _, out _, out _);

        /// <summary>
        /// Parses CIDR to start/end (inclusive) and prefix. Excludes network/broadcast for /0..30; includes for /31,/32.
        /// </summary>
        public static bool TryParseCidr(string? cidr, out uint start, out uint end, out int prefix)
        {
            start = end = 0;
            prefix = 0;
            if (string.IsNullOrWhiteSpace(cidr)) return false;

            var parts = cidr.Split('/');
            if (parts.Length != 2) return false;
            if (!TryParseIPv4(parts[0].Trim(), out var baseIp)) return false;
            if (!int.TryParse(parts[1].Trim(), out prefix) || prefix < 0 || prefix > 32) return false;

            uint mask = PrefixToMask(prefix);
            uint network = baseIp & mask;
            uint broadcast = network | ~mask;

            if (prefix >= 31) { start = network; end = broadcast; }
            else { start = network + 1; end = broadcast - 1; }

            if (start > end) { start = end = network; }
            return true;
        }

        /// <summary>
        /// From IP + mask, compute start/end and prefix. Returns false if inputs invalid or mask non-contiguous.
        /// </summary>
        public static bool TryGetNetworkRange(string ip, string mask, out uint start, out uint end, out int prefix)
        {
            start = end = 0;
            prefix = SubnetMaskToPrefix(mask);
            if (prefix < 0) return false;
            if (!TryParseIPv4(ip, out var ipU)) return false;

            var maskU = PrefixToMask(prefix);
            var network = ipU & maskU;
            var broadcast = network | ~maskU;

            if (prefix >= 31) { start = network; end = broadcast; }
            else { start = network + 1; end = broadcast - 1; }

            if (start > end) { start = end = network; }
            return true;
        }

        /// <summary>
        /// Converts dotted mask to prefix length (e.g., 255.255.255.0 → 24). Returns -1 if non-contiguous/invalid.
        /// </summary>
        public static int SubnetMaskToPrefix(string? mask)
        {
            if (!IsValidIPv4(mask)) return -1;
            if (!TryParseIPv4(mask!, out var m)) return -1;

            int prefix = 0;
            for (int i = 31; i >= 0; i--)
            {
                if (((m >> i) & 1) == 1) prefix++;
                else break;
            }
            uint expected = prefix == 0 ? 0u : 0xFFFFFFFFu << (32 - prefix);
            return m == expected ? prefix : -1;
        }

        /// <summary>
        /// Converts prefix to a 32-bit mask.
        /// </summary>
        public static uint PrefixToMask(int prefix)
        {
            if (prefix <= 0) return 0u;
            if (prefix >= 32) return 0xFFFFFFFFu;
            return 0xFFFFFFFFu << (32 - prefix);
        }

        // =========================
        // Parsing helpers
        // =========================

        /// <summary>
        /// Parse dotted IPv4 to UInt32 (network order to uint).
        /// </summary>
        public static bool TryParseIPv4(string dotted, out uint value)
        {
            value = 0;
            if (!IPAddress.TryParse(dotted, out var ip) ||
                ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return false;

            var b = ip.GetAddressBytes(); // big-endian
            value = ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
            return true;
        }

        /// <summary>
        /// Converts UInt32 to dotted IPv4.
        /// </summary>
        public static string ToIPv4(uint v)
        {
            var b1 = (byte)((v >> 24) & 0xFF);
            var b2 = (byte)((v >> 16) & 0xFF);
            var b3 = (byte)((v >> 8) & 0xFF);
            var b4 = (byte)(v & 0xFF);
            return new IPAddress(new[] { b1, b2, b3, b4 }).ToString();
        }

        /// <summary>
        /// Parses a comma/space/semicolon-separated list of ports (1..65535), deduped & sorted.
        /// </summary>
        public static List<int> ParsePortList(string? input)
        {
            var list = new List<int>();
            if (string.IsNullOrWhiteSpace(input)) return list;

            foreach (var part in input.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part.Trim(), out var p) && p >= 1 && p <= 65535)
                    list.Add(p);
            }
            return list.Distinct().OrderBy(p => p).ToList();
        }

        // =========================
        // High-level validators used by UI
        // =========================

        /// <summary>
        /// Validates a full static config. Gateway is optional (allowEmptyGateway=true).
        /// Returns (ok, errorMessage).
        /// </summary>
        public static (bool ok, string error) ValidateStaticConfig(string ip, string subnet, string? gateway, bool allowEmptyGateway = true)
        {
            if (!IsValidIPv4(ip)) return (false, "Invalid IP address.");
            if (!IsValidSubnetMask(subnet)) return (false, "Invalid or non-contiguous subnet mask.");

            if (!allowEmptyGateway || !string.IsNullOrWhiteSpace(gateway))
            {
                if (!IsValidGateway(gateway)) return (false, "Invalid gateway address.");
                // Optional: ensure gateway is in same subnet as IP (except empty)
                if (!string.IsNullOrWhiteSpace(gateway))
                {
                    if (!TryParseIPv4(ip, out var ipU) ||
                        !TryParseIPv4(subnet, out var maskU) ||
                        !TryParseIPv4(gateway!, out var gwU))
                    {
                        return (false, "Failed to parse IP/subnet/gateway.");
                    }

                    // In same subnet?
                    if ((ipU & maskU) != (gwU & maskU))
                        return (false, "Gateway is not in the same subnet as the IP address.");
                }
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Validates four IP octets and returns the dotted IP if valid.
        /// </summary>
        public static (bool ok, string ipOrError) ValidateAndBuildIp(string o1, string o2, string o3, string o4)
        {
            if (!AreValidOctets(o1, o2, o3, o4)) return (false, "Each IP octet must be a number 0..255.");
            return (true, JoinOctets(o1, o2, o3, o4));
        }
    }
}
