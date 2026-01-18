using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using NetworkUtilityApp.Properties;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// MainWindow layout restore/persist partial: restores and persists layout settings.
    /// </summary>
    public partial class MainWindow
    {
        private void OnLoadedRestoreLayout(object? sender, RoutedEventArgs e)
        {
            // Restore window size/position/state. If the saved values are off-screen, let WPF decide.
            try
            {
                var s = Settings.Default;

                if (!double.IsNaN(s.WindowWidth) && s.WindowWidth > 0) Width = s.WindowWidth;
                if (!double.IsNaN(s.WindowHeight) && s.WindowHeight > 0) Height = s.WindowHeight;

                if (!double.IsNaN(s.WindowLeft)) Left = s.WindowLeft;
                if (!double.IsNaN(s.WindowTop)) Top = s.WindowTop;

                if (s.WindowState >= 0 && s.WindowState <= 2)
                    WindowState = (WindowState)s.WindowState;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[Layout Restore Error] {ex.Message}");
            }

            // Last tab selection (defaults to "adapters")
            var tab = FindName("MainTabControl") as System.Windows.Controls.TabControl;
            var last = Settings.Default.LastTabKey;
            if (string.IsNullOrWhiteSpace(last)) last = "adapters";
            if (tab != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new System.Action(() =>
                {
                    var targetItem = tab.Items.Cast<object>()
                        .Select(i => i as System.Windows.Controls.TabItem)
                        .FirstOrDefault(ti => ti != null && (string.Equals(ti.Name, last, System.StringComparison.OrdinalIgnoreCase) || (ti.Tag as string) == last));
                    if (targetItem != null) tab.SelectedItem = targetItem;

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
                var s = Settings.Default;

                // Persist size/position only when in normal state.
                if (WindowState == WindowState.Normal)
                {
                    s.WindowLeft = Left;
                    s.WindowTop = Top;
                    s.WindowWidth = Width;
                    s.WindowHeight = Height;
                }

                s.WindowState = (int)WindowState;

                // Persist the currently selected tab key
                var tab = FindName("MainTabControl") as System.Windows.Controls.TabControl;
                if (tab?.SelectedItem is System.Windows.Controls.TabItem sel)
                {
                    var key = sel.Tag as string ?? sel.Name ?? string.Empty;
                    key = key.Trim().ToLowerInvariant();
                    if (!string.IsNullOrEmpty(key)) s.LastTabKey = key;
                }

                s.Save();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[Layout Save Error] {ex.Message}");
            }
        }
    }
}
