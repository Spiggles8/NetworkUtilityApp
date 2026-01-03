using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Helpers;
using NetworkUtilityApp.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace NetworkUtilityApp.Tabs
{
    /// <summary>
    /// Network discovery tab. Scans an IPv4 range, pings hosts and
    /// enriches results with hostname, MAC address and vendor info.
    /// </summary>
    public partial class TabDiscovery : UserControl
    {
        // Cancellation token for the active scan loop (if any)
        private CancellationTokenSource? _scanCts;
        // Stopwatch used to estimate ETA while scanning
        private Stopwatch? _scanSw;

        public TabDiscovery()
        {
            InitializeComponent();
            if (IsDesignMode()) return;

            // Wire UI events
            btnAutofill.Click += (_, __) => AutofillRange();
            btnScan.Click += async (_, __) => await StartScanAsync();
            btnCancel.Click += (_, __) => CancelScan();
            btnSave.Click += async (_, __) => await SaveResultsAsync();

            cboAdapter.SelectionChangeCommitted += (_, __) => AutofillRange();
            LoadAdapters();
        }

        private bool IsDesignMode()
            => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        // Populate adapter combo with IPv4 adapters from the controller.
        private void LoadAdapters()
        {
            try
            {
                var adapters = NetworkController.GetAdapters() ?? [];
                var ipv4 = adapters
                    .Where(a => !string.IsNullOrWhiteSpace(a.IpAddress) && ValidationHelper.IsValidIPv4(a.IpAddress))
                    .ToList();

                cboAdapter.Items.Clear();
                foreach (var a in ipv4)
                    cboAdapter.Items.Add(a.AdapterName + " | " + a.IpAddress + " / " + a.Subnet);

                if (cboAdapter.Items.Count > 0)
                {
                    cboAdapter.SelectedIndex = 0;
                    AutofillRange();
                }
            }
            catch (Exception ex)
            {
                AppendLog("[ERROR] Failed to load adapters: " + ex.Message);
            }
        }

        /// <summary>
        /// Derive a reasonable start/end range from the currently
        /// selected adapter's IP and subnet.
        /// </summary>
        private void AutofillRange()
        {
            var sel = cboAdapter.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(sel)) return;

            var parts = sel.Split('|');
            if (parts.Length < 2) return;

            var ipSubnet = parts[1].Trim().Split('/');
            if (ipSubnet.Length < 2) return;

            var ip = ipSubnet[0].Trim();
            var subnet = ipSubnet[1].Trim();
            if (!ValidationHelper.IsValidIPv4(ip) || !ValidationHelper.IsValidIPv4(subnet)) return;

            var networkBase = GetNetworkBase(ip, subnet);
            if (networkBase is null) return;

            // Simple heuristic: assume /24 and use .1 - .254
            txtStartIp.Text = networkBase + ".1";
            txtEndIp.Text = networkBase + ".254";
        }

        /// <summary>
        /// Compute the network base address (first 3 octets) for an
        /// IPv4 address + subnet mask.
        /// </summary>
        private static string? GetNetworkBase(string ip, string subnet)
        {
            try
            {
                var ipOct = ip.Split('.');
                var maskOct = subnet.Split('.');
                if (ipOct.Length != 4 || maskOct.Length != 4) return null;

                var baseParts = new string[4];
                for (int i = 0; i < 4; i++)
                {
                    int ipPart = int.Parse(ipOct[i]);
                    int maskPart = int.Parse(maskOct[i]);
                    baseParts[i] = (ipPart & maskPart).ToString();
                }

                // Caller normally uses first 3 octets of this value
                return string.Join(".", baseParts.Take(3));
            }
            catch { return null; }
        }

        /// <summary>
        /// Main scan loop. Walks the IP range, pings each host and
        /// updates the grid + progress labels.
        /// </summary>
        private async Task StartScanAsync()
        {
            var start = txtStartIp.Text.Trim();
            var end = txtEndIp.Text.Trim();
            if (!ValidationHelper.IsValidIPv4(start) || !ValidationHelper.IsValidIPv4(end))
            {
                AppendLog("[ERROR] Invalid start or end IP.");
                return;
            }

            var startVal = IpToInt(start);
            var endVal = IpToInt(end);
            if (endVal < startVal)
            {
                AppendLog("[ERROR] End IP must be >= Start IP.");
                return;
            }

            if (_scanCts != null)
            {
                AppendLog("[INFO] Scan already running.");
                return;
            }

            // Disable inputs while the scan is running
            btnScan.Enabled = false;
            btnAutofill.Enabled = false;
            cboAdapter.Enabled = false;
            btnCancel.Enabled = true;

            _scanCts = new CancellationTokenSource();
            dgvResults.Rows.Clear();
            AppendLog($"[SCAN] IP scan beginning: {start} - {end}");

            try
            {
                var token = _scanCts.Token;
                int activeCount = 0;
                long totalLong = endVal - startVal + 1;
                int total = totalLong > int.MaxValue ? int.MaxValue : (int)totalLong;

                // Progress / ETA setup
                prgScan.Value = 0;
                prgScan.Maximum = total;
                lblProgressCounts.Text = $"Scanned: 0 / {total} | Active: 0";
                lblEta.Text = "ETA: --:--:--";
                _scanSw = Stopwatch.StartNew();

                // Warm ARP cache once before scanning
                var arpCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                LoadArpTableInto(arpCache);

                int scanned = 0;
                for (long ipVal = startVal; ipVal <= endVal; ipVal++)
                {
                    token.ThrowIfCancellationRequested();
                    string ipStr = IntToIp(ipVal);

                    var result = await ProbeAsync(ipStr, arpCache);
                    scanned++;

                    if (result.IsActive)
                    {
                        activeCount++;
                        dgvResults.Rows.Add(
                            result.Ip,
                            result.Hostname,
                            result.Mac,
                            result.Manufacturer,
                            result.LatencyMs?.ToString() ?? string.Empty,
                            result.Status
                        );
                    }

                    if (scanned <= prgScan.Maximum)
                        prgScan.Value = scanned;

                    UpdateProgressLabels(scanned, total, activeCount);

                    // Keep UI responsive during long scans
                    Application.DoEvents();
                }

                _scanSw?.Stop();
                UpdateProgressLabels(total, total, activeCount);
                AppendLog($"[SCAN] IP scan completed. Active hosts found: {activeCount}");
            }
            catch (OperationCanceledException)
            {
                AppendLog("[SCAN] Cancelled by user.");
            }
            catch (Exception ex)
            {
                AppendLog("[ERROR] Scan failed: " + ex.Message);
            }
            finally
            {
                _scanSw?.Stop();
                _scanSw = null;
                _scanCts?.Dispose();
                _scanCts = null;

                // Re-enable controls regardless of outcome
                btnScan.Enabled = true;
                btnAutofill.Enabled = true;
                cboAdapter.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        private void CancelScan()
        {
            if (_scanCts is null)
            {
                AppendLog("[INFO] No scan to cancel.");
                return;
            }

            _scanCts.Cancel();
        }

        /// <summary>
        /// Update progress counts and ETA label based on scanned count
        /// and elapsed scan time.
        /// </summary>
        private void UpdateProgressLabels(int scanned, int total, int active)
        {
            lblProgressCounts.Text = $"Scanned: {scanned} / {total} | Active: {active}";

            if (_scanSw is null || scanned == 0 || scanned >= total)
            {
                lblEta.Text = scanned >= total ? "ETA: 00:00:00" : "ETA: --:--:--";
                return;
            }

            var elapsed = _scanSw.Elapsed;
            double perItem = elapsed.TotalSeconds / scanned;
            var remaining = TimeSpan.FromSeconds(Math.Max(0, (total - scanned) * perItem));
            lblEta.Text = $"ETA: {FormatSpan(remaining)}";
        }

        private static string FormatSpan(TimeSpan ts)
            => $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";

        // Convert dotted IPv4 string to an integer for simple range math.
        private static long IpToInt(string ip)
        {
            var parts = ip.Split('.').Select(int.Parse).ToArray();
            return ((long)(uint)parts[0] << 24) | ((long)(uint)parts[1] << 16) | ((long)(uint)parts[2] << 8) | (uint)parts[3];
        }

        private static string IntToIp(long val)
            => string.Join('.', new[] { (val >> 24) & 255, (val >> 16) & 255, (val >> 8) & 255, val & 255 });

        /// <summary>
        /// Probe a single IP: ping, attempt hostname via several methods,
        /// and look up MAC + vendor from ARP.
        /// </summary>
        private static async Task<ProbeResult> ProbeAsync(string ip, Dictionary<string, string> arpCache)
        {
            var pr = new ProbeResult { Ip = ip, Status = "No Reply" };
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ip, 800);
                if (reply.Status == IPStatus.Success)
                {
                    pr.IsActive = true;
                    pr.Status = "Active";
                    pr.LatencyMs = reply.RoundtripTime;

                    // Try regular DNS first, then fall back to on-network resolvers
                    try
                    {
                        var host = await Dns.GetHostEntryAsync(ip);
                        pr.Hostname = host.HostName;
                    }
                    catch
                    {
                        pr.Hostname = LlmnrResolver.TryGetHostname(ip);
                        if (string.IsNullOrWhiteSpace(pr.Hostname)) pr.Hostname = MdnsResolver.TryGetHostname(ip);
                        if (string.IsNullOrWhiteSpace(pr.Hostname)) pr.Hostname = NbnsResolver.TryGetHostname(ip, 1200, null);
                        if (string.IsNullOrWhiteSpace(pr.Hostname)) pr.Hostname = TryResolveNetbiosName(ip);
                    }

                    // Look up MAC from ARP cache, refreshing once if needed
                    if (arpCache.TryGetValue(ip, out var mac))
                        pr.Mac = NormalizeMac(mac);
                    else
                    {
                        LoadArpTableInto(arpCache);
                        if (arpCache.TryGetValue(ip, out mac))
                            pr.Mac = NormalizeMac(mac);
                    }

                    // Map MAC prefix to vendor name
                    pr.Manufacturer = ResolveManufacturer(pr.Mac);
                }
            }
            catch { }

            return pr;
        }

        // Last-resort NetBIOS name lookup using the nbtstat tool.
        private static string TryResolveNetbiosName(string ip)
        {
            try
            {
                using var p = new Process
                {
                    StartInfo = new ProcessStartInfo
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
                    if (t.StartsWith("NetBIOS", StringComparison.OrdinalIgnoreCase)) continue;
                    if (t.StartsWith("Node ", StringComparison.OrdinalIgnoreCase)) continue;
                    if (t.StartsWith("Names", StringComparison.OrdinalIgnoreCase)) continue;
                    if (!t.Contains('<')) continue;
                    if (!t.Contains("<00>")) continue;

                    var idx = t.IndexOf('<');
                    if (idx > 0)
                    {
                        var name = t[..idx].Trim();
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        if (name.Equals("*", StringComparison.Ordinal)) continue;
                        if (name.Equals("Ethernet", StringComparison.OrdinalIgnoreCase)) continue;
                        if (name.Equals("__MSBROWSE__", StringComparison.OrdinalIgnoreCase)) continue;
                        return name;
                    }
                }
            }
            catch { }

            return string.Empty;
        }

        // Populate the given map with entries from the local ARP cache.
        private static void LoadArpTableInto(Dictionary<string, string> map)
        {
            try
            {
                using var p = new Process
                {
                    StartInfo = new ProcessStartInfo
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

                    var parts = trimmed.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && ValidationHelper.IsValidIPv4(parts[0]))
                        map[parts[0]] = NormalizeMac(parts[1]);
                }
            }
            catch { }
        }

        // Normalize MAC string to AA:BB:CC:DD:EE:FF form where possible.
        private static string NormalizeMac(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            var hex = new string([.. raw.Where(c => Uri.IsHexDigit(c))]);
            if (hex.Length < 12) return raw;

            hex = hex[..12].ToUpperInvariant();
            return string.Join(':', Enumerable.Range(0, 6).Select(i => hex.Substring(i * 2, 2)));
        }

        private static string ResolveManufacturer(string mac) => MacVendors.Lookup(mac);
        private static void AppendLog(string message) => AppLog.Info(message);

        // Save current results grid to a tab-delimited text file.
        private async Task SaveResultsAsync()
        {
            try
            {
                if (dgvResults.Rows.Count == 0)
                {
                    AppendLog("[INFO] Nothing to save. No results in the table.");
                    return;
                }

                using var sfd = new SaveFileDialog
                {
                    Title = "Save Network Discovery Results",
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FileName = $"NetworkDiscovery_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt",
                    AddExtension = true,
                    DefaultExt = "txt",
                    OverwritePrompt = true
                };

                if (sfd.ShowDialog(FindForm()) != DialogResult.OK) return;

                var lines = new List<string>
                {
                    $"Network Discovery Results - {DateTime.Now}",
                    string.IsNullOrWhiteSpace(txtStartIp.Text.Trim()) || string.IsNullOrWhiteSpace(txtEndIp.Text.Trim())
                        ? string.Empty
                        : $"Range: {txtStartIp.Text.Trim()} - {txtEndIp.Text.Trim()}",
                    string.Empty
                };

                var headers = dgvResults.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText).ToArray();
                lines.Add(string.Join('\t', headers));

                foreach (DataGridViewRow row in dgvResults.Rows)
                {
                    if (row.IsNewRow) continue;

                    var cells = row.Cells.Cast<DataGridViewCell>()
                        .Select(c => (c.Value?.ToString() ?? string.Empty)
                            .Replace('\t', ' ')
                            .Replace('\r', ' ')
                            .Replace('\n', ' '))
                        .ToArray();

                    lines.Add(string.Join('\t', cells));
                }

                await File.WriteAllLinesAsync(sfd.FileName, lines);
                AppendLog($"[SAVE] Discovery results saved to: {sfd.FileName}");
            }
            catch (Exception ex)
            {
                AppendLog("[ERROR] Failed to save discovery results: " + ex.Message);
            }
        }

        private sealed class ProbeResult
        {
            public string Ip { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public long? LatencyMs { get; set; }
            public string Hostname { get; set; } = string.Empty;
            public string Mac { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public string Status { get; set; } = "No Reply";
        }
    }
}