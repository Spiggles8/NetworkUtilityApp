using System.Windows;
using NetworkUtilityApp.Services;
using NetworkUtilityApp.Helpers;
using NetworkUtilityApp.Controllers;

namespace NetworkUtilityApp.Views
{
  /// <summary>
  /// AdaptersView partial: initial load and grid population with filters.
  /// </summary>
  public partial class AdaptersView
  {
    private bool _loadedOnce;

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
      DgvAdapters.ItemsSource = _rows;

      // Tab switching can trigger Loaded multiple times depending on visual tree behavior.
      // Don't rebuild the list each time (it resets selection). Only load once, or when user hits Refresh.
      if (!_loadedOnce)
      {
        _loadedOnce = true;
        await LoadAdapters(false);
        RefreshFavoriteButtons();
      }

      // Apply shared selection (from Discovery) when the tab is shown.
      OnAppSettingsChanged_Adapters(null, EventArgs.Empty);
    }

    private async Task LoadAdapters(bool log)
    {
      try
      {
        // Preserve current selection so reloading doesn't jump to the top.
        var selectedId = CurrentRow?.AdapterId;
        var selectedName = CurrentRow?.AdapterName;

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

          _rows.Add(new AdapterRow
          {
            AdapterId = a.Id ?? string.Empty,
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

        // Ensure some shared selection exists.
        if (string.IsNullOrWhiteSpace(AppSettings.SelectedAdapterId) && string.IsNullOrWhiteSpace(AppSettings.SelectedAdapterName))
        {
          if (!string.IsNullOrWhiteSpace(selectedId) || !string.IsNullOrWhiteSpace(selectedName))
            AppSettings.SetSelectedAdapter(selectedId, selectedName);
          else if (_rows.Count > 0)
            AppSettings.SetSelectedAdapter(_rows[0].AdapterId, _rows[0].AdapterName);
        }

        // Re-apply selection if available.
        OnAppSettingsChanged_Adapters(null, EventArgs.Empty);

        // Only fall back to first row if absolutely nothing selected/available.
        if (_rows.Count > 0 && DgvAdapters.SelectedItem is null && string.IsNullOrWhiteSpace(AppSettings.SelectedAdapterId) && string.IsNullOrWhiteSpace(AppSettings.SelectedAdapterName))
          DgvAdapters.SelectedIndex = 0;

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
