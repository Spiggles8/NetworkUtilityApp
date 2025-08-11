using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkUtilityApp.Controllers
{
    public sealed class DiscoveryController : IDisposable
    {
        // ===== Models =====

        public sealed class DiscoveryResult
        {
            public string IP { get; init; } = "";
            public string Hostname { get; init; } = "";
            public string MAC { get; init; } = "";
            public long LatencyMs { get; init; } = -1;
            public List<int> OpenPorts { get; init; } = new();
        }

        public sealed class DiscoveryOptions
        {
            public bool ResolveDns { get; init; } = false;
            public int TimeoutMs { get; init; } = 1000;
            public int MaxParallel { get; init; } = 256;
            public IReadOnlyList<int> PortsToTest { get; init; } = Array.Empty<int>();
        }

        public sealed class DiscoveryProgress
        {
            public int Total { get; init; }
            public int Scanned { get; init; }
            public int Active { get; init; }
        }

        public sealed class NicSubnet
        {
            public string AdapterName { get; init; } = "";
            public string IPv4 { get; init; } = "";
            public string Mask { get; init; } = "";
            public int Prefix { get; init; }
            public uint Start { get; init; }
            public uint End { get; init; }
        }

        // ===== State =====

        private CancellationTokenSource? _scanCts;
        private SemaphoreSlim? _gate;

        // ===== Public API =====

        /// <summary>
        /// Scan an inclusive IPv4 range. Streams results & progress via callbacks and returns all discovered hosts.
        /// </summary>
        public async Task<List<DiscoveryResult>> ScanRangeAsync(
            uint start,
            uint end,
            DiscoveryOptions options,
            Action<DiscoveryProgress>? onProgress = null,
            Action<DiscoveryResult>? onHostFound = null)
        {
            Stop(); // cancel any previous run

            var total = checked((int)(end - start + 1));
            var scanned = 0;
            var active = 0;

            _scanCts = new CancellationTokenSource();
            var ct = _scanCts.Token;

            _gate = new SemaphoreSlim(Math.Max(1, options.MaxParallel));
            var results = new ConcurrentBag<DiscoveryResult>();
            var tasks = new List<Task>(total);

            for (uint ip = start; ip <= end; ip++)
            {
                var ipStr = ToIP(ip);
                tasks.Add(Task.Run(async () =>
                {
                    await _gate!.WaitAsync(ct).ConfigureAwait(false);
                    try
                    {
                        var result = await ProbeHostAsync(ipStr, options, ct).ConfigureAwait(false);
                        if (result is not null)
                        {
                            results.Add(result);
                            Interlocked.Increment(ref active);
                            onHostFound?.Invoke(result);
                        }
                    }
                    finally
                    {
                        var done = Interlocked.Increment(ref scanned);
                        onProgress?.Invoke(new DiscoveryProgress { Total = total, Scanned = done, Active = active });
                        _gate.Release();
                    }
                }, ct));
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Swallow; caller can inspect partial results
            }

            return results.OrderBy(r => ParseIPv4(r.IP)).ToList();
        }

        /// <summary>
        /// Build a unique set of targets from multiple IPv4 ranges (e.g., when scanning all NIC subnets).
        /// </summary>
        public static HashSet<uint> BuildTargetSet(IEnumerable<(uint start, uint end)> ranges)
        {
            var set = new HashSet<uint>();
            foreach (var (s, e) in ranges)
            {
                for (uint ip = s; ip <= e; ip++) set.Add(ip);
            }
            return set;
        }

        /// <summary>
        /// Scan a set of arbitrary targets (e.g., aggregated from multiple NICs).
        /// </summary>
        public async Task<List<DiscoveryResult>> ScanTargetsAsync(
            IEnumerable<uint> targets,
            DiscoveryOptions options,
            Action<DiscoveryProgress>? onProgress = null,
            Action<DiscoveryResult>? onHostFound = null)
        {
            Stop();

            var list = targets.ToList();
            var total = list.Count;
            var scanned = 0;
            var active = 0;

            _scanCts = new CancellationTokenSource();
            var ct = _scanCts.Token;

            _gate = new SemaphoreSlim(Math.Max(1, options.MaxParallel));
            var results = new ConcurrentBag<DiscoveryResult>();
            var tasks = new List<Task>(total);

            foreach (var ipU in list)
            {
                var ipStr = ToIP(ipU);
                tasks.Add(Task.Run(async () =>
                {
                    await _gate!.WaitAsync(ct).ConfigureAwait(false);
                    try
                    {
                        var result = await ProbeHostAsync(ipStr, options, ct).ConfigureAwait(false);
                        if (result is not null)
                        {
                            results.Add(result);
                            Interlocked.Increment(ref active);
                            onHostFound?.Invoke(result);
                        }
                    }
                    finally
                    {
                        var done = Interlocked.Increment(ref scanned);
                        onProgress?.Invoke(new DiscoveryProgress { Total = total, Scanned = done, Active = active });
                        _gate.Release();
                    }
                }, ct));
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // cancelled
            }

            return results.OrderBy(r => ParseIPv4(r.IP)).ToList();
        }

        /// <summary>
        /// Cancel any in-flight scan.
        /// </summary>
        public void Stop()
        {
            try { _scanCts?.Cancel(); } catch { }
            try { _gate?.Dispose(); } catch { }
            _scanCts?.Dispose();
            _scanCts = null;
            _gate = null;
        }

        public void Dispose() => Stop();

        // ===== Host probe =====

        private static async Task<DiscoveryResult?> ProbeHostAsync(string ip, DiscoveryOptions opt, CancellationToken ct)
        {
            // 1) ICMP ping first
            long rtt = -1;
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ip, opt.TimeoutMs).WaitAsync(TimeSpan.FromMilliseconds(opt.TimeoutMs + 250), ct)
                    .ConfigureAwait(false);

                if (reply.Status != IPStatus.Success) return null;
                rtt = reply.RoundtripTime;
            }
            catch
            {
                return null; // treat exceptions as not alive
            }

            // 2) Reverse DNS (optional)
            string host = "";
            if (opt.ResolveDns)
            {
                try
                {
                    var entry = await Dns.GetHostEntryAsync(ip).WaitAsync(TimeSpan.FromMilliseconds(Math.Max(200, opt.TimeoutMs)), ct)
                        .ConfigureAwait(false);
                    host = entry?.HostName ?? "";
                }
                catch { /* ignore */ }
            }

            // 3) MAC via ARP (opportunistic)
            string mac = GetMacForIP(ip) ?? "";

            // 4) Optional TCP port checks
            List<int> open = new();
            if (opt.PortsToTest.Count > 0)
            {
                open = await TestPortSetAsync(ip, opt.PortsToTest, Math.Max(200, opt.TimeoutMs / 2), Math.Min(64, opt.MaxParallel), ct)
                    .ConfigureAwait(false);
            }

            return new DiscoveryResult
            {
                IP = ip,
                Hostname = host,
                MAC = mac,
                LatencyMs = rtt,
                OpenPorts = open
            };
        }

        // ===== TCP port testing =====

        private static async Task<List<int>> TestPortSetAsync(string host, IReadOnlyList<int> ports, int timeoutMs, int maxParallel, CancellationToken ct)
        {
            var bag = new ConcurrentBag<int>();
            using var gate = new SemaphoreSlim(Math.Max(1, maxParallel));

            var tasks = ports.Select(async p =>
            {
                await gate.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    if (await IsTcpOpenAsync(host, p, timeoutMs, ct).ConfigureAwait(false))
                        bag.Add(p);
                }
                catch { /* ignore */ }
                finally
                {
                    gate.Release();
                }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
            var list = bag.ToList();
            list.Sort();
            return list;
        }

        private static async Task<bool> IsTcpOpenAsync(string host, int port, int timeoutMs, CancellationToken ct)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);

            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);

            using (cts.Token.Register(() => { try { client.Close(); } catch { } }))
            {
                try
                {
                    await connectTask.ConfigureAwait(false);
                }
                catch
                {
                    return false;
                }
            }
            return client.Connected;
        }

        // ===== CSV export =====

        public static string BuildCsv(IEnumerable<DiscoveryResult> results, bool includeHeaders = true, bool activeOnly = false)
        {
            var sb = new StringBuilder();

            if (includeHeaders)
                sb.AppendLine("IP Address,Hostname,MAC Address,Latency (ms),Open Ports");

            foreach (var r in results)
            {
                if (activeOnly && r.LatencyMs < 0) continue;

                var ports = r.OpenPorts is { Count: > 0 } ? string.Join(";", r.OpenPorts) : "";
                var row = new[]
                {
                    CsvEscape(r.IP),
                    CsvEscape(r.Hostname),
                    CsvEscape(r.MAC),
                    CsvEscape(r.LatencyMs >= 0 ? r.LatencyMs.ToString() : ""),
                    CsvEscape(ports)
                };
                sb.AppendLine(string.Join(",", row));
            }

            return sb.ToString();
        }

        private static string CsvEscape(string input)
        {
            if (input.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0)
                return "\"" + input.Replace("\"", "\"\"") + "\"";
            return input;
        }

        // ===== NIC enumeration / CIDR helpers =====

        public static List<NicSubnet> GetIPv4NicSubnets(bool includeDown = false)
        {
            var list = new List<NicSubnet>();

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!includeDown && nic.OperationalStatus != OperationalStatus.Up) continue;

                var props = nic.GetIPProperties();
                foreach (var ua in props.UnicastAddresses.Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    var ip = ua.Address.ToString();
                    var mask = ua.IPv4Mask?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(mask)) continue;

                    if (TryGetNetworkRange(ip, mask, out var start, out var end, out var prefix))
                    {
                        list.Add(new NicSubnet
                        {
                            AdapterName = nic.Name,
                            IPv4 = ip,
                            Mask = mask,
                            Prefix = prefix,
                            Start = start,
                            End = end
                        });
                    }
                }
            }

            return list;
        }

        public static bool TryParseCidr(string cidr, out uint start, out uint end, out int prefix)
        {
            start = end = 0;
            prefix = 0;

            var parts = cidr.Split('/');
            if (parts.Length != 2) return false;
            if (!TryParseIPv4(parts[0].Trim(), out var baseIp)) return false;
            if (!int.TryParse(parts[1].Trim(), out prefix) || prefix < 0 || prefix > 32) return false;

            var mask = prefix == 0 ? 0u : 0xFFFFFFFFu << (32 - prefix);
            var network = baseIp & mask;
            var broadcast = network | ~mask;

            if (prefix >= 31) { start = network; end = broadcast; }
            else { start = network + 1; end = broadcast - 1; }

            if (start > end) { start = end = network; }
            return true;
        }

        public static bool TryGetNetworkRange(string ip, string mask, out uint start, out uint end, out int prefix)
        {
            start = end = 0;
            prefix = SubnetMaskToPrefix(mask);
            if (prefix < 0) return false;
            if (!TryParseIPv4(ip, out var ipU)) return false;

            var maskU = PrefixToMask(prefix);
            var network = ipU & maskU;
            var broadcast = network | ~maskU;

            if (prefix >= 31) { start = network; end = broadcast; }
            else { start = network + 1; end = broadcast - 1; }

            if (start > end) { start = end = network; }
            return true;
        }

        public static int SubnetMaskToPrefix(string mask)
        {
            if (!TryParseIPv4(mask, out var m)) return -1;
            // must be contiguous: m & (m + 1) == 0xFFFFFFFF? (for inverted). Easier: count leading ones
            int prefix = 0;
            for (int i = 31; i >= 0; i--)
            {
                if (((m >> i) & 1) == 1) prefix++;
                else break;
            }
            // verify remaining bits are zero
            var expected = prefix == 0 ? 0u : 0xFFFFFFFFu << (32 - prefix);
            if (m != expected) return -1;
            return prefix;
        }

        public static uint PrefixToMask(int prefix)
        {
            if (prefix <= 0) return 0u;
            if (prefix >= 32) return 0xFFFFFFFFu;
            return 0xFFFFFFFFu << (32 - prefix);
        }

        public static bool TryParseIPv4(string dotted, out uint value)
        {
            value = 0;
            if (!IPAddress.TryParse(dotted, out var ip) || ip.AddressFamily != AddressFamily.InterNetwork) return false;
            var b = ip.GetAddressBytes(); // big-endian
            value = ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
            return true;
        }

        public static uint ParseIPv4(string dotted)
        {
            TryParseIPv4(dotted, out var v);
            return v;
        }

        public static string ToIP(uint v)
        {
            var b1 = (byte)((v >> 24) & 0xFF);
            var b2 = (byte)((v >> 16) & 0xFF);
            var b3 = (byte)((v >> 8) & 0xFF);
            var b4 = (byte)(v & 0xFF);
            return new IPAddress(new[] { b1, b2, b3, b4 }).ToString();
        }

        // ===== ARP / MAC =====

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int destIp, int srcIp, byte[] macAddr, ref int phyAddrLen);

        private static string? GetMacForIP(string ip)
        {
            try
            {
                var addr = IPAddress.Parse(ip);
                var bytes = addr.GetAddressBytes();
                // IPHlpApi expects IPv4 as Int32 in host order
                int dest = BitConverter.ToInt32(bytes.Reverse().ToArray(), 0);

                byte[] mac = new byte[6];
                int len = mac.Length;
                int res = SendARP(dest, 0, mac, ref len);
                if (res != 0 || len == 0) return null;

                return string.Join(":", mac.Take(len).Select(b => b.ToString("X2")));
            }
            catch
            {
                return null;
            }
        }
    }
}
