using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace NetworkUtilityApp.Views
{
    public partial class SettingsView : System.Windows.Controls.UserControl
    {
        private const string SettingsFile = "settings.json";
        private SettingsModel _settings = new();

        public SettingsView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            LoadSettings();
            BindToUi();
            WireEvents();
        }

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

            FavSlot.SelectionChanged += (_, __) => SaveFromUi();
            FavIp.TextChanged += (_, __) => SaveFromUi();
            FavSubnet.TextChanged += (_, __) => SaveFromUi();
            FavGateway.TextChanged += (_, __) => SaveFromUi();
            BtnFavSave.Click += (_, __) => SaveFavorite();

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

        private static string GetSettingsPath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkUtilityApp");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, SettingsFile);
        }

        private void LoadSettings()
        {
            try
            {
                var path = GetSettingsPath();
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var s = JsonSerializer.Deserialize<SettingsModel>(json);
                    if (s != null) _settings = s;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to load settings: " + ex.Message, "Settings", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                var path = GetSettingsPath();
                var json = JsonSerializer.Serialize(_settings);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to save settings: " + ex.Message, "Settings", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BindToUi()
        {
            ChkDarkMode.IsChecked = _settings.DarkMode;

            ChkShowVirtual.IsChecked = _settings.ShowVirtualAdapters;
            ChkShowLoopback.IsChecked = _settings.ShowLoopbackAdapters;
            ChkShowBluetooth.IsChecked = _settings.ShowBluetoothAdapters;

            // Favorite slot
            foreach (var item in FavSlot.Items)
            {
                if (item is System.Windows.Controls.ComboBoxItem cbi && int.TryParse(cbi.Tag?.ToString(), out var v) && v == _settings.FavoriteSlot)
                {
                    FavSlot.SelectedItem = cbi;
                    break;
                }
            }
            FavIp.Text = _settings.FavoriteIp ?? string.Empty;
            FavSubnet.Text = _settings.FavoriteSubnet ?? string.Empty;
            FavGateway.Text = _settings.FavoriteGateway ?? string.Empty;

            // Default subnet octets
            var sub = string.IsNullOrWhiteSpace(_settings.DefaultSubnet) ? "255.255.255.0" : _settings.DefaultSubnet;
            var parts = sub.Split('.');
            if (parts.Length == 4)
            {
                DefSub1.Text = parts[0];
                DefSub2.Text = parts[1];
                DefSub3.Text = parts[2];
                DefSub4.Text = parts[3];
            }

            SetDiscoveryParallel.Text = _settings.DiscoveryParallel?.ToString() ?? string.Empty;
            SetDiscoveryTimeout.Text = _settings.DiscoveryTimeout?.ToString() ?? string.Empty;

            ChkEnableLlmnr.IsChecked = _settings.EnableLlmnr;
            ChkEnableMdns.IsChecked = _settings.EnableMdns;
            ChkEnableNbns.IsChecked = _settings.EnableNbns;
            ChkEnableNbtstat.IsChecked = _settings.EnableNbtstat;

            SetPingRetries.Text = _settings.PingRetryCount?.ToString() ?? string.Empty;
            SetPingInterval.Text = _settings.PingIntervalSeconds?.ToString() ?? string.Empty;
        }

        private void SaveFromUi()
        {
            _settings.DarkMode = ChkDarkMode.IsChecked == true;
            _settings.ShowVirtualAdapters = ChkShowVirtual.IsChecked == true;
            _settings.ShowLoopbackAdapters = ChkShowLoopback.IsChecked == true;
            _settings.ShowBluetoothAdapters = ChkShowBluetooth.IsChecked == true;

            if (FavSlot.SelectedItem is System.Windows.Controls.ComboBoxItem cbi && int.TryParse(cbi.Tag?.ToString(), out var slot))
                _settings.FavoriteSlot = slot;
            _settings.FavoriteIp = FavIp.Text.Trim();
            _settings.FavoriteSubnet = FavSubnet.Text.Trim();
            _settings.FavoriteGateway = FavGateway.Text.Trim();

            var sub = JoinOctets(DefSub1.Text, DefSub2.Text, DefSub3.Text, DefSub4.Text);
            _settings.DefaultSubnet = string.IsNullOrWhiteSpace(sub) ? "255.255.255.0" : sub;

            _settings.DiscoveryParallel = ParseIntOrNull(SetDiscoveryParallel.Text, 1, 512);
            _settings.DiscoveryTimeout = ParseIntOrNull(SetDiscoveryTimeout.Text, 50, 5000);

            _settings.EnableLlmnr = ChkEnableLlmnr.IsChecked == true;
            _settings.EnableMdns = ChkEnableMdns.IsChecked == true;
            _settings.EnableNbns = ChkEnableNbns.IsChecked == true;
            _settings.EnableNbtstat = ChkEnableNbtstat.IsChecked == true;

            _settings.PingRetryCount = ParseIntOrNull(SetPingRetries.Text, 0, 10);
            _settings.PingIntervalSeconds = ParseIntOrNull(SetPingInterval.Text, 1, 60);

            SaveSettings();
        }

        private void SaveFavorite()
        {
            try
            {
                if (FavSlot.SelectedItem is not System.Windows.Controls.ComboBoxItem cbi || !int.TryParse(cbi.Tag?.ToString(), out var slot))
                {
                    FavSaveStatus.Text = "Select a favorite slot.";
                    return;
                }
                var ip = FavIp.Text.Trim();
                var subnet = FavSubnet.Text.Trim();
                var gateway = FavGateway.Text.Trim();
                if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(subnet))
                {
                    FavSaveStatus.Text = "IP and Subnet required.";
                    return;
                }
                Helpers.FavoriteIpStore.Save(slot, new Helpers.FavoriteIpEntry { Ip = ip, Subnet = subnet, Gateway = gateway });
                FavSaveStatus.Text = $"Saved favorite #{slot}.";
            }
            catch (Exception ex)
            {
                FavSaveStatus.Text = "Save failed: " + ex.Message;
            }
        }

        private static int? ParseIntOrNull(string text, int min, int max)
        {
            if (int.TryParse(text, out var v) && v >= min && v <= max) return v;
            return null;
        }

        private static string JoinOctets(string a, string b, string c, string d)
        {
            string oa = Octet(a), ob = Octet(b), oc = Octet(c), od = Octet(d);
            if (oa.Length == 0 || ob.Length == 0 || oc.Length == 0 || od.Length == 0) return string.Empty;
            return string.Join('.', oa, ob, oc, od);
        }

        private static string Octet(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            if (int.TryParse(s, out var v) && v >= 0 && v <= 255) return v.ToString();
            return string.Empty;
        }

        private sealed class SettingsModel
        {
            public bool DarkMode { get; set; }
            public bool ShowVirtualAdapters { get; set; }
            public bool ShowLoopbackAdapters { get; set; }
            public bool ShowBluetoothAdapters { get; set; }

            public int FavoriteSlot { get; set; } = 1;
            public string? FavoriteIp { get; set; }
            public string? FavoriteSubnet { get; set; }
            public string? FavoriteGateway { get; set; }

            public string DefaultSubnet { get; set; } = "255.255.255.0";
            public int? DiscoveryParallel { get; set; }
            public int? DiscoveryTimeout { get; set; }

            public bool EnableLlmnr { get; set; } = true;
            public bool EnableMdns { get; set; } = true;
            public bool EnableNbns { get; set; } = true;
            public bool EnableNbtstat { get; set; } = true;

            public int? PingRetryCount { get; set; }
            public int? PingIntervalSeconds { get; set; }
        }
    }
}
