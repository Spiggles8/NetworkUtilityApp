using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace NetworkUtilityApp.Controllers
{
    /// <summary>
    /// Lightweight controller that exposes common network operations used by the UI.
    ///
    /// Responsibilities:
    /// - Enumerate network adapters and report a small set of IPv4 properties
    /// - Configure adapter addressing using Windows 'netsh' (DHCP / Static)
    ///
    /// </summary>
    public sealed class NetworkAdapterInfo
    {
        // Simple POCO used by the UI. Initialized with empty strings to avoid null checks.
        public string AdapterName { get; set; } = string.Empty;
        public string IsDhcp { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Subnet { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string HardwareDetails { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
    }

    public partial class NetworkController
    {
        // -----------------------
        // Helper utilities
        // -----------------------

        /// <summary>
        /// Returns true when the current process is running with Administrator privileges.
        /// Important: calling netsh to change adapter configuration requires elevation.
        /// </summary>
        private static bool IsAdministrator()
        {
            // Get the Windows identity for the current thread/process.
            using var id = WindowsIdentity.GetCurrent();
            // Build a principal (role-based security object) from that identity.
            var pr = new WindowsPrincipal(id);
            // Check whether the principal is in the built-in Administrator role.
            return pr.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // -----------------------
        // Adapter enumeration
        // -----------------------

        /// <summary>
        /// Enumerates local network interfaces and constructs a lightweight summary
        /// containing the adapter's name, DHCP state, IPv4 address, subnet mask, gateway,
        /// operational status, description, and MAC address.
        ///
        /// </summary>
        public static List<NetworkAdapterInfo> GetAdapters()
        {
            // Prepare the list we will return to the caller (UI).
            var adaptersList = new List<NetworkAdapterInfo>();

            try
            {
                // GetAllNetworkInterfaces returns every network interface known to the OS.
                // We iterate them and extract a compact set of properties for the UI.
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // Obtain IP-related properties for this interface (addresses, gateways, etc.)
                    var props = nic.GetIPProperties();

                    // Select the first IPv4 unicast address (if any).
                    // UnicastAddresses includes IPv4 and IPv6 addresses; we filter by AddressFamily.
                    var ipv4 = props.UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    // Select the first IPv4 gateway (if any).
                    var gateway = props.GatewayAddresses
                        .FirstOrDefault(g => g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    // Create a small DTO for the UI and add it to the list.
                    adaptersList.Add(new NetworkAdapterInfo
                    {
                        AdapterName = nic.Name,
                        IsDhcp = props.GetIPv4Properties()?.IsDhcpEnabled == true ? "DHCP" : "STATIC",
                        IpAddress = ipv4?.Address.ToString() ?? string.Empty,
                        Subnet = ipv4?.IPv4Mask?.ToString() ?? string.Empty,
                        Gateway = gateway?.Address.ToString() ?? string.Empty,
                        Status = nic.OperationalStatus.ToString(),
                        HardwareDetails = nic.Description,
                        // Normalize MAC as colon-delimited uppercase (AA:BB:CC:DD:EE:FF)
                        MacAddress = NormalizeMac(nic.GetPhysicalAddress().ToString())
                    });
                }
            }
            catch (Exception ex)
            {
                // If something fails, return a single "Error" result so the UI can show the message.
                adaptersList.Add(new NetworkAdapterInfo
                {
                    AdapterName = "Error",
                    Status = $"Failed to enumerate adapters: {ex.Message}"
                });
            }
            return adaptersList;
        }

        // Normalize MAC strings from various formats into colon-delimited uppercase.
        private static string NormalizeMac(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var hex = new string(raw.Where(c => Uri.IsHexDigit(c)).ToArray());
            if (hex.Length < 12) return raw; // fallback to original if unexpected
            hex = hex.Substring(0, 12).ToUpperInvariant();
            return string.Join(":", Enumerable.Range(0, 6).Select(i => hex.Substring(i * 2, 2)));
        }

        // -----------------------
        // Adapter configuration (netsh)
        // -----------------------

        /// <summary>
        /// Enables DHCP on the named adapter by invoking the Windows 'netsh' tool.
        /// Returns a textual status message (success or error).
        /// </summary>
        public static string SetDhcp(string adapterName)
        {
            // Guard: changing adapter settings requires admin rights. Return informative message if not elevated.
            if (!IsAdministrator())
                return "[ERROR] Administrator privileges required. Run the app as Administrator.";

            try
            {
                // Prepare ProcessStartInfo to run netsh and capture output streams.
                // UseShellExecute=false is required to redirect stdout/stderr.
                var psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface ip set address \"{adapterName}\" dhcp",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Start the process, read both output streams fully, and wait for exit.
                using var process = Process.Start(psi)!;
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // If netsh wrote anything to stderr consider it an error and return it to UI.
                if (!string.IsNullOrWhiteSpace(error))
                    return $"[ERROR] Failed to set DHCP on {adapterName}: {error.Trim()}";

                return $"[SUCCESS] DHCP enabled on {adapterName}\n{output.Trim()}";
            }
            catch (Exception ex)
            {
                // Catch unexpected exceptions and return an error message (avoid throwing from UI threads).
                return $"[ERROR] Exception while setting DHCP: {ex.Message}";
            }
        }

        /// <summary>
        /// Sets a static IPv4 address on the named adapter using Windows 'netsh'.
        /// Arguments: adapterName, ip, subnetMask, gateway.
        /// Returns a textual status message (success or error).
        /// </summary>
        public static string SetStatic(string adapterName, string ip, string subnet, string gateway)
        {
            // Guard for elevation just like SetDhcp.
            if (!IsAdministrator())
                return "[ERROR] Administrator privileges required. Run the app as Administrator.";

            // Friendly validation so host can post empty values and get clear feedback
            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(subnet))
                return "[ERROR] IP and Subnet are required for static configuration.";

            try
            {
                // If gateway is empty, tell netsh 'none' and omit metric
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

                // Execute and capture output/errors.
                using var process = Process.Start(psi)!;
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // If netsh reported an error, return it for the UI to display.
                if (!string.IsNullOrWhiteSpace(error))
                    return $"[ERROR] Failed to set Static IP on {adapterName}: {error.Trim()}".Replace(" 1", string.Empty);

                // Otherwise return a success message including the requested parameters.
                return $"[SUCCESS] Static IP set on {adapterName} — IP: {ip}, Subnet: {subnet}, Gateway: {gateway}" +
                       (string.IsNullOrWhiteSpace(output) ? string.Empty : $"\n{output.Trim()}");
            }
            catch (Exception ex)
            {
                return $"[ERROR] Exception while setting Static IP: {ex.Message}";
            }
        }

        // -----------------------
        // Network diagnostics
        // -----------------------

        /// <summary>
        /// Sends a single ICMP echo (ping) and returns a short textual summary.
        /// Timeout is 2000ms. Exceptions are caught and returned as error strings.
        /// </summary>
        public static string PingHost(string ipAddress)
        {
            try
            {
                // Create a Ping instance and send an ICMP echo with a 2 second timeout.
                using var ping = new Ping();
                var reply = ping.Send(ipAddress, 2000); // 2s timeout

                // Inspect the reply status and build a small result string.
                if (reply.Status == IPStatus.Success)
                {
                    return $"[PING SUCCESS] {ipAddress} responded in {reply.RoundtripTime}ms (TTL={reply.Options?.Ttl})";
                }
                else
                {
                    return $"[PING FAIL] {ipAddress} - {reply.Status}";
                }
            }
            catch (Exception ex)
            {
                // Return error text rather than throwing (keeps UI code simple).
                return $"[ERROR] Ping failed: {ex.Message}";
            }
        }

        // -------- Traceroute models --------

        /// <summary>
        /// Represents a single hop returned by traceroute.
        /// RTT values are nullable because tracert reports '*' on timeout.
        /// </summary>
        public sealed class TraceHop
        {
            public int Hop { get; init; }
            public int? Rtt1Ms { get; init; }
            public int? Rtt2Ms { get; init; }
            public int? Rtt3Ms { get; init; }
            public string HostnameOrAddress { get; init; } = string.Empty;
            public bool TimedOut { get; init; }
        }

        /// <summary>
        /// Container for parsed traceroute output including raw output for debugging.
        /// </summary>
        public sealed class TraceResult
        {
            // Hops is initially empty; callers can inspect RawOutput for full command text.
            public List<TraceHop> Hops { get; } = new List<TraceHop>();
            public string RawOutput { get; init; } = string.Empty;
            public string Target { get; init; } = string.Empty;
        }

        /// <summary>
        /// Runs Windows 'tracert' and parses the textual output into TraceHop entries.
        /// This parsing uses a regular expression tuned for the typical English tracert output.
        /// Localized OS output can break the regex — treat parsing results as best-effort.
        /// </summary>
        /// <remarks>
        /// Caller should run this off the UI thread to avoid blocking the UI while tracert runs.
        /// </remarks>
        public static TraceResult Traceroute(
            string target,
            int maxHops = 30,
            int timeoutPerHopMs = 4000,
            bool resolveNames = true)
        {
            // Validate input early to avoid running tracert with an empty target.
            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentException("Target is required.", nameof(target));

            // Build the list of command-line arguments for tracert.
            // Use -d to skip DNS resolution when resolveNames == false.
            var args = new List<string>();
            if (!resolveNames) args.Add("-d");
            args.Add("-h"); args.Add(maxHops.ToString());
            args.Add("-w"); args.Add(timeoutPerHopMs.ToString());
            args.Add(target);

            // Configure ProcessStartInfo to run tracert and capture output.
            var psi = new ProcessStartInfo("tracert", string.Join(" ", args))
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            string output;
            // Start the process and read all output. Caller must run off UI thread to avoid blocking.
            using (var p = Process.Start(psi)!)
            {
                // Read all stdout; for very long traces consider streaming line-by-line.
                output = p.StandardOutput.ReadToEnd();
                // Some Windows builds write informational text to stderr; read to drain the stream.
                _ = p.StandardError.ReadToEnd();
                p.WaitForExit();
            }

            // Prepare the return container including the raw text for debugging.
            var result = new TraceResult { RawOutput = output, Target = target };

            // Use the source-generated compiled regex to parse each hop line.
            var hopRegex = MyRegex();

            // Split output into non-empty lines and process each.
            foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                // Attempt to match the expected tracert hop line format.
                var m = hopRegex.Match(line);
                if (!m.Success) continue;

                // Parse hop number from capture group 1; skip if it isn't an integer.
                if (!int.TryParse(m.Groups[1].Value, out var hop)) continue;

                // Local helper: parse a single RTT token which might be "*", "<1 ms" or "3 ms".
                static int? ParseRtt(string s)
                {
                    s = s.Trim();
                    // '*' or "Request timed out." indicates no reply -> null RTT.
                    if (s == "*" || s.Equals("Request timed out.", StringComparison.OrdinalIgnoreCase))
                        return null;
                    // Remove '<' and 'ms' then try parse the integer milliseconds value.
                    s = s.Replace("<", "").Replace("ms", "", StringComparison.OrdinalIgnoreCase).Trim();
                    return int.TryParse(s, out var v) ? v : (int?)null;
                }

                // Parse the three RTT samples from capture groups 2/3/4.
                var rtt1 = ParseRtt(m.Groups[2].Value);
                var rtt2 = ParseRtt(m.Groups[3].Value);
                var rtt3 = ParseRtt(m.Groups[4].Value);

                // Tail group contains either hostname/address or "Request timed out." text.
                var tail = m.Groups[5].Value.Trim();
                // Mark timedOut when the tail contains the phrase "timed out" (case-insensitive).
                var timedOut = tail.Contains("timed out", StringComparison.OrdinalIgnoreCase);

                // Add parsed hop to the result list.
                result.Hops.Add(new TraceHop
                {
                    Hop = hop,
                    Rtt1Ms = rtt1,
                    Rtt2Ms = rtt2,
                    Rtt3Ms = rtt3,
                    HostnameOrAddress = tail,
                    TimedOut = timedOut
                });
            }
            return result;
        }

        // Use the GeneratedRegex attribute (available on modern .NET) to produce a compiled regex
        // at compile-time rather than building/compiling it at runtime.
        [GeneratedRegex(@"^\s*(\d+)\s+(\*|<*\d+\s*ms)\s+(\*|<*\d+\s*ms)\s+(\*|<*\d+\s*ms)\s+(.+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex MyRegex();
    }
}