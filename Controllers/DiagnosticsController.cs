using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkUtilityApp.Controllers
{
    public enum DiagStatus
    {
        Info,
        Success,
        Warning,
        Error
    }

    public sealed class DiagnosticsController : IDisposable
    {
        private CancellationTokenSource? _pingCts;
        private CancellationTokenSource? _traceCts;

        #region Ping (single)

        /// <summary>
        /// Pings a host once. Returns a formatted status line.
        /// </summary>
        public async Task<string> PingOnceAsync(string host, int timeoutMs = 2000, int size = 32, int ttl = 128, bool dontFragment = true)
        {
            try
            {
                using var ping = new Ping();
                var options = new PingOptions(ttl, dontFragment);
                var buffer = new byte[Math.Max(0, size)];

                var reply = await ping.SendPingAsync(host, timeoutMs, buffer, options).ConfigureAwait(false);

                return reply.Status == IPStatus.Success
                    ? $"[PING SUCCESS] {host} {reply.RoundtripTime}ms (TTL={reply.Options?.Ttl}, Size={buffer.Length})"
                    : $"[PING FAIL] {host} - {reply.Status}";
            }
            catch (Exception ex)
            {
                return $"[ERROR] Ping exception for {host}: {ex.Message}";
            }
        }

        #endregion

        #region Ping (continuous)

        /// <summary>
        /// Begins a continuous ping loop. Use StopContinuousPing() to cancel.
        /// Lines are streamed via onLine callback as they occur.
        /// </summary>
        public void StartContinuousPing(
            string host,
            int intervalMs,
            Action<string, DiagStatus> onLine,
            int timeoutMs = 2000,
            int size = 32,
            int ttl = 128,
            bool dontFragment = true)
        {
            StopContinuousPing(); // cancel previous if running
            _pingCts = new CancellationTokenSource();
            var token = _pingCts.Token;

            _ = Task.Run(async () =>
            {
                onLine?.Invoke($"[PING START] {host} interval={intervalMs}ms timeout={timeoutMs}ms", DiagStatus.Info);

                using var ping = new Ping();
                var options = new PingOptions(ttl, dontFragment);
                var buffer = new byte[Math.Max(0, size)];

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var reply = await ping.SendPingAsync(host, timeoutMs, buffer, options).ConfigureAwait(false);
                        if (reply.Status == IPStatus.Success)
                            onLine?.Invoke($"[PING] {host} {reply.RoundtripTime}ms (TTL={reply.Options?.Ttl})", DiagStatus.Success);
                        else
                            onLine?.Invoke($"[PING] {host} - {reply.Status}", DiagStatus.Warning);
                    }
                    catch (Exception ex)
                    {
                        onLine?.Invoke($"[ERROR] Ping exception: {ex.Message}", DiagStatus.Error);
                    }

                    try
                    {
                        await Task.Delay(Math.Max(0, intervalMs), token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException) { /* normal */ }
                }

                onLine?.Invoke($"[PING STOP] {host}", DiagStatus.Info);
            }, token);
        }

        public void StopContinuousPing()
        {
            if (_pingCts == null) return;
            try { _pingCts.Cancel(); }
            catch { /* ignore */ }
            finally
            {
                _pingCts.Dispose();
                _pingCts = null;
            }
        }

        #endregion

        #region Traceroute (tracert.exe)

        /// <summary>
        /// Runs traceroute via Windows 'tracert'. Streams output lines via onLine; also returns a final block string.
        /// </summary>
        public async Task<string> TracerouteAsync(
            string host,
            bool resolveHostnames,
            int maxHops = 30,
            int perHopTimeoutMs = 2000,
            Action<string, DiagStatus>? onLine = null)
        {
            StopTraceroute(); // cancel any prior run
            _traceCts = new CancellationTokenSource();
            var token = _traceCts.Token;

            var sb = new StringBuilder();
            void Emit(string line, DiagStatus st)
            {
                sb.AppendLine(line);
                onLine?.Invoke(line, st);
            }

            try
            {
                var psi = new ProcessStartInfo("tracert")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                if (!resolveHostnames) { psi.ArgumentList.Add("-d"); }
                psi.ArgumentList.Add("-h"); psi.ArgumentList.Add(Math.Max(1, maxHops).ToString());
                psi.ArgumentList.Add("-w"); psi.ArgumentList.Add(Math.Max(100, perHopTimeoutMs).ToString());
                psi.ArgumentList.Add(host);

                Emit($"[TRACE START] {host} hops={maxHops} timeout={perHopTimeoutMs}ms resolve={(resolveHostnames ? "ON" : "OFF")}", DiagStatus.Info);

                using var p = new Process { StartInfo = psi };
                p.Start();

                var readOut = Task.Run(async () =>
                {
                    string? line;
                    while ((line = await p.StandardOutput.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        Emit(line, DiagStatus.Info);
                    }
                }, token);

                var readErr = Task.Run(async () =>
                {
                    string? line;
                    while ((line = await p.StandardError.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        Emit("[stderr] " + line, DiagStatus.Warning);
                    }
                }, token);

                await Task.WhenAll(Task.Run(() => p.WaitForExit(), token), readOut, readErr).ConfigureAwait(false);

                var code = p.ExitCode;
                Emit($"[TRACE END] ExitCode={code}", code == 0 ? DiagStatus.Success : DiagStatus.Warning);

                return sb.ToString();
            }
            catch (OperationCanceledException)
            {
                Emit("[TRACE] cancelled.", DiagStatus.Warning);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Emit("[ERROR] Traceroute exception: " + ex.Message, DiagStatus.Error);
                return sb.ToString();
            }
            finally
            {
                StopTraceroute();
            }
        }

        public void StopTraceroute()
        {
            if (_traceCts == null) return;
            try { _traceCts.Cancel(); }
            catch { /* ignore */ }
            finally
            {
                _traceCts.Dispose();
                _traceCts = null;
            }
        }

        #endregion

        #region TCP Port Tests

        /// <summary>
        /// Tests a single TCP port. Returns (ok, elapsedMs, errorMessage).
        /// </summary>
        public async Task<(bool ok, long elapsedMs, string? error)> TestTcpPortAsync(string host, int port, int timeoutMs = 3000)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var cts = new CancellationTokenSource(timeoutMs);
                using var client = new TcpClient();

                var task = client.ConnectAsync(host, port);
                using (cts.Token.Register(() => { try { client.Close(); } catch { } }))
                {
                    await task.ConfigureAwait(false);
                }

                sw.Stop();
                return (client.Connected, sw.ElapsedMilliseconds, null);
            }
            catch (Exception ex)
            {
                sw.Stop();
                return (false, sw.ElapsedMilliseconds, ex.Message);
            }
        }

        /// <summary>
        /// Tests a set of ports concurrently; returns the list of open ports.
        /// </summary>
        public async Task<List<int>> TestPortSetAsync(string host, IEnumerable<int> ports, int timeoutMs = 1500, int maxParallel = 64, Action<string, DiagStatus>? onLine = null)
        {
            var open = new ConcurrentBag<int>();
            using var gate = new SemaphoreSlim(Math.Max(1, maxParallel));

            var tasks = new List<Task>();
            foreach (var p in ports)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await gate.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        var (ok, ms, err) = await TestTcpPortAsync(host, p, timeoutMs).ConfigureAwait(false);
                        if (ok)
                        {
                            open.Add(p);
                            onLine?.Invoke($"[PORT OPEN] {host}:{p} ({ms}ms)", DiagStatus.Success);
                        }
                        else
                        {
                            onLine?.Invoke($"[PORT CLOSED] {host}:{p} ({ms}ms) {err}", DiagStatus.Info);
                        }
                    }
                    finally
                    {
                        gate.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            var list = new List<int>(open);
            list.Sort();
            return list;
        }

        #endregion

        public void Dispose()
        {
            StopContinuousPing();
            StopTraceroute();
        }
    }
}
