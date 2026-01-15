using System.Diagnostics;

namespace NetworkUtilityApp.Controllers
{
    /// <summary>
    /// NetworkController partial: adapter configuration via netsh (DHCP/Static).
    /// </summary>
    public partial class NetworkController
    {
        public static string SetDhcp(string adapterName)
        {
            if (!IsAdministrator())
                return "[ERROR] Administrator privileges required. Run the app as Administrator.";
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface ip set address \"{adapterName}\" dhcp",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi)!;
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (!string.IsNullOrWhiteSpace(error))
                    return $"[ERROR] Failed to set DHCP on {adapterName}: {error.Trim()}";
                return $"[SUCCESS] DHCP enabled on {adapterName}\n{output.Trim()}";
            }
            catch (Exception ex) { return $"[ERROR] Exception while setting DHCP: {ex.Message}"; }
        }

        public static string SetStatic(string adapterName, string ip, string subnet, string gateway)
        {
            if (!IsAdministrator())
                return "[ERROR] Administrator privileges required. Run the app as Administrator.";
            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(subnet))
                return "[ERROR] IP and Subnet are required for static configuration.";
            try
            {
                var args = string.IsNullOrWhiteSpace(gateway)
                    ? $"interface ip set address \"{adapterName}\" static {ip} {subnet} none"
                    : $"interface ip set address \"{adapterName}\" static {ip} {subnet} {gateway} 1";
                var psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi)!;
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (!string.IsNullOrWhiteSpace(error))
                    return $"[ERROR] Failed to set Static IP on {adapterName}: {error.Trim()}".Replace(" 1", string.Empty);
                return $"[SUCCESS] Static IP set on {adapterName} — IP: {ip}, Subnet: {subnet}, Gateway: {gateway}" +
                       (string.IsNullOrWhiteSpace(output) ? string.Empty : $"\n{output.Trim()}");
            }
            catch (Exception ex) { return $"[ERROR] Exception while setting Static IP: {ex.Message}"; }
        }
    }
}
