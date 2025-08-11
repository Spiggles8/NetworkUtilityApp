using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetworkUtilityApp.Controllers;

namespace NetworkUtilityApp.Tabs
{
    public partial class TabDiagnostics : UserControl
    {
        private readonly DiagnosticsController _diag = new();

        /// <summary>
        /// Optional logger callback provided by host form.
        /// </summary>
        public Action<string, LogLevel>? Log { get; set; }

        public TabDiagnostics()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                WireEventsIfNeeded();
                ApplyDefaultValues();
            }
        }

        /// <summary>
        /// Call from the host after construction if you want to set focus or load defaults.
        /// </summary>
        public void Initialize()
        {
            // optional: focus target box
            var target = Find<TextBox>("txtDiagTarget");
            target?.Focus();
        }

        // ---------------------------
        // Wire UI events
        // ---------------------------
        private void WireEventsIfNeeded()
        {
            Find<Button>("btnPingOnce")?.Apply(b => b.Click += async (_, __) => await OnPingOnceAsync());
            Find<Button>("btnPingStart")?.Apply(b => b.Click += (_, __) => OnPingStart());
            Find<Button>("btnPingStop")?.Apply(b => b.Click += (_, __) => OnPingStop());

            Find<Button>("btnTraceroute")?.Apply(b => b.Click += async (_, __) => await OnTracerouteAsync());
            Find<Button>("btnTracerouteStop")?.Apply(b => b.Click += (_, __) => OnTracerouteStop()); // optional

            Find<Button>("btnTcpTest")?.Apply(b => b.Click += async (_, __) => await OnTcpTestAsync());
        }

        private void ApplyDefaultValues()
        {
            SetIfMissing("numPingIntervalMs", 1000);
            SetIfMissing("numPingTimeoutMs", 2000);
            SetIfMissing("numPingSize", 32);
            SetIfMissing("numPingTtl", 128);

            SetIfMissing("chkPingDontFragment", true);

            SetIfMissing("chkTraceResolve", false);
            SetIfMissing("numTraceMaxHops", 30);
            SetIfMissing("numTracePerHopTimeoutMs", 2000);

            SetIfMissing("numTcpPort", 80);
            SetIfMissing("numTcpTimeoutMs", 1500);
        }

        // ---------------------------
        // Ping
        // ---------------------------
        private async Task OnPingOnceAsync()
        {
            string host = GetText("txtDiagTarget");
            if (string.IsNullOrWhiteSpace(host))
            {
                MessageBox.Show("Enter a host or IP to ping.", "Diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int timeout = GetInt("numPingTimeoutMs", 2000);
            int size = GetInt("numPingSize", 32);
            int ttl = GetInt("numPingTtl", 128);
            bool dontFrag = GetBool("chkPingDontFragment", true);

            var line = await _diag.PingOnceAsync(host, timeout, size, ttl, dontFrag);
            Log?.Invoke(line, line.Contains("SUCCESS", StringComparison.OrdinalIgnoreCase) ? LogLevel.Success :
                             line.Contains("FAIL", StringComparison.OrdinalIgnoreCase) ? LogLevel.Warning : LogLevel.Info);
        }

        private void OnPingStart()
        {
            string host = GetText("txtDiagTarget");
            if (string.IsNullOrWhiteSpace(host))
            {
                MessageBox.Show("Enter a host or IP to ping.", "Diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int interval = GetInt("numPingIntervalMs", 1000);
            int timeout = GetInt("numPingTimeoutMs", 2000);
            int size = GetInt("numPingSize", 32);
            int ttl = GetInt("numPingTtl", 128);
            bool dontFrag = GetBool("chkPingDontFragment", true);

            SetButtonsEnabled(pinging: true);

            _diag.StartContinuousPing(
                host: host,
                intervalMs: interval,
                timeoutMs: timeout,
                size: size,
                ttl: ttl,
                dontFragment: dontFrag,
                onLine: (line, st) => Log?.Invoke(line, Map(st))
            );
        }

        private void OnPingStop()
        {
            _diag.StopContinuousPing();
            SetButtonsEnabled(pinging: false);
        }

        private void SetButtonsEnabled(bool pinging)
        {
            var start = Find<Button>("btnPingStart");
            var stop = Find<Button>("btnPingStop");
            if (start != null) start.Enabled = !pinging;
            if (stop != null) stop.Enabled = pinging;
        }

        // ---------------------------
        // Traceroute
        // ---------------------------
        private async Task OnTracerouteAsync()
        {
            string host = GetText("txtDiagTarget");
            if (string.IsNullOrWhiteSpace(host))
            {
                MessageBox.Show("Enter a host or IP to trace.", "Diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool resolve = GetBool("chkTraceResolve", false);
            int hops = GetInt("numTraceMaxHops", 30);
            int perHopMs = GetInt("numTracePerHopTimeoutMs", 2000);

            var btnRun = Find<Button>("btnTraceroute");
            var btnStop = Find<Button>("btnTracerouteStop");
            if (btnRun != null) btnRun.Enabled = false;
            if (btnStop != null) btnStop.Enabled = true;

            try
            {
                await _diag.TracerouteAsync(
                    host: host,
                    resolveHostnames: resolve,
                    maxHops: hops,
                    perHopTimeoutMs: perHopMs,
                    onLine: (line, st) => Log?.Invoke(line, Map(st))
                );
            }
            finally
            {
                if (btnRun != null) btnRun.Enabled = true;
                if (btnStop != null) btnStop.Enabled = false;
            }
        }

        private void OnTracerouteStop()
        {
            _diag.StopTraceroute();
        }

        // ---------------------------
        // TCP Port Test
        // ---------------------------
        private async Task OnTcpTestAsync()
        {
            string host = GetText("txtTcpHost");
            if (string.IsNullOrWhiteSpace(host))
            {
                // fallback to main target
                host = GetText("txtDiagTarget");
            }

            if (string.IsNullOrWhiteSpace(host))
            {
                MessageBox.Show("Enter a host or IP to test.", "Diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int port = GetInt("numTcpPort", 80);
            int timeout = GetInt("numTcpTimeoutMs", 1500);

            var (ok, ms, err) = await _diag.TestTcpPortAsync(host, port, timeout);
            if (ok)
                Log?.Invoke($"[TCP OPEN] {host}:{port} ({ms}ms)", LogLevel.Success);
            else
                Log?.Invoke($"[TCP CLOSED] {host}:{port} ({ms}ms) {err}", LogLevel.Warning);
        }

        // ---------------------------
        // Helpers
        // ---------------------------
        private T? Find<T>(string name) where T : Control
            => Controls.Find(name, true).FirstOrDefault() as T;

        private string GetText(string name)
            => (Find<TextBox>(name)?.Text ?? string.Empty).Trim();

        private int GetInt(string name, int fallback)
        {
            var nud = Find<NumericUpDown>(name);
            if (nud != null) return (int)nud.Value;
            var txt = Find<TextBox>(name);
            if (txt != null && int.TryParse(txt.Text, out var v)) return v;
            return fallback;
        }

        private bool GetBool(string name, bool fallback)
        {
            var cb = Find<CheckBox>(name);
            if (cb != null) return cb.Checked;
            return fallback;
        }

        private void SetIfMissing(string name, int value)
        {
            var nud = Find<NumericUpDown>(name);
            if (nud != null && nud.Value == 0) nud.Value = value;
            var txt = Find<TextBox>(name);
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text)) txt.Text = value.ToString();
        }

        private void SetIfMissing(string name, bool value)
        {
            var cb = Find<CheckBox>(name);
            if (cb != null) cb.Checked = value;
        }

        private static LogLevel Map(DiagStatus status) => status switch
        {
            DiagStatus.Success => LogLevel.Success,
            DiagStatus.Warning => LogLevel.Warning,
            DiagStatus.Error => LogLevel.Error,
            _ => LogLevel.Info
        };
    }

    public enum LogLevel { Info, Success, Warning, Error }
}
