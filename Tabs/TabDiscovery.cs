using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Helpers;
using NetworkUtilityApp.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace NetworkUtilityApp.Tabs
{
    public partial class TabDiscovery : UserControl
    {
        private CancellationTokenSource? _scanCts;
        private Stopwatch? _scanSw; // NEW

        public TabDiscovery()
        {
            InitializeComponent();
            if (IsDesignMode()) return;

            btnAutofill.Click += (_, __) => AutofillRange();
            btnScan.Click += async (_, __) => await StartScanAsync();
            btnCancel.Click += (_, __) => CancelScan();
            btnSave.Click += async (_, __) => await SaveResultsAsync();

            // Update start/end when user picks a different adapter
            cboAdapter.SelectionChangeCommitted += (_, __) => AutofillRange();

            LoadAdapters();
        }

        private bool IsDesignMode()
            => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private void LoadAdapters()
        {
            try
            {
                var adapters = NetworkController.GetAdapters() ?? [];
                var ipv4 = adapters
                    .Where(a => !string.IsNullOrWhiteSpace(a.IpAddress) &&
                                ValidationHelper.IsValidIPv4(a.IpAddress))
                    .Select(a => a)
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
            // Simple /24 assumption fallback
            txtStartIp.Text = networkBase + ".1";
            txtEndIp.Text = networkBase + ".254";
        }

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
                return string.Join(".", baseParts.Take(3)); // assume /24 for range
            }
            catch { return null; }
        }

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

            // UI state
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

                // progress init
                prgScan.Value = 0;
                prgScan.Maximum = total;
                lblProgressCounts.Text = $"Scanned: 0 / {total} | Active: 0";
                lblEta.Text = "ETA: --:--:--";
                _scanSw = Stopwatch.StartNew();

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
                            result.LatencyMs?.ToString() ?? "",
                            result.Status
                        );
                    }

                    // progress update
                    if (scanned <= prgScan.Maximum) prgScan.Value = scanned;
                    UpdateProgressLabels(scanned, total, activeCount);

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

                // restore UI
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

        private void UpdateProgressLabels(int scanned, int total, int active) // NEW
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

        private static string FormatSpan(TimeSpan ts) // NEW
        {
            return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
        }

        private static long IpToInt(string ip)
        {
            var parts = ip.Split('.').Select(int.Parse).ToArray();
            return ((long)(uint)parts[0] << 24) | ((long)(uint)parts[1] << 16) | ((long)(uint)parts[2] << 8) | (uint)parts[3];
        }

        private static string IntToIp(long val)
            => string.Join(".", new[]
            {
                (val >> 24) & 255,
                (val >> 16) & 255,
                (val >> 8) & 255,
                val & 255
            });

        private static async Task<ProbeResult> ProbeAsync(string ip, Dictionary<string,string> arpCache)
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
                    try
                    {
                        var host = await Dns.GetHostEntryAsync(ip);
                        pr.Hostname = host.HostName;
                    }
                    catch { pr.Hostname = ""; }
                    if (arpCache.TryGetValue(ip, out var mac))
                        pr.Mac = mac;
                    else
                    {
                        // Refresh ARP for this IP
                        LoadArpTableInto(arpCache);
                        if (arpCache.TryGetValue(ip, out mac))
                            pr.Mac = mac;
                    }
                    pr.Manufacturer = ResolveManufacturer(pr.Mac);
                }
            }
            catch { /* ignore individual failures */ }
            return pr;
        }

        private static void LoadArpTableInto(Dictionary<string,string> map)
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
                // Parse lines containing IP and MAC
                foreach (var line in output.Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed)) continue;
                    // Expected format:  IP address  MAC address  Type
                    var parts = trimmed.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && ValidationHelper.IsValidIPv4(parts[0]))
                        map[parts[0]] = parts[1];
                }
            }
            catch { /* ignore */ }
        }

        // Basic OUI to manufacturer resolver; extend map as needed
        private static string ResolveManufacturer(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac)) return "";
            var hex = new string([.. mac.ToUpperInvariant()
                                     .Replace("-", "")
                                     .Replace(":", "")
                                     .Where(Uri.IsHexDigit)]);
            if (hex.Length < 6) return "";
            var oui = hex[..6]; // first 3 bytes

            // Minimal known OUIs; extend safely over time
            var vendors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "000C29", "VMware, Inc." },
                { "0003FF", "Microsoft" },       // Hyper-V (example)
                { "001C14", "Intel Corporation" },
                { "F4F5E8", "Intel Corporation" },
                { "B827EB", "Raspberry Pi Foundation" },
                { "F01FAF", "Apple, Inc." },
                { "A45E60", "Apple, Inc." },
                { "D83062", "Apple, Inc." },
                { "00E04C", "Realtek Semiconductor" },
                { "001E49", "Cisco Systems" }
            };

            return vendors.TryGetValue(oui, out var vendor) ? vendor : "Unknown";
        }

        private static void AppendLog(string message) => AppLog.Info(message);

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

                var lines = new List<string>();

                // Header
                var start = txtStartIp.Text.Trim();
                var end = txtEndIp.Text.Trim();
                lines.Add($"Network Discovery Results - {DateTime.Now}");
                if (!string.IsNullOrWhiteSpace(start) && !string.IsNullOrWhiteSpace(end))
                    lines.Add($"Range: {start} - {end}");
                lines.Add("");

                // Columns
                var headers = dgvResults.Columns
                    .Cast<DataGridViewColumn>()
                    .Select(c => c.HeaderText)
                    .ToArray();
                lines.Add(string.Join("\t", headers));

                // Rows
                foreach (DataGridViewRow row in dgvResults.Rows)
                {
                    if (row.IsNewRow) continue;
                    var cells = row.Cells
                        .Cast<DataGridViewCell>()
                        .Select(c => (c.Value?.ToString() ?? "")
                            .Replace("\t", " ")
                            .Replace("\r", " ")
                            .Replace("\n", " "))
                        .ToArray();
                    lines.Add(string.Join("\t", cells));
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
            public string Ip { get; set; } = "";
            public bool IsActive { get; set; }
            public long? LatencyMs { get; set; }
            public string Hostname { get; set; } = "";
            public string Mac { get; set; } = "";
            public string Manufacturer { get; set; } = ""; // NEW
            public string Status { get; set; } = "No Reply"; // NEW
        }
    }
}