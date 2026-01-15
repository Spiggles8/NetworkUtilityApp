namespace NetworkUtilityApp.Helpers
{
    /// <summary>
    /// Lightweight in-memory application settings shared across views.
    /// Only contains values that must affect other tabs at runtime (no persistence).
    /// </summary>
    public static class AppSettings
    {
        // Visibility flags for Adapters tab
        public static bool ShowVirtualAdapters { get; private set; } = true;  
        public static bool ShowLoopbackAdapters { get; private set; } = true; 
        public static bool ShowBluetoothAdapters { get; private set; } = true; 

        // Raised when any flag changes; views can subscribe to refresh their UI
        public static event EventHandler? Changed;

        /// <summary>
        /// Update the visibility flags and notify listeners if any value changed.
        /// </summary>
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
    }
}
