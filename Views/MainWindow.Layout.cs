using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using NetworkUtilityApp.Properties;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// MainWindow layout restore/persist partial: restores window size, log height, and last tab.
    /// </summary>
    public partial class MainWindow
    {
        private void OnLoadedRestoreLayout(object? sender, RoutedEventArgs e)
        {
            // Restore window size if previously saved
            if (Settings.Default.WindowWidth > 0 && Settings.Default.WindowHeight > 0)
            { Width = Settings.Default.WindowWidth; Height = Settings.Default.WindowHeight; }

            // Restore output log row height across all tabs
            var h = Settings.Default.OutputLogHeight; if (h > 50)
            { RowAdaptersLog.Height = new GridLength(h); RowDiagnosticsLog.Height = new GridLength(h); RowDiscoveryLog.Height = new GridLength(h); RowSettingsLog.Height = new GridLength(h); }

            // Restore last tab selection (defaults to "adapters")
            var tab = FindName("MainTabControl") as System.Windows.Controls.TabControl;
            var last = Settings.Default.LastTabKey;
            if (string.IsNullOrWhiteSpace(last)) last = "adapters";
            if (tab != null)
            {
                // Defer to Loaded priority so TabControl items are fully created
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new System.Action(() =>
                {
                    // Find tab item by Name or Tag and select it
                    var targetItem = tab.Items.Cast<object>()
                        .Select(i => i as System.Windows.Controls.TabItem)
                        .FirstOrDefault(ti => ti != null && (string.Equals(ti.Name, last, System.StringComparison.OrdinalIgnoreCase) || (ti.Tag as string) == last));
                    if (targetItem != null) tab.SelectedItem = targetItem;

                    // Persist changes to LastTabKey whenever the user switches tabs
                    tab.SelectionChanged += (_, __) =>
                    {
                        if (tab.SelectedItem is System.Windows.Controls.TabItem sel)
                        {
                            var key = sel.Tag as string ?? sel.Name ?? string.Empty;
                            key = key.Trim().ToLowerInvariant();
                            if (!string.IsNullOrEmpty(key))
                            {
                                Settings.Default.LastTabKey = key;
                                Settings.Default.Save();
                            }
                        }
                    };
                }));
            }
        }

        private void OnClosingPersistLayout(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Persist window size (use RestoreBounds when maximized)
                if (WindowState == WindowState.Normal)
                { Settings.Default.WindowWidth = Width; Settings.Default.WindowHeight = Height; }
                else
                { var rb = RestoreBounds; Settings.Default.WindowWidth = rb.Width; Settings.Default.WindowHeight = rb.Height; }

                // Persist current output log height (adapters tab row)
                var h = RowAdaptersLog.Height.Value; if (h > 50) Settings.Default.OutputLogHeight = h;

                // Persist the currently selected tab key
                var tab = FindName("MainTabControl") as System.Windows.Controls.TabControl;
                if (tab?.SelectedItem is System.Windows.Controls.TabItem sel)
                {
                    var key = sel.Tag as string ?? sel.Name ?? string.Empty;
                    key = key.Trim().ToLowerInvariant();
                    if (!string.IsNullOrEmpty(key)) Settings.Default.LastTabKey = key;
                }

                Settings.Default.Save();
            }
            catch (System.Exception ex)
            { Debug.WriteLine($"[Layout Save Error] {ex.Message}"); }
        }
    }
}
