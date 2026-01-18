using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using NetworkUtilityApp.Helpers;

namespace NetworkUtilityApp.Views
{
  /// <summary>
  /// SettingsView partial: load/save settings and bind to UI.
  /// </summary>
  public partial class SettingsView
  {
    // Resolve path under LocalAppData where settings.json is stored
    private static string GetSettingsPath()
    {
      var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkUtilityApp");
      Directory.CreateDirectory(dir);
      return Path.Combine(dir, "settings.json");
    }

    // Load settings from disk into the backing model; tolerant of missing/invalid file
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
      { System.Windows.MessageBox.Show("Failed to load settings: " + ex.Message, "Settings", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    // Save current backing model to disk as JSON
    private void SaveSettings()
    {
      try
      {
        var path = GetSettingsPath();
        var json = JsonSerializer.Serialize(_settings);
        File.WriteAllText(path, json);
      }
      catch (Exception ex)
      { System.Windows.MessageBox.Show("Failed to save settings: " + ex.Message, "Settings", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    // Copy model values into UI controls and propagate filter flags to AppSettings
    private void BindToUi()
    {
      ChkDarkMode.IsChecked = _settings.DarkMode;

      ChkShowVirtual.IsChecked = _settings.ShowVirtualAdapters;
      ChkShowLoopback.IsChecked = _settings.ShowLoopbackAdapters;
      ChkShowBluetooth.IsChecked = _settings.ShowBluetoothAdapters;

      // Propagate visibility flags to AppSettings so other tabs refresh
      AppSettings.SetVisibilityFlags(_settings.ShowVirtualAdapters, _settings.ShowLoopbackAdapters, _settings.ShowBluetoothAdapters);

      // Select favorite slot in ComboBox by Tag
      foreach (var item in FavSlot.Items)
      {
        if (item is System.Windows.Controls.ComboBoxItem cbi && int.TryParse(cbi.Tag?.ToString(), out var v) && v == _settings.FavoriteSlot)
        { FavSlot.SelectedItem = cbi; break; }
      }

      // Fill IP/Subnet/Gateway split fields
      FillOctetsOrClear(_settings.FavoriteIp, FavIp1, FavIp2, FavIp3, FavIp4);
      FillOctetsOrClear(_settings.FavoriteSubnet, FavSubnet1, FavSubnet2, FavSubnet3, FavSubnet4);
      FillOctetsOrClear(_settings.FavoriteGateway, FavGateway1, FavGateway2, FavGateway3, FavGateway4);

      // Default subnet (fallback to 255.255.255.0)
      var sub = string.IsNullOrWhiteSpace(_settings.DefaultSubnet) ? "255.255.255.0" : _settings.DefaultSubnet;
      var parts = sub.Split('.');
      if (parts.Length == 4)
      { DefSub1.Text = parts[0]; DefSub2.Text = parts[1]; DefSub3.Text = parts[2]; DefSub4.Text = parts[3]; }

      // Discovery tuning
      SetDiscoveryParallel.Text = _settings.DiscoveryParallel?.ToString() ?? string.Empty;
      SetDiscoveryTimeout.Text = _settings.DiscoveryTimeout?.ToString() ?? string.Empty;

      // Resolver feature toggles
      ChkEnableLlmnr.IsChecked = _settings.EnableLlmnr;
      ChkEnableMdns.IsChecked = _settings.EnableMdns;
      ChkEnableNbns.IsChecked = _settings.EnableNbns;
      ChkEnableNbtstat.IsChecked = _settings.EnableNbtstat;

      // Ping settings
      SetPingRetries.Text = _settings.PingRetryCount?.ToString() ?? string.Empty;
      SetPingInterval.Text = _settings.PingIntervalSeconds?.ToString() ?? string.Empty;
    }

    // Read values from UI, update backing model, propagate flags, and persist to disk
    private void SaveFromUi()
    {
      _settings.DarkMode = ChkDarkMode.IsChecked == true;
      _settings.ShowVirtualAdapters = ChkShowVirtual.IsChecked == true;
      _settings.ShowLoopbackAdapters = ChkShowLoopback.IsChecked == true;
      _settings.ShowBluetoothAdapters = ChkShowBluetooth.IsChecked == true;

      // Keep AppSettings in sync so other views reflect filter changes
      AppSettings.SetVisibilityFlags(_settings.ShowVirtualAdapters, _settings.ShowLoopbackAdapters, _settings.ShowBluetoothAdapters);

      // Selected favorite slot
      if (FavSlot.SelectedItem is System.Windows.Controls.ComboBoxItem cbi && int.TryParse(cbi.Tag?.ToString(), out var slot)) _settings.FavoriteSlot = slot;
      // Assemble dotted strings from split octets
      _settings.FavoriteIp = JoinOctets(FavIp1.Text, FavIp2.Text, FavIp3.Text, FavIp4.Text);
      _settings.FavoriteSubnet = JoinOctets(FavSubnet1.Text, FavSubnet2.Text, FavSubnet3.Text, FavSubnet4.Text);
      _settings.FavoriteGateway = JoinOctets(FavGateway1.Text, FavGateway2.Text, FavGateway3.Text, FavGateway4.Text);

      // Default subnet with fallback
      var sub = JoinOctets(DefSub1.Text, DefSub2.Text, DefSub3.Text, DefSub4.Text);
      _settings.DefaultSubnet = string.IsNullOrWhiteSpace(sub) ? "255.255.255.0" : sub;

      // Discovery tuning
      _settings.DiscoveryParallel = ParseIntOrNull(SetDiscoveryParallel.Text, 1, 512);
      _settings.DiscoveryTimeout = ParseIntOrNull(SetDiscoveryTimeout.Text, 50, 5000);

      // Resolver toggles
      _settings.EnableLlmnr = ChkEnableLlmnr.IsChecked == true;
      _settings.EnableMdns = ChkEnableMdns.IsChecked == true;
      _settings.EnableNbns = ChkEnableNbns.IsChecked == true;
      _settings.EnableNbtstat = ChkEnableNbtstat.IsChecked == true;

      // Ping settings
      _settings.PingRetryCount = ParseIntOrNull(SetPingRetries.Text, 0, 10);
      _settings.PingIntervalSeconds = ParseIntOrNull(SetPingInterval.Text, 1, 60);

      SaveSettings();
    }
  }
}
