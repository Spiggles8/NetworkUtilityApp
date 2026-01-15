using System.Diagnostics;
using System.Linq;
using NetworkUtilityApp.Models;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// DiscoveryView partial: probe a single IP and enrich with hostname, ARP, and vendor.
    /// </summary>
    public partial class DiscoveryView
    {
        private async Task<DiscoveryProbeRow> ProbeAsync(string ip)
        {
            var pr = new DiscoveryProbeRow { Ip = ip, Status = "No Reply" };
            try
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = await ping.SendPingAsync(ip, 400);
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    pr.IsActive = true;
                    pr.Status = "Active";
                    pr.LatencyMs = reply.RoundtripTime;

                    try { var host = await System.Net.Dns.GetHostEntryAsync(ip); pr.Hostname = host.HostName; }
                    catch
                    {
                        pr.Hostname = Helpers.LlmnrResolver.TryGetHostname(ip, 1200, null);
                        if (string.IsNullOrWhiteSpace(pr.Hostname)) pr.Hostname = Helpers.MdnsResolver.TryGetHostname(ip, 1500, null);
                        if (string.IsNullOrWhiteSpace(pr.Hostname)) pr.Hostname = Helpers.NbnsResolver.TryGetHostname(ip, 1200, null);
                        if (string.IsNullOrWhiteSpace(pr.Hostname)) pr.Hostname = TryResolveNetbiosName(ip);
                    }

                    if (_arpCache.TryGetValue(ip, out var mac)) pr.Mac = mac; else {
                        LoadArpTableInto(_arpCache);
                        if (_arpCache.TryGetValue(ip, out mac)) pr.Mac = mac;
                    }
                    pr.Manufacturer = Helpers.MacVendors.Lookup(pr.Mac);
                }
            }
            catch { }
            return pr;
        }

        private static string TryResolveNetbiosName(string ip)
        {
            try
            {
                using var p = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "nbtstat",
                        Arguments = $"-A {ip}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                _ = p.StandardError.ReadToEnd();
                p.WaitForExit(4000);
                foreach (var line in output.Split('\n'))
                {
                    var t = line.Trim();
                    if (string.IsNullOrWhiteSpace(t)) continue;
                    if (!t.Contains('<') || !t.Contains("<00>")) continue;
                    var idx = t.IndexOf('<');
                    if (idx > 0)
                    {
                        var name = t[..idx].Trim();
                        if (name.Length > 0 && name != "*" && !name.Equals("Ethernet", StringComparison.OrdinalIgnoreCase) && !name.Equals("__MSBROWSE__", StringComparison.OrdinalIgnoreCase))
                            return name;
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        private static void LoadArpTableInto(Dictionary<string,string> map)
        {
            try
            {
                using var p = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "arp",
                        Arguments = "-a",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(4000);
                foreach (var line in output.Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed)) continue;
                    var parts = trimmed.Split(new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && Helpers.ValidationHelper.IsValidIPv4(parts[0]))
                    {
                        var raw = parts[1];
                        var hex = new string([.. raw.Where(c => Uri.IsHexDigit(c))]);
                        if (hex.Length >= 12)
                        {
                            hex = hex[..12].ToUpperInvariant();
                            var mac = string.Join(":", Enumerable.Range(0,6).Select(i => hex.Substring(i*2,2)));
                            map[parts[0]] = mac;
                        }
                    }
                }
            }
            catch { }
        }
    }
}
