using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Services;
using System.ComponentModel;
using System.Diagnostics;

namespace NetworkUtilityApp.Tabs
{
    /// <summary>
    /// Diagnostics tab: wraps common network tools (ping, traceroute,
    /// nslookup, pathping) and provides a simple UI around them.
    /// </summary>
    public partial class TabDiagnostics : UserControl
    {
        // Cancellation source for continuous ping loop
        private CancellationTokenSource? _pingCts;
        // Interval between continuous pings (ms). UI exposes a separate
        // interval for the WebView-based page; this constant is for WinForms.
        private const int PingIntervalMs = 2000;

        public TabDiagnostics()
        {
            InitializeComponent();
            if (IsDesignMode()) return;

            // Wire button handlers once at runtime
            btnPing.Click += async (_, __) => await OnPingAsync();
            btnTrace.Click += async (_, __) => await OnTraceAsync();
            btnNslookup.Click += async (_, __) => await OnNslookupAsync();
            btnPathPing.Click += async (_, __) => await OnPathPingAsync();

            // Ensure any running continuous ping is cancelled on disposal
            Disposed += (_, __) => _pingCts?.Cancel();

            // Allow manual BackColor/ForeColor overrides at runtime
            btnPing.UseVisualStyleBackColor = false;
            btnPing.FlatStyle = FlatStyle.Standard;
        }

        // Kept for API symmetry; currently no initialization required
        public static void Initialize()
        {
        }

        private bool IsDesignMode()
            => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>
        /// Handle Ping button: runs either a single ping or a
        /// cancellable continuous ping loop based on the checkbox.
        /// </summary>
        private async Task OnPingAsync()
        {
            var target = txtPingTarget.Text?.Trim();
            if (string.IsNullOrWhiteSpace(target))
            {
                AppendLog("[ERROR] Please enter a ping target.");
                return;
            }

            // Continuous mode: toggle start/stop
            if (chkPingContinuous.Checked)
            {
                // If already running, stop current loop
                if (_pingCts is not null)
                {
                    _pingCts.Cancel();
                    _pingCts = null;
                    btnPing.Text = "Ping";
                    // Revert button appearance
                    btnPing.BackColor = SystemColors.Control;
                    btnPing.ForeColor = SystemColors.ControlText;
                    btnPing.UseVisualStyleBackColor = true;
                    AppendLog("[PING] Continuous ping stopped.");
                    return;
                }

                // Start continuous loop
                _pingCts = new CancellationTokenSource();
                btnPing.Text = "Stop";
                // Emphasize active state as a danger-style button
                btnPing.UseVisualStyleBackColor = false;
                btnPing.BackColor = Color.Red;
                btnPing.ForeColor = Color.White;
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
                    // Expected when user stops continuous ping
                }
                catch (Exception ex)
                {
                    AppendLog("[ERROR] " + ex.Message);
                }
                finally
                {
                    // Always restore button state
                    btnPing.Text = "Ping";
                    btnPing.BackColor = SystemColors.Control;
                    btnPing.ForeColor = SystemColors.ControlText;
                    btnPing.UseVisualStyleBackColor = true;
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

        // Run traceroute using the controller wrapper and log hop details.
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

        // Invoke nslookup for the given target and log raw output.
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

        // Invoke pathping with basic arguments and log output.
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

        // Run a console tool and return combined stdout/stderr.
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

                var text = (stdout + (string.IsNullOrWhiteSpace(stderr) ? string.Empty : Environment.NewLine + stderr)).Trim();
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