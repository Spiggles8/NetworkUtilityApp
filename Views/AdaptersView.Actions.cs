using System.Windows.Controls;
using NetworkUtilityApp.Services;
using NetworkUtilityApp.Helpers;
using NetworkUtilityApp.Controllers;

namespace NetworkUtilityApp.Views
{
  /// <summary>
  /// Adapters tab actions: selection sync, DHCP/static apply, and reaction to shared app settings.
  /// Keeps selection synchronized across tabs via AppSettings and applies network changes.
  /// </summary>
  public partial class AdaptersView
  {
    // Current grid selection convenience accessor
    private AdapterRow? CurrentRow => DgvAdapters.SelectedItem as AdapterRow;
    private bool _syncingSelection;          // guards against re-entrancy when we programmatically change selection
    private bool _isReloadingForSelection;   // prevents reload loops when temporarily expanding filters

    // Grid selection changed -> reflect in label and broadcast to AppSettings
    private void DgvAdapters_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var row = CurrentRow;
      LblSelectedAdapter.Text = row == null ? "Selected Adapter: None" : $"Selected Adapter: {row.AdapterName}";

      if (_syncingSelection) return;
      AppSettings.SetSelectedAdapter(row?.AdapterId, row?.AdapterName);
    }

    // Respond to shared AppSettings changes (e.g., a selection made on Discovery)
    // Tries to match by AdapterId first (stable NIC GUID), then by name; expands filters if needed.
    private async void OnAppSettingsChanged_Adapters(object? sender, EventArgs e)
    {
      if (!Dispatcher.CheckAccess())
      {
        await Dispatcher.InvokeAsync(() => OnAppSettingsChanged_Adapters(sender, e));
        return;
      }

      if (_syncingSelection) return;

      var targetId = AppSettings.SelectedAdapterId;
      var targetName = AppSettings.SelectedAdapterName;
      if (string.IsNullOrWhiteSpace(targetId) && string.IsNullOrWhiteSpace(targetName)) return;

      if (_rows.Count == 0) return; // not loaded yet

      var current = CurrentRow;
      if (current != null)
      {
        if (!string.IsNullOrWhiteSpace(targetId) && string.Equals(current.AdapterId, targetId, StringComparison.OrdinalIgnoreCase)) return;
        if (string.IsNullOrWhiteSpace(targetId) && !string.IsNullOrWhiteSpace(targetName) && string.Equals(current.AdapterName, targetName, StringComparison.OrdinalIgnoreCase)) return;
      }

      AdapterRow? match = null;
      if (!string.IsNullOrWhiteSpace(targetId))
        match = _rows.FirstOrDefault(r => string.Equals(r.AdapterId, targetId, StringComparison.OrdinalIgnoreCase));

      if (match == null && !string.IsNullOrWhiteSpace(targetName))
        match = _rows.FirstOrDefault(r => string.Equals(r.AdapterName, targetName, StringComparison.OrdinalIgnoreCase));

      if (match == null && !string.IsNullOrWhiteSpace(targetName))
      {
        match = _rows.FirstOrDefault(r => r.AdapterName.Contains(targetName, StringComparison.OrdinalIgnoreCase))
            ?? _rows.FirstOrDefault(r => targetName.Contains(r.AdapterName, StringComparison.OrdinalIgnoreCase));
      }

      if (match == null)
      {
        // If filters currently hide the target, temporarily expand and reload
        if (!_isReloadingForSelection && (!AppSettings.ShowVirtualAdapters || !AppSettings.ShowLoopbackAdapters || !AppSettings.ShowBluetoothAdapters))
        {
          try
          {
            _isReloadingForSelection = true;
            AppSettings.SetVisibilityFlags(showVirtual: true, showLoopback: true, showBluetooth: true);
            await LoadAdapters(false);
          }
          finally
          {
            _isReloadingForSelection = false;
          }

          if (!string.IsNullOrWhiteSpace(targetId))
            match = _rows.FirstOrDefault(r => string.Equals(r.AdapterId, targetId, StringComparison.OrdinalIgnoreCase));
          if (match == null && !string.IsNullOrWhiteSpace(targetName))
            match = _rows.FirstOrDefault(r => string.Equals(r.AdapterName, targetName, StringComparison.OrdinalIgnoreCase));
        }

        if (match == null) return;
      }

      try
      {
        _syncingSelection = true;
        DgvAdapters.SelectedItem = match;
        DgvAdapters.ScrollIntoView(match);
      }
      finally
      {
        _syncingSelection = false;
      }
    }

    // Button: Set DHCP on the selected adapter; logs outcome and refreshes the table
    private async void OnSetDhcp()
    {
      var row = CurrentRow;
      if (row == null) { AppLog.Warn("Set DHCP: Select an adapter first."); return; }
      var result = NetworkController.SetDhcp(row.AdapterName);
      if (result.StartsWith("[ERROR]", StringComparison.OrdinalIgnoreCase)) AppLog.Error(result);
      else if (result.StartsWith("[SUCCESS]", StringComparison.OrdinalIgnoreCase)) AppLog.Success(result);
      else AppLog.Info(result);
      await LoadAdapters(true);
    }

    // Button: Set Static on the selected adapter; validates octets, logs outcome, and refreshes
    private async void OnSetStatic()
    {
      var row = CurrentRow;
      if (row == null) { AppLog.Warn("Set Static: Select an adapter first."); return; }

      var ip = OctetsToIp(TxtIP1.Text, TxtIP2.Text, TxtIP3.Text, TxtIP4.Text);
      var mask = OctetsToIp(TxtSubnet1.Text, TxtSubnet2.Text, TxtSubnet3.Text, TxtSubnet4.Text);
      var gw = OctetsToIp(TxtGateway1.Text, TxtGateway2.Text, TxtGateway3.Text, TxtGateway4.Text);

      if (string.IsNullOrWhiteSpace(ip) || !ValidationHelper.IsValidIPv4(ip))
      { AppLog.Warn("Set Static: Enter a valid IP address."); return; }
      if (string.IsNullOrWhiteSpace(mask) || !ValidationHelper.IsValidIPv4(mask))
      { AppLog.Warn("Set Static: Enter a valid subnet mask."); return; }
      if (!string.IsNullOrWhiteSpace(gw) && !ValidationHelper.IsValidIPv4(gw))
      { AppLog.Warn("Set Static: Enter a valid gateway or leave empty."); return; }

      var result = NetworkController.SetStatic(row.AdapterName, ip, mask, gw);
      if (result.StartsWith("[ERROR]", StringComparison.OrdinalIgnoreCase)) AppLog.Error(result);
      else if (result.StartsWith("[SUCCESS]", StringComparison.OrdinalIgnoreCase)) AppLog.Success(result);
      else AppLog.Info(result);
      await LoadAdapters(true);
    }
  }
}
