using System.Diagnostics;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// Diagnostics tab view (partial). Shared state, UI wiring, and common helpers.
    /// </summary>
    public partial class DiagnosticsView : System.Windows.Controls.UserControl
    {
        // Shared cancellation token for currently running diagnostic process
        private CancellationTokenSource? _cts;
        // Handle to the active external process so we can cancel/kill it on demand
        private Process? _activeProcess;

        public DiagnosticsView()
        {
            InitializeComponent();
            Loaded += OnLoaded; // defer wiring so controls exist
        }

        // Wire UI buttons to their async actions once the view is loaded
        private void OnLoaded(object? sender, System.Windows.RoutedEventArgs e)
        {
            BtnRunTraceroute.Click += async (_, __) => await RunTracerouteAsync();
            BtnNslookupRun.Click += async (_, __) => await RunNslookupAsync();
            BtnPathpingRun.Click += async (_, __) => await RunPathpingAsync();
            BtnCancel.Click += (_, __) => CancelActive();
            BtnPing.Click += async (_, __) => await RunPingOnceAsync();
        }

        // Append a line to the diagnostics output textbox and keep it scrolled to end
        private void Append(string line)
        {
            if (TxtOutput == null) return;
            TxtOutput.AppendText((TxtOutput.Text.Length == 0 ? string.Empty : System.Environment.NewLine) + line);
            TxtOutput.ScrollToEnd();
        }

        // Cancel the active tool run: signal cancellation and kill the process if still alive
        private void CancelActive()
        {
            try
            {
                _cts?.Cancel();
                if (_activeProcess != null && !_activeProcess.HasExited) _activeProcess.Kill(true);
            }
            catch { /* best-effort cancel; ignore failures */ }
        }
    }
}
