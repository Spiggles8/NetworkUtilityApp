namespace NetworkUtilityApp.Helpers
{
    /// <summary>
    /// ValidationHelper partial: subnet mask and CIDR operations.
    /// </summary>
    public static partial class ValidationHelper
    {
        // True if mask is a valid dotted-quad AND is a contiguous subnet mask.
        public static bool IsValidSubnetMask(string? mask)
        {
            if (!IsValidIPv4(mask)) return false;
            if (!TryParseIPv4(mask!, out var m)) return false;
            int prefix = 0;
            for (int i = 31; i >= 0; i--)
            {
                if (((m >> i) & 1) == 1) prefix++;
                else break;
            }
            uint expected = prefix == 0 ? 0u : 0xFFFFFFFFu << (32 - prefix);
            return m == expected;
        }

        // Gateway is optional: returns true when empty OR valid IPv4.
        public static bool IsValidGateway(string? gateway)
            => string.IsNullOrWhiteSpace(gateway) || IsValidIPv4(gateway);

        // Validates CIDR like "192.168.1.0/24".
        public static bool IsValidCidr(string? cidr)
            => TryParseCidr(cidr, out _, out _, out _);

        // Parses CIDR to start/end (inclusive) and prefix.
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

        // From IP + mask, compute start/end and prefix.
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

        // Converts dotted mask to prefix length.
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

        // Converts prefix to a 32-bit mask.
        public static uint PrefixToMask(int prefix)
        {
            if (prefix <= 0) return 0u;
            if (prefix >= 32) return 0xFFFFFFFFu;
            return 0xFFFFFFFFu << (32 - prefix);
        }
    }
}
