using System.Windows;
using NetworkUtilityApp.Services;
using NetworkUtilityApp.Helpers;
using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Models;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// AdaptersView partial: initial load and grid population with filters.
    /// </summary>
    public partial class AdaptersView
    {
        private async void OnLoaded(object? sender, RoutedEventArgs e)
        {
            DgvAdapters.ItemsSource = _rows;
            await LoadAdapters(false);
            _hasInitialLoadCompleted = true;
            RefreshFavoriteButtons();
        }

        private async Task LoadAdapters(bool log)
        {
            try
            {
                _rows.Clear();
                await Task.Delay(150);

                var adapters = NetworkController.GetAdapters() ?? [];

                bool showVirtual = AppSettings.ShowVirtualAdapters;
                bool showLoopback = AppSettings.ShowLoopbackAdapters;
                bool showBluetooth = AppSettings.ShowBluetoothAdapters;

                foreach (var a in adapters)
                {
                    var desc = (a.HardwareDetails ?? string.Empty).ToLowerInvariant();
                    var name = (a.AdapterName ?? string.Empty).ToLowerInvariant();

                    bool isLoopback = name.Contains("loopback") || desc.Contains("loopback");
                    if (!showLoopback && isLoopback) continue;

                    bool isVirtual = desc.Contains("virtual") || name.Contains("virtualbox") || name.Contains("hyper-v") || desc.Contains("vmware");
                    if (!showVirtual && isVirtual) continue;

                    bool isBluetooth = desc.Contains("bluetooth") || name.Contains("bluetooth");
                    if (!showBluetooth && isBluetooth) continue;

                    _rows.Add(new AdaptersRow
                    {
                        AdapterName = a.AdapterName ?? string.Empty,
                        Dhcp = a.IsDhcp ?? string.Empty,
                        IpAddress = a.IpAddress ?? string.Empty,
                        Subnet = a.Subnet ?? string.Empty,
                        Gateway = a.Gateway ?? string.Empty,
                        Status = a.Status ?? string.Empty,
                        HardwareDetails = a.HardwareDetails ?? string.Empty,
                        MacAddress = a.MacAddress ?? string.Empty
                    });
                }
                if (_rows.Count > 0)
                {
                    DgvAdapters.SelectedIndex = 0;
                }

                if (log)
                    AppLog.Info($"Adapters loaded ({_rows.Count} shown). Filters — Virtual: {showVirtual}, Loopback: {showLoopback}, Bluetooth: {showBluetooth}");
            }
            catch (Exception ex)
            {
                if (log) AppLog.Error("Failed to load adapters: " + ex.Message);
            }
        }
    }
}
