using System.Windows;
using NetworkUtilityApp.Helpers;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// Settings tab view (partial). Core wiring and shared model.
    /// </summary>
    public partial class SettingsView : System.Windows.Controls.UserControl
    {
        // Backing view-model for the settings UI
        private SettingsModel _settings = new();

        public SettingsView()
        {
            InitializeComponent();
            Loaded += OnLoaded; // delay wiring until visual tree exists
        }

        // On first load: hydrate model, bind values, wire events, and prep helpers
        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            LoadSettings();                 // read persisted settings into _settings
            BindToUi();                     // copy _settings -> controls
            WireEvents();                   // hook change handlers to persist edits
            WireOctetAutoAdvance();         // UX: auto-advance IP/subnet/gateway octets
            RefreshFavoriteFieldsFromStore(); // populate favorite IP fields from store
        }

        // Wire all setting-related events to save/persist on change
        private void WireEvents()
        {
            ChkDarkMode.Checked += (_, __) => SaveFromUi();
            ChkDarkMode.Unchecked += (_, __) => SaveFromUi();

            ChkShowVirtual.Checked += (_, __) => SaveFromUi();
            ChkShowVirtual.Unchecked += (_, __) => SaveFromUi();
            ChkShowLoopback.Checked += (_, __) => SaveFromUi();
            ChkShowLoopback.Unchecked += (_, __) => SaveFromUi();
            ChkShowBluetooth.Checked += (_, __) => SaveFromUi();
            ChkShowBluetooth.Unchecked += (_, __) => SaveFromUi();

            FavSlot.SelectionChanged += (_, __) => { SaveFromUi(); RefreshFavoriteFieldsFromStore(); };
            BtnFavSave.Click += (_, __) => { SaveFavorite(); RefreshFavoriteFieldsFromStore(); };

            DefSub1.TextChanged += (_, __) => SaveFromUi();
            DefSub2.TextChanged += (_, __) => SaveFromUi();
            DefSub3.TextChanged += (_, __) => SaveFromUi();
            DefSub4.TextChanged += (_, __) => SaveFromUi();

            SetDiscoveryParallel.TextChanged += (_, __) => SaveFromUi();
            SetDiscoveryTimeout.TextChanged += (_, __) => SaveFromUi();

            ChkEnableLlmnr.Checked += (_, __) => SaveFromUi();
            ChkEnableLlmnr.Unchecked += (_, __) => SaveFromUi();
            ChkEnableMdns.Checked += (_, __) => SaveFromUi();
            ChkEnableMdns.Unchecked += (_, __) => SaveFromUi();
            ChkEnableNbns.Checked += (_, __) => SaveFromUi();
            ChkEnableNbns.Unchecked += (_, __) => SaveFromUi();
            ChkEnableNbtstat.Checked += (_, __) => SaveFromUi();
            ChkEnableNbtstat.Unchecked += (_, __) => SaveFromUi();

            SetPingRetries.TextChanged += (_, __) => SaveFromUi();
            SetPingInterval.TextChanged += (_, __) => SaveFromUi();
        }

        // Simple model used to bind and persist settings from the UI
        private sealed class SettingsModel
        {
            // Appearance
            public bool DarkMode { get; set; }

            // Discovery/Adapters filtering
            public bool ShowVirtualAdapters { get; set; }
            public bool ShowLoopbackAdapters { get; set; }
            public bool ShowBluetoothAdapters { get; set; }

            // Favorite IP preset editing
            public int FavoriteSlot { get; set; } = 1;
            public string? FavoriteIp { get; set; }
            public string? FavoriteSubnet { get; set; }
            public string? FavoriteGateway { get; set; }

            // Defaults and discovery tuning
            public string DefaultSubnet { get; set; } = "255.255.255.0";
            public int? DiscoveryParallel { get; set; }
            public int? DiscoveryTimeout { get; set; }

            // Hostname resolution feature toggles
            public bool EnableLlmnr { get; set; } = true;
            public bool EnableMdns { get; set; } = true;
            public bool EnableNbns { get; set; } = true;
            public bool EnableNbtstat { get; set; } = true;

            // Ping behavior
            public int? PingRetryCount { get; set; }
            public int? PingIntervalSeconds { get; set; }
        }
    }
}
