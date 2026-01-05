using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Helpers;

namespace NetworkUtilityApp.Views
{
    public partial class AdaptersView : System.Windows.Controls.UserControl
    {
        private readonly ObservableCollection<AdapterRow> _rows = new();

        public AdaptersView()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            BtnRefresh.Click += (_, __) => LoadAdapters();
            DgvAdapters.SelectionChanged += DgvAdapters_SelectionChanged;
            BtnSetDhcp.Click += (_, __) => OnSetDhcp();
            BtnSetStatic.Click += (_, __) => OnSetStatic();

            BtnFavIPAddress1.Click += (_, __) => FillFavoriteSlot(1);
            BtnFavIPAddress2.Click += (_, __) => FillFavoriteSlot(2);
            BtnFavIPAddress3.Click += (_, __) => FillFavoriteSlot(3);
            BtnFavIPAddress4.Click += (_, __) => FillFavoriteSlot(4);
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            DgvAdapters.ItemsSource = _rows;
            LoadAdapters();
            RefreshFavoriteButtons();
        }

        private void RefreshFavoriteButtons()
        {
            void Set(System.Windows.Controls.Button btn, int slot)
            {
                var fav = FavoriteIpStore.Get(slot);
                var has = fav is not null && !string.IsNullOrWhiteSpace(fav.Ip);
                btn.Content = has ? fav!.Ip : "(empty)";
                btn.IsEnabled = has;
                var gw = string.IsNullOrWhiteSpace(fav?.Gateway) ? "(none)" : fav!.Gateway;
                btn.ToolTip = has ? $"IP: {fav!.Ip}\nSubnet: {fav!.Subnet}\nGateway: {gw}" : null;
            }
            Set(BtnFavIPAddress1, 1);
            Set(BtnFavIPAddress2, 2);
            Set(BtnFavIPAddress3, 3);
            Set(BtnFavIPAddress4, 4);
        }

        private void LoadAdapters()
        {
            try
            {
                _rows.Clear();
                var adapters = NetworkController.GetAdapters() ?? [];
                foreach (var a in adapters)
                {
                    _rows.Add(new AdapterRow
                    {
                        AdapterName = a.AdapterName,
                        Dhcp = a.IsDhcp,
                        IpAddress = a.IpAddress,
                        Subnet = a.Subnet,
                        Gateway = a.Gateway,
                        Status = a.Status,
                        HardwareDetails = a.HardwareDetails,
                        MacAddress = a.MacAddress
                    });
                }
                if (_rows.Count > 0)
                {
                    DgvAdapters.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to load adapters.\n\n" + ex.Message, "Adapters", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private AdapterRow? CurrentRow => DgvAdapters.SelectedItem as AdapterRow;

        private void DgvAdapters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = CurrentRow;
            LblSelectedAdapter.Text = row == null ? "Selected Adapter: None" : $"Selected Adapter: {row.AdapterName}";
        }

        private void OnSetDhcp()
        {
            var row = CurrentRow;
            if (row == null) { System.Windows.MessageBox.Show("Select an adapter first.", "Set DHCP", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            var result = NetworkController.SetDhcp(row.AdapterName);
            System.Windows.MessageBox.Show(result, "Set DHCP", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAdapters();
        }

        private void OnSetStatic()
        {
            var row = CurrentRow;
            if (row == null) { System.Windows.MessageBox.Show("Select an adapter first.", "Set Static", MessageBoxButton.OK, MessageBoxImage.Information); return; }

            var ip = OctetsToIp(TxtIP1.Text, TxtIP2.Text, TxtIP3.Text, TxtIP4.Text);
            var mask = OctetsToIp(TxtSubnet1.Text, TxtSubnet2.Text, TxtSubnet3.Text, TxtSubnet4.Text);
            var gw = OctetsToIp(TxtGateway1.Text, TxtGateway2.Text, TxtGateway3.Text, TxtGateway4.Text);

            if (string.IsNullOrWhiteSpace(ip) || !ValidationHelper.IsValidIPv4(ip))
            {
                System.Windows.MessageBox.Show("Enter a valid IP address.", "Set Static", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(mask) || !ValidationHelper.IsValidIPv4(mask))
            {
                System.Windows.MessageBox.Show("Enter a valid subnet mask.", "Set Static", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrWhiteSpace(gw) && !ValidationHelper.IsValidIPv4(gw))
            {
                System.Windows.MessageBox.Show("Enter a valid gateway or leave empty." , "Set Static", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = NetworkController.SetStatic(row.AdapterName, ip, mask, gw);
            System.Windows.MessageBox.Show(result, "Set Static", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAdapters();
        }

        private static string Octet(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            if (int.TryParse(s, out var v) && v >= 0 && v <= 255) return v.ToString();
            return string.Empty;
        }

        private static string OctetsToIp(string a, string b, string c, string d)
        {
            var o1 = Octet(a); var o2 = Octet(b); var o3 = Octet(c); var o4 = Octet(d);
            if (string.IsNullOrEmpty(o1) || string.IsNullOrEmpty(o2) || string.IsNullOrEmpty(o3) || string.IsNullOrEmpty(o4)) return string.Empty;
            return string.Join('.', o1, o2, o3, o4);
        }

        private void FillFavoriteSlot(int slot)
        {
            var fav = FavoriteIpStore.Get(slot);
            if (fav is null || string.IsNullOrWhiteSpace(fav.Ip)) return;
            FillIpSubnetGateway(fav.Ip, fav.Subnet, fav.Gateway);
        }

        private void FillIpSubnetGateway(string ip, string? subnet, string? gateway)
        {
            // IP
            var parts = ip.Split('.');
            if (parts.Length == 4)
            {
                TxtIP1.Text = parts[0];
                TxtIP2.Text = parts[1];
                TxtIP3.Text = parts[2];
                TxtIP4.Text = parts[3];
            }

            // Subnet (use provided or default)
            var mask = !string.IsNullOrWhiteSpace(subnet) && ValidationHelper.IsValidIPv4(subnet)
                ? subnet
                : "255.255.255.0";
            var m = mask.Split('.');
            if (m.Length == 4)
            {
                TxtSubnet1.Text = m[0];
                TxtSubnet2.Text = m[1];
                TxtSubnet3.Text = m[2];
                TxtSubnet4.Text = m[3];
            }

            // Gateway (only if provided; do not infer by default)
            if (!string.IsNullOrWhiteSpace(gateway) && ValidationHelper.IsValidIPv4(gateway))
            {
                var g = gateway.Split('.');
                if (g.Length == 4)
                {
                    TxtGateway1.Text = g[0];
                    TxtGateway2.Text = g[1];
                    TxtGateway3.Text = g[2];
                    TxtGateway4.Text = g[3];
                }
            }
            else
            {
                // Clear gateway octets when no favorite gateway exists
                TxtGateway1.Text = string.Empty;
                TxtGateway2.Text = string.Empty;
                TxtGateway3.Text = string.Empty;
                TxtGateway4.Text = string.Empty;
            }
        }

        private sealed class AdapterRow
        {
            public string AdapterName { get; set; } = string.Empty;
            public string Dhcp { get; set; } = string.Empty;
            public string IpAddress { get; set; } = string.Empty;
            public string Subnet { get; set; } = string.Empty;
            public string Gateway { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string HardwareDetails { get; set; } = string.Empty;
            public string MacAddress { get; set; } = string.Empty;
        }
    }
}
