namespace NetworkUtilityApp.Helpers
{
    /// <summary>
    /// ValidationHelper partial: high-level validators used by UI.
    /// </summary>
    public static partial class ValidationHelper
    {
        // Validates a full static config. Gateway is optional.
        public static (bool ok, string error) ValidateStaticConfig(string ip, string subnet, string? gateway, bool allowEmptyGateway = true)
        {
            if (!IsValidIPv4(ip)) return (false, "Invalid IP address.");
            if (!IsValidSubnetMask(subnet)) return (false, "Invalid or non-contiguous subnet mask.");
            if (!allowEmptyGateway || !string.IsNullOrWhiteSpace(gateway))
            {
                if (!IsValidGateway(gateway)) return (false, "Invalid gateway address.");
                if (!string.IsNullOrWhiteSpace(gateway))
                {
                    if (!TryParseIPv4(ip, out var ipU) || !TryParseIPv4(subnet, out var maskU) || !TryParseIPv4(gateway!, out var gwU))
                        return (false, "Failed to parse IP/subnet/gateway.");
                    if ((ipU & maskU) != (gwU & maskU))
                        return (false, "Gateway is not in the same subnet as the IP address.");
                }
            }
            return (true, string.Empty);
        }

        // Validates four IP octets and returns the dotted IP if valid.
        public static (bool ok, string ipOrError) ValidateAndBuildIp(string o1, string o2, string o3, string o4)
        {
            if (!AreValidOctets(o1, o2, o3, o4)) return (false, "Each IP octet must be a number 0..255.");
            return (true, JoinOctets(o1, o2, o3, o4));
        }

        // (Optional) Convert dotted IPv4 to 32-bit (network order)
        public static bool TryParseIPv4ToUInt(string ip, out uint value)
        {
            value = 0;
            if (!IsValidIPv4(ip)) return false;
            var parts = ip.Split('.');
            uint p1 = uint.Parse(parts[0]);
            uint p2 = uint.Parse(parts[1]);
            uint p3 = uint.Parse(parts[2]);
            uint p4 = uint.Parse(parts[3]);
            value = (p1 << 24) | (p2 << 16) | (p3 << 8) | p4;
            return true;
        }
    }
}
