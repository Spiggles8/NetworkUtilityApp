using System.Diagnostics;

namespace NetworkUtilityApp.Controllers
{
    /// <summary>
    /// NetworkController partial: diagnostics (Ping, Traceroute).
    /// </summary>
    public partial class NetworkController
    {
        public static string PingHost(string ipAddress)
        {
            try
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = ping.Send(ipAddress, 2000);
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    return $"[PING SUCCESS] {ipAddress} responded in {reply.RoundtripTime}ms (TTL={reply.Options?.Ttl})";
                else
                    return $"[PING FAIL] {ipAddress} - {reply.Status}";
            }
            catch (Exception ex) { return $"[ERROR] Ping failed: {ex.Message}"; }
        }

        public sealed class TraceHop
        {
            public int Hop { get; init; }
            public int? Rtt1Ms { get; init; }
            public int? Rtt2Ms { get; init; }
            public int? Rtt3Ms { get; init; }
            public string HostnameOrAddress { get; init; } = string.Empty;
            public bool TimedOut { get; init; }
        }

        public sealed class TraceResult
        {
            public List<TraceHop> Hops { get; } = [];
            public string RawOutput { get; init; } = string.Empty;
            public string Target { get; init; } = string.Empty;
        }

        public static TraceResult Traceroute(string target, int maxHops = 30, int timeoutPerHopMs = 4000, bool resolveNames = true)
        {
            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentException("Target is required.", nameof(target));
            var args = new List<string>();
            if (!resolveNames) args.Add("-d");
            args.Add("-h"); args.Add(maxHops.ToString());
            args.Add("-w"); args.Add(timeoutPerHopMs.ToString());
            args.Add(target);
            var psi = new ProcessStartInfo("tracert", string.Join(" ", args))
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            string output;
            using (var p = Process.Start(psi)!)
            {
                output = p.StandardOutput.ReadToEnd();
                _ = p.StandardError.ReadToEnd();
                p.WaitForExit();
            }
            var result = new TraceResult { RawOutput = output, Target = target };
            var hopRegex = MyRegex();
            foreach (var line in output.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries))
            {
                var m = hopRegex.Match(line);
                if (!m.Success) continue;
                if (!int.TryParse(m.Groups[1].Value, out var hop)) continue;
                static int? ParseRtt(string s)
                {
                    s = s.Trim();
                    if (s == "*" || s.Equals("Request timed out.", StringComparison.OrdinalIgnoreCase)) return null;
                    s = s.Replace("<", "").Replace("ms", "", StringComparison.OrdinalIgnoreCase).Trim();
                    return int.TryParse(s, out var v) ? v : (int?)null;
                }
                var rtt1 = ParseRtt(m.Groups[2].Value);
                var rtt2 = ParseRtt(m.Groups[3].Value);
                var rtt3 = ParseRtt(m.Groups[4].Value);
                var tail = m.Groups[5].Value.Trim();
                var timedOut = tail.Contains("timed out", StringComparison.OrdinalIgnoreCase);
                result.Hops.Add(new TraceHop { Hop = hop, Rtt1Ms = rtt1, Rtt2Ms = rtt2, Rtt3Ms = rtt3, HostnameOrAddress = tail, TimedOut = timedOut });
            }
            return result;
        }
    }
}
