using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows;
using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Helpers;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// Discovery tab view (partial). Wires lifecycle and events; scan/resolve/export split into partials.
    /// </summary>
    public partial class DiscoveryView : System.Windows.Controls.UserControl
    {
        private CancellationTokenSource? _cts;
        private readonly ObservableCollection<ProbeRow> _rows = [];
        private Stopwatch? _sw;
        private int _total;
        private int _scanned;
        private int _active;
        private readonly Dictionary<string, string> _arpCache = new(StringComparer.OrdinalIgnoreCase);
        private bool _syncingSelection;
        private bool _eventsWired;

        public DiscoveryView()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            AppSettings.Changed -= OnAppSettingsChanged_Discovery;
            AppSettings.Changed += OnAppSettingsChanged_Discovery;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            DgvResults.ItemsSource = _rows;

            if (!_eventsWired)
            {
                BtnScan.Click += async (_, __) => await StartScanAsync();
                BtnCancel.Click += (_, __) => CancelScan();
                BtnSave.Click += async (_, __) => await SaveResultsAsync();
                CboAdapter.SelectionChanged += CboAdapter_SelectionChanged;
                _eventsWired = true;
            }

            LoadAdapters();
            OnAppSettingsChanged_Discovery(null, EventArgs.Empty);
        }

        private void CboAdapter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_syncingSelection) return;

            if (CboAdapter.SelectedItem is NetworkAdapterInfo a)
            {
                AppSettings.SetSelectedAdapter(a.Id, a.AdapterName);
                AutofillRange();
            }
        }

        private void OnAppSettingsChanged_Discovery(object? sender, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => OnAppSettingsChanged_Discovery(sender, e)));
                return;
            }

            if (_syncingSelection) return;

            // Refresh adapter list to reflect visibility flags.
            LoadAdapters();

            var target = AppSettings.SelectedAdapterName;
            if (string.IsNullOrWhiteSpace(target)) return;

            if (CboAdapter.SelectedItem is NetworkAdapterInfo cur && string.Equals(cur.AdapterName, target, StringComparison.OrdinalIgnoreCase))
                return;

            if (CboAdapter.ItemsSource is IEnumerable<NetworkAdapterInfo> items)
            {
                var match = items.FirstOrDefault(a => string.Equals(a.AdapterName, target, StringComparison.OrdinalIgnoreCase));
                if (match == null) return;

                try
                {
                    _syncingSelection = true;
                    CboAdapter.SelectedItem = match;
                }
                finally
                {
                    _syncingSelection = false;
                }
            }
        }

        private sealed class ProbeRow
        {
            public string Ip { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public long? LatencyMs { get; set; }
            public string Hostname { get; set; } = string.Empty;
            public string Mac { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public string Status { get; set; } = "No Reply";
        }
    }
}
