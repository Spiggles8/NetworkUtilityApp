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
        private Stopwatch? _scanSw;

        public TabDiscovery()
        {
            InitializeComponent();
            if (IsDesignMode()) return;

            btnAutofill.Click += (_, __) => AutofillRange();
            btnScan.Click += async (_, __) => await StartScanAsync();
            btnCancel.Click += (_, __) => CancelScan();
            btnSave.Click += async (_, __) => await SaveResultsAsync();

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
                return string.Join(".", baseParts.Take(3));
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

        private static long IpToInt(string ip)
        {
            var parts = ip.Split('.').Select(int.Parse).ToArray();
            return ((long)(uint)parts[0] << 24) | ((long)(uint)parts[1] << 16) | ((long)(uint)parts[2] << 8) | (uint)parts[3];
        }

        private static string IntToIp(long val)
            => string.Join('.', new[] { (val >> 24) & 255, (val >> 16) & 255, (val >> 8) & 255, val & 255 });

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

                    if (arpCache.TryGetValue(ip, out var mac))
                        pr.Mac = NormalizeMac(mac);
                    else
                    {
                        LoadArpTableInto(arpCache);
                        if (arpCache.TryGetValue(ip, out mac)) pr.Mac = NormalizeMac(mac);
                    }
                    pr.Manufacturer = ResolveManufacturer(pr.Mac);
                }
            }
            catch { }
            return pr;
        }

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
                    if (parts.Length >= 3 && ValidationHelper.IsValidIPv4(parts[0])) map[parts[0]] = NormalizeMac(parts[1]);
                }
            }
            catch { }
        }

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
                        .Select(c => (c.Value?.ToString() ?? "").Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' '))
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
            public string Ip { get; set; } = "";
            public bool IsActive { get; set; }
            public long? LatencyMs { get; set; }
            public string Hostname { get; set; } = "";
            public string Mac { get; set; } = "";
            public string Manufacturer { get; set; } = "";
            public string Status { get; set; } = "No Reply";
        }
    }
}