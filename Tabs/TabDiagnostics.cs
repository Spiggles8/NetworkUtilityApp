using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Services;
using System.ComponentModel;
using System.Diagnostics;

namespace NetworkUtilityApp.Tabs
{
    public partial class TabDiagnostics : UserControl
    {
        private CancellationTokenSource? _pingCts;                 // NEW
        private const int PingIntervalMs = 2000;                   // NEW (2 seconds)

        public TabDiagnostics()
        {
            InitializeComponent();
            if (IsDesignMode()) return;
            btnPing.Click += async (_, __) => await OnPingAsync();
            btnTrace.Click += async (_, __) => await OnTraceAsync();
            btnNslookup.Click += async (_, __) => await OnNslookupAsync();
            btnPathPing.Click += async (_, __) => await OnPathPingAsync();
            Disposed += (_, __) => _pingCts?.Cancel();             // NEW: cleanup
        }

        // Reverted to instance (was static) so Form1 can call tabDiagnostics.Initialize();
        public static void Initialize()
        {
            // No startup work needed currently.
        }

        private bool IsDesignMode()
            => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private async Task OnPingAsync()
        {
            var target = txtPingTarget.Text?.Trim();
            if (string.IsNullOrWhiteSpace(target))
            {
                AppendLog("[ERROR] Please enter a ping target.");
                return;
            }

            // Continuous mode
            if (chkPingContinuous.Checked)
            {
                // Toggle: if already running, stop it
                if (_pingCts is not null)
                {
                    _pingCts.Cancel();
                    _pingCts = null;
                    btnPing.Text = "Ping";
                    AppendLog("[PING] Continuous ping stopped.");
                    return;
                }

                _pingCts = new CancellationTokenSource();
                btnPing.Text = "Stop";
                AppendLog($"[PING] Starting continuous ping: {target} (every {PingIntervalMs / 1000}s)");

                try
                {
                    await Task.Run(async () =>
                    {
                        var token = _pingCts!.Token;
                        while (!token.IsCancellationRequested)
                        {
                            var result = NetworkController.PingHost(target);
                            AppendLog(result);
                            await Task.Delay(PingIntervalMs, token);
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    // expected on stop
                }
                catch (Exception ex)
                {
                    AppendLog("[ERROR] " + ex.Message);
                }
                finally
                {
                    btnPing.Text = "Ping";
                    _pingCts?.Dispose();
                    _pingCts = null;
                }
                return;
            }

            // One-shot ping
            AppendLog($"[PING] Target: {target}");
            var once = await Task.Run(() => NetworkController.PingHost(target));
            AppendLog(once);
        }

        private async Task OnTraceAsync()
        {
            var target = txtTraceTarget.Text?.Trim();
            if (string.IsNullOrWhiteSpace(target))
            {
                AppendLog("[ERROR] Please enter a traceroute target.");
                return;
            }
            var resolve = chkResolveNames.Checked;
            AppendLog($"[TRACEROUTE] Target: {target} (Resolve names: {resolve})");
            var result = await Task.Run(() => NetworkController.Traceroute(target, 30, 4000, resolve));
            if (result is null)
            {
                AppendLog("[ERROR] Traceroute returned no data.");
                return;
            }
            if (result.Hops == null || result.Hops.Count == 0)
            {
                AppendLog("[INFO] No hops parsed. Raw output follows:");
                AppendLog(result.RawOutput);
                return;
            }
            foreach (var hop in result.Hops)
            {
                var r1 = hop.Rtt1Ms?.ToString() ?? "*";
                var r2 = hop.Rtt2Ms?.ToString() ?? "*";
                var r3 = hop.Rtt3Ms?.ToString() ?? "*";
                var host = hop.TimedOut ? "*" : hop.HostnameOrAddress;
                AppendLog($"{hop.Hop,2}: {r1} ms  {r2} ms  {r3} ms  {host}");
            }
        }

        private async Task OnNslookupAsync()
        {
            var target = txtNslookupTarget.Text?.Trim();
            if (string.IsNullOrWhiteSpace(target))
            {
                AppendLog("[ERROR] Please enter a target for nslookup.");
                return;
            }
            AppendLog($"[NSLOOKUP] Target: {target}");
            var output = await Task.Run(() => RunTool("nslookup", target));
            AppendLog(output);
        }

        private async Task OnPathPingAsync()
        {
            var target = txtPathPingTarget.Text?.Trim();
            if (string.IsNullOrWhiteSpace(target))
            {
                AppendLog("[ERROR] Please enter a target for pathping (IP or host).");
                return;
            }
            AppendLog($"[PATHPING] Target: {target}");
            var output = await Task.Run(() => RunTool("pathping", $"-n {target}"));
            AppendLog(output);
        }

        private static string RunTool(string fileName, string arguments)
        {
            try
            {
                using var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p.Start();
                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                if (!p.WaitForExit(120_000))
                    return "[ERROR] Command timed out.";
                var text = (stdout + (string.IsNullOrWhiteSpace(stderr) ? "" : Environment.NewLine + stderr)).Trim();
                return string.IsNullOrWhiteSpace(text) ? "[INFO] No output." : text;
            }
            catch (Exception ex)
            {
                return "[ERROR] " + ex.Message;
            }
        }

        private static void AppendLog(string message) => AppLog.Info(message);
    }
}