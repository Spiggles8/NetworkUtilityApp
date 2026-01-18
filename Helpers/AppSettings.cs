namespace NetworkUtilityApp.Helpers
{
  /// <summary>
  /// Lightweight in-memory application settings shared across views.
  /// Only contains values that must affect other tabs at runtime (no persistence beyond user settings already wired for adapter selection).
  /// </summary>
  public static class AppSettings
  {
    // Visibility flags for Adapters tab
    public static bool ShowVirtualAdapters { get; private set; } = true;
    public static bool ShowLoopbackAdapters { get; private set; } = true;
    public static bool ShowBluetoothAdapters { get; private set; } = true;

    // Cross-tab selection for network adapter
    public static string? SelectedAdapterId { get; private set; } = LoadSelectedAdapterId();
    public static string? SelectedAdapterName { get; private set; } = LoadSelectedAdapterName();

    // Raised when any flag/selection changes; views can subscribe to refresh their UI
    public static event EventHandler? Changed;

    // Update the visibility flags and notify listeners if any value changed.
    public static void SetVisibilityFlags(bool showVirtual, bool showLoopback, bool showBluetooth)
    {
      // No-op if values are unchanged
      if (ShowVirtualAdapters == showVirtual && ShowLoopbackAdapters == showLoopback && ShowBluetoothAdapters == showBluetooth)
        return;

      ShowVirtualAdapters = showVirtual;
      ShowLoopbackAdapters = showLoopback;
      ShowBluetoothAdapters = showBluetooth;
      Changed?.Invoke(null, EventArgs.Empty); // broadcast change to interested views
    }

    // Set the currently selected adapter (Id + Name). Notifies listeners about the change.
    public static void SetSelectedAdapter(string? adapterId, string? adapterName)
    {
      adapterId = string.IsNullOrWhiteSpace(adapterId) ? null : adapterId.Trim();
      adapterName = string.IsNullOrWhiteSpace(adapterName) ? null : adapterName.Trim();

      if (string.Equals(SelectedAdapterId, adapterId, StringComparison.OrdinalIgnoreCase) &&
          string.Equals(SelectedAdapterName, adapterName, StringComparison.OrdinalIgnoreCase))
        return;

      SelectedAdapterId = adapterId;
      SelectedAdapterName = adapterName;
      TryPersistSelectedAdapter(adapterId, adapterName);
      Changed?.Invoke(null, EventArgs.Empty); // notify change to interested views
    }

    // Back-compat overload (name-only)
    public static void SetSelectedAdapter(string? adapterName)
        => SetSelectedAdapter(adapterId: null, adapterName: adapterName);

    // Load persisted adapter Id from user settings
    private static string? LoadSelectedAdapterId()
    {
      try
      {
        var v = Properties.Settings.Default[nameof(SelectedAdapterId)] as string;
        return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
      }
      catch { return null; }
    }

    // Load persisted adapter name from user settings
    private static string? LoadSelectedAdapterName()
    {
      try
      {
        var v = Properties.Settings.Default[nameof(SelectedAdapterName)] as string;
        return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
      }
      catch { return null; }
    }

    // Persist adapter selection to user settings
    private static void TryPersistSelectedAdapter(string? adapterId, string? adapterName)
    {
      try
      {
        Properties.Settings.Default[nameof(SelectedAdapterId)] = adapterId ?? string.Empty;
        Properties.Settings.Default[nameof(SelectedAdapterName)] = adapterName ?? string.Empty;
        Properties.Settings.Default.Save();
      }
      catch { }
    }
  }
}
