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
        private async Task RunNslookupAsync()
        {
            await RunToolAsync("nslookup", NslookupTarget.Text.Trim(), 60000, tag: "NSLOOKUP");
        }

        private async Task RunToolAsync(string fileName, string target, int timeoutMs, string tag, string argsPrefix = "")
        {
            if (string.IsNullOrWhiteSpace(target)) { Append($"[ERROR] Enter a target for {tag}."); return; }
            CancelActive(); // ensure only one tool runs at a time
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
