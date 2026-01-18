using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows; // for MessageBoxButton/Image
using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Helpers;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// DiscoveryView partial: scanning logic and progress/ETA updates.
    /// </summary>
    public partial class DiscoveryView
    {
        // Populate adapter dropdown with filters aligned to AdaptersView
        private void LoadAdapters()
        {
            try
            {
                var adapters = NetworkController.GetAdapters() ?? [];

                bool showVirtual = AppSettings.ShowVirtualAdapters;
                bool showLoopback = AppSettings.ShowLoopbackAdapters;
                bool showBluetooth = AppSettings.ShowBluetoothAdapters;

                // Apply the same filtering rules as AdaptersView
                var filtered = new List<NetworkAdapterInfo>();
                foreach (var a in adapters)
                {
                    var desc = (a.HardwareDetails ?? string.Empty).ToLowerInvariant();
                    var name = (a.AdapterName ?? string.Empty).ToLowerInvariant();

                    bool isLoopback = name.Contains("loopback") || desc.Contains("loopback");
                    if (!showLoopback && isLoopback) continue;

                    bool isVirtual = desc.Contains("virtual") || name.Contains("virtualbox") || name.Contains("hyper-v") || desc.Contains("vmware");
                    if (!showVirtual && isVirtual) continue;

                    bool isBluetooth = desc.Contains("bluetooth") || name.Contains("bluetooth");
                    if (!showBluetooth && isBluetooth) continue;

                    filtered.Add(a);
                }

                CboAdapter.ItemsSource = filtered;
                CboAdapter.DisplayMemberPath = nameof(NetworkAdapterInfo.AdapterName);

                // Prefer existing shared selection; otherwise select first adapter.
                var target = AppSettings.SelectedAdapterName;
                if (!string.IsNullOrWhiteSpace(target) && filtered.Count > 0)
                {
                    var match = filtered.FirstOrDefault(a => string.Equals(a.AdapterName, target, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                        CboAdapter.SelectedItem = match;
                }

                if (CboAdapter.SelectedIndex < 0 && filtered.Count > 0)
                {
                    CboAdapter.SelectedIndex = 0;
                    if (CboAdapter.SelectedItem is NetworkAdapterInfo sel && string.IsNullOrWhiteSpace(AppSettings.SelectedAdapterName))
                        AppSettings.SetSelectedAdapter(sel.AdapterName);
                }
            }
            catch (Exception ex)
            { System.Windows.MessageBox.Show("Failed to load adapters.\n\n" + ex.Message, "Discovery", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        // Autofill the start/end scan range based on selected adapter IP/subnet
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

        // Compute the first 3 octets of the network address (ip & mask) as base string
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

        // Kick off a parallel scan across the IP range; update progress and ETA
        private async Task StartScanAsync()
        {
            var start = TxtStartIp.Text.Trim();
            var end = TxtEndIp.Text.Trim();
            if (!Helpers.ValidationHelper.IsValidIPv4(start) || !Helpers.ValidationHelper.IsValidIPv4(end))
            { System.Windows.MessageBox.Show("Enter valid start and end IP.", "Scan", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            long s = IpToLong(start);
            long e = IpToLong(end);
            if (e < s)
            { System.Windows.MessageBox.Show("End IP must be greater than or equal to Start IP.", "Scan", MessageBoxButton.OK, MessageBoxImage.Information); return; }

            // Reset state and initialize progress
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
            var sem = new SemaphoreSlim(64); // bound concurrency
            var tasks = new List<Task>();
            for (long ipVal = s; ipVal <= e; ipVal++)
            {
                if (token.IsCancellationRequested) break;
                var ip = LongToIp(ipVal);
                try { await sem.WaitAsync(token); }
                catch (OperationCanceledException) { break; }

                // Probe in background, update UI via dispatcher
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
            try { await Task.WhenAll(tasks); }
            catch (OperationCanceledException) { }
            finally
            {
                _sw?.Stop(); _cts?.Dispose(); _cts = null;
                UpdateStats();
            }
        }

        // Cancel current scan if running
        private void CancelScan()
        { try { _cts?.Cancel(); } catch { } }

        // Update progress bar, counts, and compute simple ETA from average rate
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

        // Convert dotted IPv4 to 32-bit integer for range iteration
        private static long IpToLong(string ip)
        {
            var parts = ip.Split('.');
            var b0 = byte.Parse(parts[0]);
            var b1 = byte.Parse(parts[1]);
            var b2 = byte.Parse(parts[2]);
            var b3 = byte.Parse(parts[3]);
            return ((long)b0 << 24) | ((long)b1 << 16) | ((long)b2 << 8) | b3;
        }

        // Convert 32-bit integer back to dotted IPv4 string
        private static string LongToIp(long v)
            => string.Join('.', (v >> 24) & 255, (v >> 16) & 255, (v >> 8) & 255, v & 255);
    }
}
