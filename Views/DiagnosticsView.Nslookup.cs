using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// DiagnosticsView partial: nslookup external process runner.
    /// </summary>
    public partial class DiagnosticsView
    {
        // Run nslookup for the value in the target textbox with a generous timeout
        private async Task RunNslookupAsync()
        {
            await RunToolAsync("nslookup", NslookupTarget.Text.Trim(), 60000, tag: "NSLOOKUP");
        }

        // Generic external tool runner used by diagnostics actions.
        // - fileName   : process to start (e.g., nslookup)
        // - target     : input argument (validated non-empty)
        // - timeoutMs  : max time to stream stdout before killing the process
        // - tag        : short tag used in emitted log lines
        // - argsPrefix : optional argument prefix (e.g., "-n " for ping)
        private async Task RunToolAsync(string fileName, string target, int timeoutMs, string tag, string argsPrefix = "")
        {
            if (string.IsNullOrWhiteSpace(target)) { Append($"[ERROR] Enter a target for {tag}."); return; }
            CancelActive(); // ensure only one tool runs at a time
            _cts = new CancellationTokenSource();
            try
            {
                // Start process with redirected stdout/stderr for streaming
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
                var sw = Stopwatch.StartNew();

                // Stream stdout lines until timeout or cancellation
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

                // Drain stderr (if any), wait exit, then finalize
                var err = await _activeProcess.StandardError.ReadToEndAsync();
                _activeProcess.WaitForExit();
                if (!string.IsNullOrWhiteSpace(err)) Append($"[{tag} ERROR] " + err.Trim());
                Append($"[{tag}] Completed.");
            }
            catch (System.Exception ex)
            {
                Append($"[ERROR] {tag} failed: " + ex.Message);
            }
            finally
            {
                _cts?.Dispose(); _cts = null;
                _activeProcess = null;
            }
        }
    }
}
