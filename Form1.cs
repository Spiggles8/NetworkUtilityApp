using NetworkUtilityApp.Services;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Helpers; // Added for FavoriteIpStore

namespace NetworkUtilityApp
{
    /// <summary>
    /// Main application window hosting a WebView2-based HTML UI and a global output log.
    /// </summary>
    public partial class Form1 : Form
    {
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
                if (File.Exists(indexPath)) webView.CoreWebView2.Navigate(indexPath); else AppendToGlobalLog("[ERROR] index.html not found in wwwroot.");
                // Removed generic startup info lines; only meaningful operational events will log.
                foreach (var e1 in AppLog.Snapshot()) AppendToGlobalLog(e1.ToString());
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
                    string target = msg.Substring(5).Trim();
                    if (string.IsNullOrWhiteSpace(target)) return;
                    var result = NetworkController.PingHost(target);
                    PostMessageToWeb($"pingResult:{result}");
                }
                else if (msg.Equals("log:clear", StringComparison.OrdinalIgnoreCase))
                {
                    OnClearLog();
                }
                else if (msg.Equals("adapters:request", StringComparison.OrdinalIgnoreCase))
                {
                    var adapters = NetworkController.GetAdapters();
                    AppLog.Info($"Adapter List Refreshed: Adapters returned = {adapters?.Count ?? 0}");
                    var payload = JsonSerializer.Serialize(adapters, new JsonSerializerOptions { WriteIndented = false });
                    PostMessageToWeb("adapters:data:" + payload);
                }
                else if (msg.StartsWith("adapters:setDhcp:", StringComparison.OrdinalIgnoreCase))
                {
                    var adapter = msg.Substring("adapters:setDhcp:".Length).Trim();
                    if (!string.IsNullOrWhiteSpace(adapter))
                    {
                        var result = NetworkController.SetDhcp(adapter);
                        AppLog.Info(result);
                        PostMessageToWeb("adapters:result:" + result);
                        var adapters = NetworkController.GetAdapters();
                        AppLog.Info($"Adapter List Refreshed (post DHCP): Adapters returned = {adapters?.Count ?? 0}");
                        PostMessageToWeb("adapters:data:" + JsonSerializer.Serialize(adapters));
                    }
                }
                else if (msg.StartsWith("adapters:setStatic:", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = msg.Substring("adapters:setStatic:".Length);
                    var parts = payload.Split('|');
                    if (parts.Length >= 4)
                    {
                        var adapter = parts[0].Trim();
                        var ip = parts[1].Trim();
                        var mask = parts[2].Trim();
                        var gw = parts[3].Trim();
                        if (!string.IsNullOrWhiteSpace(adapter) && !string.IsNullOrWhiteSpace(ip) && !string.IsNullOrWhiteSpace(mask))
                        {
                            var result = NetworkController.SetStatic(adapter, ip, mask, gw);
                            AppLog.Info(result);
                            PostMessageToWeb("adapters:result:" + result);
                            var adapters = NetworkController.GetAdapters();
                            AppLog.Info($"Adapter List Refreshed (post static): Adapters returned = {adapters?.Count ?? 0}");
                            PostMessageToWeb("adapters:data:" + JsonSerializer.Serialize(adapters));
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
                    AppLog.Info("Favorites loaded.");
                    PostMessageToWeb("favorites:data:" + JsonSerializer.Serialize(favs));
                }
                else if (msg.StartsWith("favorites:save:", StringComparison.OrdinalIgnoreCase))
                {
                    var data = msg.Substring("favorites:save:".Length);
                    var parts = data.Split('|');
                    if (parts.Length >= 4 && int.TryParse(parts[0], out var slot) && slot>=1 && slot<=4)
                    {
                        var ip = parts[1].Trim();
                        var subnet = parts[2].Trim();
                        var gateway = parts[3].Trim();
                        if (!string.IsNullOrWhiteSpace(ip) && !string.IsNullOrWhiteSpace(subnet))
                        {
                            FavoriteIpStore.Save(slot, new FavoriteIpEntry { Ip = ip, Subnet = subnet, Gateway = string.IsNullOrWhiteSpace(gateway)?"":gateway });
                            AppLog.Info($"Saved favorite slot {slot}: {ip}/{subnet}{(string.IsNullOrWhiteSpace(gateway)?"":"/"+gateway)}");
                            PostMessageToWeb("favorites:save:Saved favorite slot " + slot);
                        }
                        else
                        {
                            AppLog.Info("Invalid favorite data provided.");
                            PostMessageToWeb("favorites:save:Invalid favorite data.");
                        }
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
                webView.CoreWebView2.PostWebMessageAsString(text);
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
            // Removed PostMessageToWeb("log:" + e.ToString()); to avoid stray log text in Web UI
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
                using var dlg = new SaveFileDialog
                {
                    Title = "Save Log As",
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FileName = $"NetworkUtilityLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    OverwritePrompt = true
                };
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var snapshot = AppLog.Snapshot();
                File.WriteAllLines(dlg.FileName, snapshot.Select(s => s.ToString()));
                AppLog.Info($"Log saved to: {dlg.FileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save log.\n\n" + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private const string WindowStateFile = "window_state.json";
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
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkUtilityApp", WindowStateFile);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                var state = new {
                    Width = Width,
                    Height = Height,
                    Left = Left,
                    Top = Top,
                    Maximized = WindowState == FormWindowState.Maximized
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
        }
    }
}