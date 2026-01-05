using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkUtilityApp.Views
{
    public partial class DiagnosticsView : System.Windows.Controls.UserControl
    {
        private CancellationTokenSource? _cts;
        private Process? _activeProcess;

        public DiagnosticsView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, System.Windows.RoutedEventArgs e)
        {
            BtnRunTraceroute.Click += async (_, __) => await RunTracerouteAsync();
            BtnNslookupRun.Click += async (_, __) => await RunToolAsync("nslookup", NslookupTarget.Text.Trim(), 60000, tag: "NSLOOKUP");
            BtnPathpingRun.Click += async (_, __) => await RunToolAsync("pathping", PathpingTarget.Text.Trim(), 180000, tag: "PATHPING", argsPrefix: "-n ");
            BtnCancel.Click += (_, __) => CancelActive();
            BtnPing.Click += async (_, __) => await RunPingOnceAsync();
        }

        private void Append(string line)
        {
            if (TxtOutput == null) return;
            TxtOutput.AppendText((TxtOutput.Text.Length == 0 ? string.Empty : Environment.NewLine) + line);
            TxtOutput.ScrollToEnd();
        }

        private async Task RunPingOnceAsync()
        {
            var target = PingTarget.Text.Trim();
            if (string.IsNullOrWhiteSpace(target)) { Append("[ERROR] Enter a ping target."); return; }
            var result = Controllers.NetworkController.PingHost(target);
            Append(result);
            await Task.CompletedTask;
        }

        private async Task RunTracerouteAsync()
        {
            var target = TraceTarget.Text.Trim();
            if (string.IsNullOrWhiteSpace(target)) { Append("[ERROR] Enter a traceroute target."); return; }
            try
            {
                var res = Controllers.NetworkController.Traceroute(target, resolveNames: ChkTraceResolve.IsChecked == true);
                Append($"[TRACE] Target: {res.Target}");
                foreach (var h in res.Hops)
                {
                    var r1 = h.Rtt1Ms?.ToString() ?? "*";
                    var r2 = h.Rtt2Ms?.ToString() ?? "*";
                    var r3 = h.Rtt3Ms?.ToString() ?? "*";
                    Append($"{h.Hop,2}  {r1,4} ms  {r2,4} ms  {r3,4} ms  {h.HostnameOrAddress}");
                }
                if (res.Hops.Count == 0)
                {
                    Append("[TRACE] No hops parsed. Raw output:");
                    Append(res.RawOutput);
                }
            }
            catch (Exception ex)
            {
                Append("[ERROR] Traceroute failed: " + ex.Message);
            }
            await Task.CompletedTask;
        }

        private async Task RunToolAsync(string fileName, string target, int timeoutMs, string tag, string argsPrefix = "")
        {
            if (string.IsNullOrWhiteSpace(target)) { Append($"[ERROR] Enter a target for {tag}."); return; }
            CancelActive();
            _cts = new CancellationTokenSource();
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = string.IsNullOrWhiteSpace(argsPrefix) ? target : $"{argsPrefix}{target}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                _activeProcess = Process.Start(psi);
                if (_activeProcess == null) { Append($"[ERROR] Failed to start {fileName}."); return; }

                var token = _cts.Token;
                var sb = new StringBuilder();
                var sw = Stopwatch.StartNew();

                await Task.Run(async () =>
                {
                    try
                    {
                        using var reader = _activeProcess.StandardOutput;
                        string? line;
                        while (!reader.EndOfStream)
                        {
                            if (timeoutMs > 0 && sw.ElapsedMilliseconds > timeoutMs)
                            {
                                Append($"[{tag}] Timeout exceeded.");
                                try { _activeProcess.Kill(true); } catch { }
                                return;
                            }
                            token.ThrowIfCancellationRequested();
                            line = await reader.ReadLineAsync();
                            if (line == null) break;
                            if (line.Length == 0) continue;
                            Append(line);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Append($"[{tag}] Cancelled.");
                    }
                });

                var err = await _activeProcess.StandardError.ReadToEndAsync();
                _activeProcess.WaitForExit();
                if (!string.IsNullOrWhiteSpace(err)) Append($"[{tag} ERROR] " + err.Trim());
                Append($"[{tag}] Completed.");
            }
            catch (Exception ex)
            {
                Append($"[ERROR] {tag} failed: " + ex.Message);
            }
            finally
            {
                _cts?.Dispose(); _cts = null;
                _activeProcess = null;
            }
        }

        private void CancelActive()
        {
            try
            {
                _cts?.Cancel();
                if (_activeProcess != null && !_activeProcess.HasExited) _activeProcess.Kill(true);
            }
            catch { }
        }
    }
}
