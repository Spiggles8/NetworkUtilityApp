using NetworkUtilityApp.Services;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Helpers; // Added for FavoriteIpStore
using System.Diagnostics;
using NetworkUtilityApp.Ui;


namespace NetworkUtilityApp
{
    /// <summary>
    /// Main application window hosting a WebView2-based HTML UI and a global output log.
    /// </summary>
    public partial class Form1 : Form
    {
        private Process? _currentDiagProcess; // active diagnostic process
        private string? _currentDiagTag;      // tag of active diagnostic (TRACE, NSLOOKUP, PATHPING)
        private readonly object _diagLock = new();
        private CancellationTokenSource? _discCts;
        private int _discTotal;
        private int _discScanned;
        private int _discActive;
        private Stopwatch? _discSw;
        private readonly List<object> _discResults = [];
        private readonly Dictionary<string,string> _arpCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };
        private const int DiscoveryMaxParallel = 64; // throttle level
        private const int DiscoveryPingTimeoutMs = 400; // faster cancel responsiveness
        private volatile bool _discCancelled; // indicates user requested cancellation

        /// <summary>
        /// Initializes the form, wires global log button handlers, and subscribes to AppLog updates.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            AppLog.EntryAdded += OnAppLogEntryAdded;

            // Wire clear / save buttons (if present in designer)
            if (btnGlobalLogClear is not null)
                btnGlobalLogClear.Click += (_, __) => OnClearLog();
            if (btnGlobalLogSave is not null)
                btnGlobalLogSave.Click += (_, __) => OnSaveLog();
        }

        private static string GetDefaultLogPath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkUtilityApp");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "NetworkUtility_OutputLog.txt");
        }

        /// <summary>
        /// Form load handler. Seeds the global log textbox from the existing log snapshot
        /// and runs any required per-tab initialization.
        /// </summary>
        private async void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                var indexPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html");
                if (File.Exists(indexPath)) webView.CoreWebView2.Navigate(indexPath); else AppendToGlobalLog("index.html not found in wwwroot.");

                // Load persisted output log into the textbox (do not re-log to AppLog to avoid duplicates)
                var defaultLog = GetDefaultLogPath();
                if (File.Exists(defaultLog))
                {
                    try
                    {
                        var existing = File.ReadAllText(defaultLog);
                        if (!string.IsNullOrEmpty(existing) && txtGlobalLog != null)
                        {
                            var cleaned = existing.TrimEnd('\r','\n');
                            txtGlobalLog.Text = cleaned;
                            txtGlobalLog.SelectionStart = txtGlobalLog.TextLength;
                            txtGlobalLog.ScrollToCaret();
                        }
                    }
                    catch { /* ignore load errors */ }
                }

                LoadSettings();
                ApplyTheme();

                // Log application open event (this will append to UI via AppLog event once)
                AppLog.Info($"App Opened at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize WebView2 / form.\n\n" + ex.Message,
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var msg = e.TryGetWebMessageAsString();
                if (string.IsNullOrWhiteSpace(msg)) return;

                // Simple command router (extend protocol as needed)
                if (msg.StartsWith("ping:", StringComparison.OrdinalIgnoreCase))
                {
                    string target = msg[5..].Trim();
                    if (string.IsNullOrWhiteSpace(target)) return;
                    var result = NetworkController.PingHost(target);
                    AppLog.Info(result);
                }
                else if (msg.Equals("log:clear", StringComparison.OrdinalIgnoreCase))
                {
                    OnClearLog();
                }
                else if (msg.Equals("adapters:request", StringComparison.OrdinalIgnoreCase))
                {
                    var adapters = NetworkController.GetAdapters();
                    AppLog.Success($"Adapter List Refreshed: Adapters returned = {adapters?.Count ?? 0}");
                    var payload = JsonSerializer.Serialize(adapters, JsonOpts);
                    PostMessageToWeb("adapters:data:" + payload);
                }
                else if (msg.StartsWith("adapters:setDhcp:", StringComparison.OrdinalIgnoreCase))
                {
                    var adapter = msg["adapters:setDhcp:".Length..].Trim();
                    if (string.IsNullOrWhiteSpace(adapter))
                    {
                        var err = "[setDHCP] No adapter selected.";
                        AppLog.Error(err);
                        PostMessageToWeb("adapters:result:" + err);
                        try { AlertForm.ShowError(this, "No adapter selected.", "Set DHCP", _settings.DarkMode); } catch { }
                        return;
                    }
                    var result = NetworkController.SetDhcp(adapter);
                    AppLog.Info(result);
                    PostMessageToWeb("adapters:result:" + result);
                    var adapters = NetworkController.GetAdapters();
                    AppLog.Success($"Adapter List Refreshed (post DHCP): Adapters returned = {adapters?.Count ?? 0}");
                    PostMessageToWeb("adapters:data:" + JsonSerializer.Serialize(adapters, JsonOpts));
                }
                else if (msg.StartsWith("adapters:setStatic:", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = msg["adapters:setStatic:".Length..];
                    var parts = payload.Split('|');
                    if (parts.Length >= 4)
                    {
                        var adapter = parts[0].Trim();
                        var ip = parts[1].Trim();
                        var mask = parts[2].Trim();
                        var gw = parts[3].Trim();

                        // Explicit error when no adapter selected
                        if (string.IsNullOrWhiteSpace(adapter))
                        {
                            var err = "[setStatic] No adapter selected.";
                            AppLog.Error(err);
                            PostMessageToWeb("adapters:result:" + err);
                            try { AlertForm.ShowError(this, "No adapter selected.", "Set Static", _settings.DarkMode); } catch { }
                            return;
                        }

                        if (!string.IsNullOrWhiteSpace(ip) && !string.IsNullOrWhiteSpace(mask))
                        {
                            var result = NetworkController.SetStatic(adapter, ip, mask, gw);
                            AppLog.Info(result);
                            PostMessageToWeb("adapters:result:" + result);
                            var adapters = NetworkController.GetAdapters();
                            AppLog.Success($"Adapter List Refreshed (post static): Adapters returned = {adapters?.Count ?? 0}");
                            PostMessageToWeb("adapters:data:" + JsonSerializer.Serialize(adapters, JsonOpts));
                        }
                    }
                }
                else if (msg.Equals("favorites:request", StringComparison.OrdinalIgnoreCase))
                {
                    var favs = Enumerable.Range(1,4).Select(slot => new {
                        slot,
                        ip = FavoriteIpStore.Get(slot)?.Ip,
                        subnet = FavoriteIpStore.Get(slot)?.Subnet,
                        gateway = FavoriteIpStore.Get(slot)?.Gateway
                    });
                    // Suppress startup noise: don't log on favorites load requests
                    PostMessageToWeb("favorites:data:" + JsonSerializer.Serialize(favs, JsonOpts));
                }
                else if (msg.StartsWith("favorites:save:", StringComparison.OrdinalIgnoreCase))
                {
                    var data = msg["favorites:save:".Length..];
                    var parts = data.Split('|');
                    if (parts.Length >= 4 && int.TryParse(parts[0], out var slot) && slot>=1 && slot<=4)
                    {
                        var ip = parts[1].Trim();
                        var subnet = parts[2].Trim();
                        var gateway = parts[3].Trim();
                        if (!string.IsNullOrWhiteSpace(ip) && !string.IsNullOrWhiteSpace(subnet))
                        {
                            FavoriteIpStore.Save(slot, new FavoriteIpEntry { Ip = ip, Subnet = subnet, Gateway = string.IsNullOrWhiteSpace(gateway)?"":gateway });
                            AppLog.Success($"Saved favorite slot {slot}: {ip}/{subnet}{(string.IsNullOrWhiteSpace(gateway)?"":"/"+gateway)}");
                            PostMessageToWeb("favorites:save:Saved favorite slot " + slot);
                        }
                        else
                        {
                            AppLog.Error("Invalid favorite data provided.");
                            PostMessageToWeb("favorites:save:Invalid favorite data.");
                        }
                    }
                }
                else if (msg.Equals("diagnostics:cancel", StringComparison.OrdinalIgnoreCase))
                {
                    bool canceled = false;
                    string? tag;
                    lock (_diagLock)
                    {
                        tag = _currentDiagTag;
                        if (_currentDiagProcess != null && !_currentDiagProcess.HasExited)
                        {
                            try
                            {
                                _currentDiagProcess.Kill(true);
                                canceled = true;
                            }
                            catch (Exception ex)
                            {
                                AppLog.Error("Failed to kill process: " + ex.Message);
                            }
                        }
                        _currentDiagProcess = null;
                        _currentDiagTag = null;
                    }
                    AppLog.Info(canceled && tag!=null ? $"Cancelled {tag} command." : "No active diagnostic command to cancel.");
                }
                else if (msg.StartsWith("trace:", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = msg["trace:".Length..];
                    var parts = payload.Split('|');
                    var target = parts[0].Trim();
                    bool resolve = parts.Length > 1 && parts[1].Equals("resolve", StringComparison.OrdinalIgnoreCase);
                    if (string.IsNullOrWhiteSpace(target)) return;
                    lock (_diagLock)
                    {
                        if (_currentDiagProcess != null && !_currentDiagProcess.HasExited)
                        {
                            AppLog.Warn("[TRACE] Another diagnostic is running. Cancel it first.");
                            return;
                        }
                        _currentDiagTag = "TRACE";
                    }
                    AppLog.Info($"[TRACE] Starting traceroute: {target} (resolve names: {resolve})");
                    _ = Task.Run(() => StreamTrace(target, resolve));
                }
                else if (msg.StartsWith("nslookup:", StringComparison.OrdinalIgnoreCase))
                {
                    var target = msg["nslookup:".Length..].Trim();
                    if (string.IsNullOrWhiteSpace(target)) return;
                    lock (_diagLock)
                    {
                        if (_currentDiagProcess != null && !_currentDiagProcess.HasExited)
                        {
                            AppLog.Warn("[NSLOOKUP] Another diagnostic is running. Cancel it first.");
                            return;
                        }
                        _currentDiagTag = "NSLOOKUP";
                    }
                    AppLog.Info($"[NSLOOKUP] Starting: {target}");
                    _ = Task.Run(() => StreamTool("nslookup", target, 60000, "NSLOOKUP"));
                }
                else if (msg.StartsWith("pathping:", StringComparison.OrdinalIgnoreCase))
                {
                    var target = msg["pathping:".Length..].Trim();
                    if (string.IsNullOrWhiteSpace(target)) return;
                    lock (_diagLock)
                    {
                        if (_currentDiagProcess != null && !_currentDiagProcess.HasExited)
                        {
                            AppLog.Warn("[PATHPING] Another diagnostic is running. Cancel it first.");
                            return;
                        }
                        _currentDiagTag = "PATHPING";
                    }
                    AppLog.Info($"[PATHPING] Starting: {target}");
                    _ = Task.Run(() => StreamTool("pathping", $"-n {target}", 180000, "PATHPING"));
                }
                else if (msg.StartsWith("log:info:", StringComparison.OrdinalIgnoreCase))
                {
                    var text = msg["log:info:".Length..].Trim();
                    if (!string.IsNullOrWhiteSpace(text)) AppLog.Info(text);
                }
                else if (msg.Equals("disc:adapters", StringComparison.OrdinalIgnoreCase))
                {
                    var adapters = NetworkController.GetAdapters() ?? [];
                    var payload = JsonSerializer.Serialize(adapters, JsonOpts);
                    PostMessageToWeb("disc:adapters:" + payload);
                }
                else if (msg.StartsWith("disc:start:", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = msg["disc:start:".Length..].Split('|');
                    if (parts.Length < 3) return;
                    var adapter = parts[0].Trim();
                    var startIp = parts[1].Trim();
                    var endIp = parts[2].Trim();
                    if (string.IsNullOrWhiteSpace(startIp) || string.IsNullOrWhiteSpace(endIp)) return;
                    if (_discCts != null)
                    {
                        AppLog.Warn("[DISC] Scan already running. Cancel first.");
                        return;
                    }
                    if (!Helpers.ValidationHelper.IsValidIPv4(startIp) || !Helpers.ValidationHelper.IsValidIPv4(endIp))
                    {
                        AppLog.Error("[DISC] Invalid start or end IP.");
                        return;
                    }
                    long startVal = IpToLong(startIp);
                    long endVal = IpToLong(endIp);
                    if (endVal < startVal)
                    {
                        AppLog.Warn("[DISC] End IP must be >= Start IP.");
                        return;
                    }
                    _discCts = new CancellationTokenSource();
                    _discScanned = 0; _discActive = 0; _discResults.Clear();
                    _discTotal = (int)Math.Min(int.MaxValue, endVal - startVal + 1);
                    _discSw = Stopwatch.StartNew();
                    PostMessageToWeb("disc:clear");
                    AppLog.Info($"[DISC] Scan starting: {startIp} - {endIp} (total {_discTotal}, parallel {DiscoveryMaxParallel})");
                    // Prime ARP cache once
                    LoadArpTableInto(_arpCache);
                    // Prepare IP list
                    var ips = new System.Collections.Generic.List<string>(_discTotal);
                    for (long ipVal = startVal; ipVal <= endVal; ipVal++) ips.Add(LongToIp(ipVal));
                    _discCancelled = false;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var sem = new SemaphoreSlim(DiscoveryMaxParallel);
                            var tasks = ips.Select(async ip =>
                            {
                                await sem.WaitAsync(_discCts.Token);
                                try
                                {
                                    _discCts.Token.ThrowIfCancellationRequested();
                                    var probe = await ProbeAsyncWithMac(ip);
                                    System.Threading.Interlocked.Increment(ref _discScanned);
                                    if (probe.IsActive) System.Threading.Interlocked.Increment(ref _discActive);
                                    lock (_discResults) _discResults.Add(probe);
                                    if (probe.IsActive)
                                        PostMessageToWeb("disc:result:" + JsonSerializer.Serialize(probe, JsonOpts));
                                    UpdateDiscStats();
                                }
                                catch (OperationCanceledException) { }
                                catch (Exception exProbe) { AppLog.Error("[DISC ERROR] " + exProbe.Message); }
                                finally { sem.Release(); }
                            }).ToArray();
                            await Task.WhenAll(tasks);
                            if (!_discCancelled && !_discCts.IsCancellationRequested)
                                AppLog.Success($"[DISC] Scan completed. Active hosts: {_discActive}");
                            else
                                AppLog.Info($"[DISC] Scan terminated early. Scanned {_discScanned} / {_discTotal} hosts.");
                            UpdateDiscStats();
                        }
                        catch (OperationCanceledException)
                        {
                            AppLog.Info("[DISC] Scan cancelled.");
                            UpdateDiscStats();
                        }
                        catch (Exception ex2)
                        {
                            AppLog.Error("[DISC ERROR] " + ex2.Message);
                        }
                        finally
                        {
                            _discSw?.Stop();
                            _discCts?.Dispose();
                            _discCts = null;
                        }
                    });
                }
                else if (msg.Equals("disc:cancel", StringComparison.OrdinalIgnoreCase))
                {
                    if (_discCts == null)
                    {
                        AppLog.Info("[DISC] No active scan to cancel.");
                        PostMessageToWeb("disc:cancelled");
                    }
                    else
                    {
                        _discCancelled = true;
                        _discCts.Cancel();
                        AppLog.Info("[DISC] Cancellation requested.");
                        PostMessageToWeb("disc:cancelled");
                    }
                }
                else if (msg.Equals("disc:save", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (_discResults.Count == 0)
                        {
                            AppLog.Info("[DISC] Nothing to save.");
                            return;
                        }
                        using var dlg = new SaveFileDialog
                        {
                            Title = "Save Discovery Results",
                            Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                            FileName = $"Discovery_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                            OverwritePrompt = true
                        };
                        if (dlg.ShowDialog(this) != DialogResult.OK) return;
                        var lines = new System.Collections.Generic.List<string>
                        {
                            $"Discovery Results - {DateTime.Now}",
                            $"Total Scanned: {_discScanned}",
                            $"Active Hosts: {_discActive}",
                            "",
                            "IP\tHostname\tLatencyMs\tMAC\tVendor\tStatus"
                        };
                        lock (_discResults)
                        {
                            foreach (var o in _discResults)
                            {
                                var p = (DiscProbe)o;
                                lines.Add($"{p.Ip}\t{(p.Hostname??"").Replace('\t',' ')}\t{(p.LatencyMs?.ToString()??"")}\t{p.Mac}\t{p.Manufacturer}\t{p.Status}");
                            }
                        }
                        System.IO.File.WriteAllLines(dlg.FileName, lines);
                        AppLog.Success($"[disc] Results saved to: {dlg.FileName}");
                    }
                    catch (Exception exSave)
                    {
                        AppLog.Error("Failed to save: " + exSave.Message);
                    }
                }
                else if (msg.Equals("disc:reset", StringComparison.OrdinalIgnoreCase))
                {
                    ResetDiscoveryState(includeArp: true);
                }
                else if (msg.Equals("settings:request", StringComparison.OrdinalIgnoreCase))
                {
                    LoadSettings();
                    PostMessageToWeb("settings:data:" + JsonSerializer.Serialize(_settings));
                }
                else if (msg.StartsWith("settings:save:", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = msg["settings:save:".Length..].Split('|');
                    // New format: ||parallel|timeout (first two entries may be empty)
                    if (payload.Length >= 4)
                    {
                        // Drop ping/trace feature flags
                        if (int.TryParse(payload[2], out var par) && par >=1 && par <= 512) _settings.DiscoveryParallel = par; else _settings.DiscoveryParallel = null;
                        if (int.TryParse(payload[3], out var to) && to >= 50 && to <= 5000) _settings.DiscoveryTimeout = to; else _settings.DiscoveryTimeout = null;
                        if (payload.Length >= 5)
                        {
                            _settings.DarkMode = payload[4].Equals("dark", StringComparison.OrdinalIgnoreCase) || payload[4].Equals("true", StringComparison.OrdinalIgnoreCase);
                        }
                        SaveSettings();
                        ApplyTheme();
                        PostMessageToWeb("settings:save:Settings saved.");
                        AppLog.Success("[SETTINGS] Saved.");
                    }
                }
            }
            catch (Exception ex)
            {
                AppendToGlobalLog("[ERROR] Message handling failed: " + ex.Message);
            }
        }

        private void PostMessageToWeb(string text)
        {
            try
            {
                if (IsDisposed) return;
                if (webView.InvokeRequired)
                {
                    webView.BeginInvoke(new Action<string>(PostMessageToWeb), text);
                    return;
                }
                webView.CoreWebView2?.PostWebMessageAsString(text);
            }
            catch { /* ignore */ }
        }

        /// <summary>
        /// AppLog event callback. Marshals to UI thread if required and appends
        /// the new entry to the global log textbox.
        /// </summary>
        private void OnAppLogEntryAdded(object? sender, AppLog.LogEntry e)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object?, AppLog.LogEntry>(OnAppLogEntryAdded), sender, e);
                return;
            }
            AppendToGlobalLog(e.ToString());
        }

        private void AppendToGlobalLog(string line)
        {
            if (txtGlobalLog is null) return;
            if (txtGlobalLog.TextLength == 0) txtGlobalLog.Text = line;
            else txtGlobalLog.AppendText(Environment.NewLine + line);
            txtGlobalLog.SelectionStart = txtGlobalLog.TextLength;
            txtGlobalLog.ScrollToCaret();
        }

        private void OnClearLog()
        {
            AppLog.Clear();
            txtGlobalLog?.Clear();
            var snapshot = AppLog.Snapshot();
            var last = snapshot.Count > 0 ? snapshot[^1] : null;
            if (last is not null) AppendToGlobalLog(last.ToString());
            PostMessageToWeb("log:cleared");
        }

        private void OnSaveLog()
        {
            try
            {
                var defaultPath = GetDefaultLogPath();
                var defaultDir = Path.GetDirectoryName(defaultPath)!;
                Directory.CreateDirectory(defaultDir);

                using var dlg = new SaveFileDialog
                {
                    Title = "Save Output Log As",
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FileName = Path.GetFileName(defaultPath),
                    InitialDirectory = defaultDir,
                    OverwritePrompt = true
                };

                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                // Log the save event first so it is included in the saved file
                AppLog.Success($"[LOG] Saved to: {dlg.FileName} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                var snapshot = AppLog.Snapshot();
                File.WriteAllLines(dlg.FileName, snapshot.Select(s => s.ToString()));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save log.\n\n" + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private const string WindowStateFile = "window_state.json";
        private const string SettingsFile = "settings.json";
        private class AppSettings { public bool PingContinuous {get;set;} = false; public bool TraceResolve {get;set;} = true; public int? DiscoveryParallel {get;set;} = null; public int? DiscoveryTimeout {get;set;} = null; public bool DarkMode {get;set;} = false; }
        private AppSettings _settings = new();

        private void Form1_Resize(object? sender, EventArgs e)
        {
            try
            {
                if (splitMain is null || splitMain.IsSplitterFixed) return;
                int total = splitMain.Height;
                int topMin = splitMain.Panel1MinSize;
                int bottomMin = splitMain.Panel2MinSize;
                int desiredTop = (int)(total * 0.62);
                if (desiredTop < topMin) desiredTop = topMin;
                if (total - desiredTop < bottomMin) desiredTop = total - bottomMin;
                splitMain.SplitterDistance = desiredTop;
            }
            catch { /* ignore */ }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                var now = DateTime.Now;
                var pathLog = GetDefaultLogPath();

                // Log save event first so it appears in the saved file
                AppLog.Success($"Saved to: {pathLog} at {now:yyyy-MM-dd HH:mm:ss}");
                // Then log the app closing event
                AppLog.Info($"App closed at {now:yyyy-MM-dd HH:mm:ss}");

                // Persist the full snapshot including the two lines above
                var snapshot = AppLog.Snapshot();
                File.WriteAllLines(pathLog, snapshot.Select(s => s.ToString()));

                // Persist window state
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkUtilityApp", WindowStateFile);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                var state = new {
                    Width,
                    Height,
                    Left,
                    Top,
                    Maximized = WindowState == FormWindowState.Maximized,
                    PingContinuous = false,
                    TraceResolve = true
                };
                File.WriteAllText(path, JsonSerializer.Serialize(state));
            }
            catch { }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkUtilityApp", WindowStateFile);
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var state = JsonSerializer.Deserialize<WindowStateSnapshot>(json);
                    if (state != null)
                    {
                        Width = state.Width;
                        Height = state.Height;
                        Left = state.Left;
                        Top = state.Top;
                        if (state.Maximized) WindowState = FormWindowState.Maximized;
                    }
                }
            }
            catch { }
        }

        private class WindowStateSnapshot
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int Left { get; set; }
            public int Top { get; set; }
            public bool Maximized { get; set; }
            public bool PingContinuous { get; set; } // NEW
            public bool TraceResolve { get; set; }   // NEW
        }

        private void StreamTrace(string target, bool resolve)
        {
            Process? localProc = null;
            try
            {
                var args = new System.Collections.Generic.List<string>();
                if (!resolve) args.Add("-d");
                args.Add("-h"); args.Add("30");
                args.Add("-w"); args.Add("4000");
                args.Add(target);
                var psi = new ProcessStartInfo("tracert", string.Join(" ", args))
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                localProc = Process.Start(psi);
                lock (_diagLock) { _currentDiagProcess = localProc; }
                if (localProc == null) throw new InvalidOperationException("Failed to start tracert.");
                using var reader = localProc.StandardOutput;
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    AppLog.Info(line.TrimEnd());
                }
                string err = localProc.StandardError.ReadToEnd();
                localProc.WaitForExit();
                if (!string.IsNullOrWhiteSpace(err)) AppLog.Error("[TRACE ERROR] " + err.Trim());
                AppLog.Success($"[TRACE] Completed: {target}");
            }
            catch (Exception ex)
            {
                AppLog.Error("[TRACE] Traceroute failed: " + ex.Message);
            }
            finally
            {
                lock (_diagLock)
                {
                    if (_currentDiagProcess == localProc) { _currentDiagProcess = null; _currentDiagTag = null; }
                }
            }
        }

        private void StreamTool(string fileName, string args, int timeoutMs, string tag)
        {
            Process? localProc = null;
            try
            {
                var psi = new ProcessStartInfo(fileName, args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                localProc = Process.Start(psi);
                lock (_diagLock) { _currentDiagProcess = localProc; }
                if (localProc == null) throw new InvalidOperationException($"Failed to start {fileName}.");
                using var reader = localProc.StandardOutput;
                string? line;
                var sw = Stopwatch.StartNew();
                while (!reader.EndOfStream)
                {
                    if (timeoutMs > 0 && sw.ElapsedMilliseconds > timeoutMs)
                    {
                        AppLog.Warn($"[{tag}] Timeout exceeded.");
                        try { localProc.Kill(true); } catch { }
                        return;
                    }
                    line = reader.ReadLine();
                    if (line == null) break;
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    AppLog.Info(line.TrimEnd());
                }
                string err = localProc.StandardError.ReadToEnd();
                localProc.WaitForExit();
                if (!string.IsNullOrWhiteSpace(err)) AppLog.Error($"[{tag} ERROR] " + err.Trim());
                AppLog.Success($"[{tag}] Completed.");
            }
            catch (Exception ex)
            {
                AppLog.Error($"[ERROR] {tag} failed: {ex.Message}");
            }
            finally
            {
                lock (_diagLock)
                {
                    if (_currentDiagProcess == localProc) { _currentDiagProcess = null; _currentDiagTag = null; }
                }
            }
        }

        private static long IpToLong(string ip)
        {
            var parts = ip.Split('.');
            // Parse as byte to avoid sign-extension warnings, then upcast
            var b0 = byte.Parse(parts[0]);
            var b1 = byte.Parse(parts[1]);
            var b2 = byte.Parse(parts[2]);
            var b3 = byte.Parse(parts[3]);
            return ((long)b0 << 24) | ((long)b1 << 16) | ((long)b2 << 8) | b3;
        }
        private static string LongToIp(long v)
        {
            return string.Join('.', (v >> 24) & 255, (v >> 16) & 255, (v >> 8) & 255, v & 255);
        }
        private class DiscProbe
        {
            public string Ip { get; set; } = "";
            public bool IsActive { get; set; }
            public long? LatencyMs { get; set; }
            public string Hostname { get; set; } = "";
            public string Mac { get; set; } = "";
            public string Manufacturer { get; set; } = "";
            public string Status { get; set; } = "No Reply";
        }
        private async Task<DiscProbe> ProbeAsyncWithMac(string ip)
        {
            var pr = new DiscProbe { Ip = ip };
            try
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = await ping.SendPingAsync(ip, DiscoveryPingTimeoutMs);
                if (_discCancelled || (_discCts?.IsCancellationRequested ?? false)) return pr;
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    pr.IsActive = true; pr.Status = "Active"; pr.LatencyMs = reply.RoundtripTime;
                    if (_discCancelled || (_discCts?.IsCancellationRequested ?? false)) return pr;
                    try { var host = await System.Net.Dns.GetHostEntryAsync(ip); pr.Hostname = host.HostName; } catch { }
                    if (_discCancelled || (_discCts?.IsCancellationRequested ?? false)) return pr;
                    if (_arpCache.TryGetValue(ip, out var mac)) pr.Mac = mac; else { if (!_discCancelled) { LoadArpTableInto(_arpCache); _arpCache.TryGetValue(ip, out mac); } pr.Mac = mac??""; }
                    pr.Manufacturer = ResolveManufacturer(pr.Mac);
                }
            }
            catch { }
            return pr;
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
                    var parts = trimmed.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && ValidationHelper.IsValidIPv4(parts[0]))
                        map[parts[0]] = parts[1];
                }
            }
            catch { }
        }
        private static string ResolveManufacturer(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac)) return "";
            var hex = new string([.. mac.ToUpperInvariant().Replace("-"," ").Replace(":"," ").Where(Uri.IsHexDigit)]);
            if (hex.Length < 6) return "";
            var oui = hex[..6];
            var vendors = new System.Collections.Generic.Dictionary<string,string>(StringComparer.OrdinalIgnoreCase)
            {
                { "000C29", "VMware" }, { "0003FF", "Microsoft" }, { "001C14", "Intel" },
                { "F4F5E8", "Intel" }, { "B827EB", "Raspberry Pi" }, { "F01FAF", "Apple" },
                { "A45E60", "Apple" }, { "D83062", "Apple" }, { "00E04C", "Realtek" }, { "001E49", "Cisco" }
            };
            return vendors.TryGetValue(oui, out var v) ? v : "Unknown";
        }

        private void UpdateDiscStats()
        {
            var eta = "--:--:--";
            if (_discSw != null && _discScanned > 0 && _discScanned < _discTotal)
            {
                double per = _discSw.Elapsed.TotalSeconds / _discScanned;
                var remaining = TimeSpan.FromSeconds(Math.Max(0, (_discTotal - _discScanned) * per));
                eta = $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
            }
            PostMessageToWeb($"disc:stats:{_discScanned}|{_discTotal}|{_discActive}|{eta}");
        }

        private void ResetDiscoveryState(bool includeArp)
        {
            try { _discCancelled = true; _discCts?.Cancel(); } catch { }
            _discSw?.Stop();
            _discScanned = 0;
            _discActive = 0;
            _discTotal = 0;
            lock (_discResults) _discResults.Clear();
            if (includeArp) _arpCache.Clear();
            PostMessageToWeb("disc:clear");
            AppLog.Info("State reset.");
        }

        private void LoadSettings()
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkUtilityApp", SettingsFile);
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var s = JsonSerializer.Deserialize<AppSettings>(json);
                    if (s != null) _settings = s;
                }
            }
            catch { }
        }
        private void SaveSettings()
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkUtilityApp");
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, SettingsFile);
                File.WriteAllText(path, JsonSerializer.Serialize(_settings));
            }
            catch { }
        }
        private void ApplyTheme()
        {
            try
            {
                bool dark = _settings.DarkMode;
                var back = dark ? Color.FromArgb(30,30,30) : SystemColors.Window;
                var fore = dark ? Color.FromArgb(230,230,230) : SystemColors.WindowText;
                if (txtGlobalLog != null)
                {
                    txtGlobalLog.BackColor = back;
                    txtGlobalLog.ForeColor = fore;
                }
                if (pnlGlobalLogInner != null)
                {
                    pnlGlobalLogInner.BackColor = dark ? Color.FromArgb(24,24,24) : Color.White;
                }
                if (lblGlobalLog != null)
                {
                    lblGlobalLog.ForeColor = fore;
                }
                if (pnlUnderline != null)
                {
                    pnlUnderline.BackColor = dark ? Color.FromArgb(60,60,60) : Color.FromArgb(224,224,224);
                }
                if (splitMain != null)
                {
                    splitMain.BackColor = dark ? Color.FromArgb(32,32,32) : Color.White;
                }
                if (flowGlobalLogButtons != null)
                {
                    flowGlobalLogButtons.BackColor = dark ? Color.FromArgb(24,24,24) : Color.White;
                }
                // Adjust buttons for dark contrast
                static void AdjustButton(Button b, Color normal, Color border, Color text)
                {
                    if (b == null) return;
                    b.BackColor = normal;
                    b.FlatAppearance.BorderColor = border;
                    b.ForeColor = text;
                }
                if (btnGlobalLogClear != null)
                {
                    if (dark) AdjustButton(btnGlobalLogClear, Color.FromArgb(180,140,0), Color.FromArgb(160,120,0), Color.White);
                    else AdjustButton(btnGlobalLogClear, Color.FromArgb(255,199,0), Color.FromArgb(214,167,0), Color.Black);
                }
                if (btnGlobalLogSave != null)
                {
                    if (dark) AdjustButton(btnGlobalLogSave, Color.FromArgb(15,95,15), Color.FromArgb(12,70,12), Color.White);
                    else AdjustButton(btnGlobalLogSave, Color.FromArgb(20,111,20), Color.FromArgb(15,85,15), Color.White);
                }
            }
            catch { }
        }
    }
}