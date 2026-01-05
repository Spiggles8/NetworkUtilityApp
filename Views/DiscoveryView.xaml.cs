using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Helpers;

namespace NetworkUtilityApp.Views
{
    public partial class DiscoveryView : System.Windows.Controls.UserControl
    {
        private CancellationTokenSource? _cts;
        private readonly ObservableCollection<ProbeRow> _rows = new();
        private Stopwatch? _sw;
        private int _total;
        private int _scanned;
        private int _active;
        private readonly Dictionary<string,string> _arpCache = new(StringComparer.OrdinalIgnoreCase);

        public DiscoveryView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            DgvResults.ItemsSource = _rows;
            BtnScan.Click += async (_, __) => await StartScanAsync();
            BtnCancel.Click += (_, __) => CancelScan();
            BtnSave.Click += async (_, __) => await SaveResultsAsync();
            CboAdapter.SelectionChanged += (_, __) => AutofillRange();
            LoadAdapters();
        }

        private void LoadAdapters()
        {
            try
            {
                var adapters = NetworkController.GetAdapters() ?? [];
                CboAdapter.ItemsSource = adapters;
                CboAdapter.DisplayMemberPath = nameof(NetworkAdapterInfo.AdapterName);
                CboAdapter.SelectedIndex = adapters.Count > 0 ? 0 : -1;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to load adapters.\n\n" + ex.Message, "Discovery", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AutofillRange()
        {
            if (CboAdapter.SelectedItem is NetworkAdapterInfo a)
            {
                var ip = a.IpAddress;
                var subnet = a.Subnet;
                if (!ValidationHelper.IsValidIPv4(ip) || !ValidationHelper.IsValidIPv4(subnet)) return;
                var base3 = GetNetworkBase(ip, subnet);
                if (base3 is null) return;
                TxtStartIp.Text = base3 + ".1";
                TxtEndIp.Text = base3 + ".254";
            }
        }

        private static string? GetNetworkBase(string ip, string subnet)
        {
            try
            {
                var ipOct = ip.Split('.');
                var maskOct = subnet.Split('.');
                if (ipOct.Length != 4 || maskOct.Length != 4) return null;
                var baseParts = new string[3];
                for (int i = 0; i < 3; i++)
                {
                    int ipPart = int.Parse(ipOct[i]);
                    int maskPart = int.Parse(maskOct[i]);
                    baseParts[i] = (ipPart & maskPart).ToString();
                }
                return string.Join(".", baseParts);
            }
            catch { return null; }
        }

        private async Task StartScanAsync()
        {
            var start = TxtStartIp.Text.Trim();
            var end = TxtEndIp.Text.Trim();
            if (!ValidationHelper.IsValidIPv4(start) || !ValidationHelper.IsValidIPv4(end))
            {
                System.Windows.MessageBox.Show("Enter valid start and end IP.", "Scan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            long s = IpToLong(start);
            long e = IpToLong(end);
            if (e < s)
            {
                System.Windows.MessageBox.Show("End IP must be greater than or equal to Start IP.", "Scan", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            CancelScan();
            _cts = new CancellationTokenSource();
            _rows.Clear();
            _scanned = 0; _active = 0; _total = (int)Math.Min(int.MaxValue, e - s + 1);
            PrgScan.Value = 0; PrgScan.Maximum = _total;
            LblCounts.Text = $"Scanned: 0 / {_total} | Active: 0";
            LblEta.Text = "--:--:--";
            _sw = Stopwatch.StartNew();
            _arpCache.Clear();
            LoadArpTableInto(_arpCache);

            var token = _cts.Token;
            var sem = new SemaphoreSlim(64);
            var tasks = new List<Task>();
            for (long ipVal = s; ipVal <= e; ipVal++)
            {
                if (token.IsCancellationRequested) break; // exit loop cleanly
                var ip = LongToIp(ipVal);
                try
                {
                    await sem.WaitAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break; // stop scheduling more tasks
                }

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        var probe = await ProbeAsync(ip);
                        Interlocked.Increment(ref _scanned);
                        if (probe.IsActive) Interlocked.Increment(ref _active);
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (probe.IsActive) _rows.Add(probe);
                            UpdateStats();
                        });
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex) { Debug.WriteLine("DISC ERROR: " + ex.Message); }
                    finally { sem.Release(); }
                }, token));
            }
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) { }
            finally
            {
                _sw?.Stop(); _cts?.Dispose(); _cts = null;
                UpdateStats();
            }
        }

        private void CancelScan()
        {
            try { _cts?.Cancel(); } catch { }
        }

        private void UpdateStats()
        {
            PrgScan.Value = Math.Min(_scanned, _total);
            LblCounts.Text = $"Scanned: {_scanned} / {_total} | Active: {_active}";
            var eta = "--:--:--";
            if (_sw != null && _scanned > 0 && _scanned < _total)
            {
                double per = _sw.Elapsed.TotalSeconds / _scanned;
                var remaining = TimeSpan.FromSeconds(Math.Max(0, (_total - _scanned) * per));
                eta = $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
            }
            LblEta.Text = eta;
        }

        private static long IpToLong(string ip)
        {
            var parts = ip.Split('.');
            var b0 = byte.Parse(parts[0]);
            var b1 = byte.Parse(parts[1]);
            var b2 = byte.Parse(parts[2]);
            var b3 = byte.Parse(parts[3]);
            return ((long)b0 << 24) | ((long)b1 << 16) | ((long)b2 << 8) | b3;
        }

        private static string LongToIp(long v)
            => string.Join('.', (v >> 24) & 255, (v >> 16) & 255, (v >> 8) & 255, v & 255);

        private async Task<ProbeRow> ProbeAsync(string ip)
        {
            var pr = new ProbeRow { Ip = ip, Status = "No Reply" };
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ip, 400);
                if (reply.Status == IPStatus.Success)
                {
                    pr.IsActive = true;
                    pr.Status = "Active";
                    pr.LatencyMs = reply.RoundtripTime;

                    try
                    {
                        var host = await System.Net.Dns.GetHostEntryAsync(ip);
                        pr.Hostname = host.HostName;
                    }
                    catch
                    {
                        pr.Hostname = LlmnrResolver.TryGetHostname(ip, 1200, null);
                        if (string.IsNullOrWhiteSpace(pr.Hostname)) pr.Hostname = MdnsResolver.TryGetHostname(ip, 1500, null);
                        if (string.IsNullOrWhiteSpace(pr.Hostname)) pr.Hostname = NbnsResolver.TryGetHostname(ip, 1200, null);
                        if (string.IsNullOrWhiteSpace(pr.Hostname)) pr.Hostname = TryResolveNetbiosName(ip);
                    }

                    if (_arpCache.TryGetValue(ip, out var mac)) pr.Mac = mac; else {
                        LoadArpTableInto(_arpCache);
                        if (_arpCache.TryGetValue(ip, out mac)) pr.Mac = mac;
                    }
                    pr.Manufacturer = MacVendors.Lookup(pr.Mac);
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
                    var parts = trimmed.Split(new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && ValidationHelper.IsValidIPv4(parts[0]))
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

        private async Task SaveResultsAsync()
        {
            try
            {
                if (_rows.Count == 0) { System.Windows.MessageBox.Show("Nothing to save.", "Discovery", MessageBoxButton.OK, MessageBoxImage.Information); return; }
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save Discovery Results",
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FileName = $"NetworkDiscovery_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"
                };
                if (dlg.ShowDialog() != true) return;
                var lines = _rows.Select(r => $"{r.Ip}\t{r.Hostname}\t{r.Mac}\t{r.Manufacturer}\t{r.LatencyMs}\t{r.Status}");
                await System.IO.File.WriteAllLinesAsync(dlg.FileName, lines);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to save: " + ex.Message, "Discovery", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private sealed class ProbeRow
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
