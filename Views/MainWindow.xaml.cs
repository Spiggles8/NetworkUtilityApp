using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NetworkUtilityApp.Properties; // add to access Settings

// Alias WPF controls explicitly to avoid ambiguity with WinForms types
using WpfTabControl = System.Windows.Controls.TabControl;
using WpfTabItem = System.Windows.Controls.TabItem;

namespace NetworkUtilityApp.Views
{
    public partial class MainWindow : Window
    {
        private const string TabKeyAdapters = "adapters";
        private const string TabKeyDiscovery = "discovery";
        private const string TabKeyDiagnostics = "diagnostics";
        private const string TabKeySettings = "settings";

        private bool _isRestoringTab = true; // suppress selection saves until restore completes
        private const bool DiagnosticsEnabled = true; // set false to disable popups

        public MainWindow()
        {
            InitializeComponent();

            // Wire events after initialization
            Loaded += OnLoadedRestoreTab;
            Closing += OnClosingPersistTab;

            // Attach SelectionChanged now; _isRestoringTab prevents early saves
            var tab = FindName("MainTabControl") as WpfTabControl;
            if (tab != null)
            {
                tab.SelectionChanged += OnTabSelectionChanged;
            }
        }

        private void OnLoadedRestoreTab(object? sender, RoutedEventArgs e)
        {
            var last = LoadLastTabKey();
            if (DiagnosticsEnabled)
                Debug.WriteLine($"[OnLoadedRestoreTab] Loaded LastTabKey='{last}'");

            if (string.IsNullOrWhiteSpace(last))
                last = TabKeyAdapters; // default to adapters

            var tab = FindName("MainTabControl") as WpfTabControl;
            if (tab == null) { _isRestoringTab = false; return; }

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                try
                {
                    // still in restoring mode
                    var targetItem = tab.Items.Cast<object>()
                        .Select(item => item as WpfTabItem)
                        .FirstOrDefault(ti =>
                        {
                            if (ti == null) return false;
                            var nameMatch = string.Equals(ti.Name, last, StringComparison.OrdinalIgnoreCase);
                            var tagMatch = ti.Tag is string s && string.Equals(s, last, StringComparison.OrdinalIgnoreCase);
                            return nameMatch || tagMatch;
                        });

                    if (targetItem != null)
                    {
                        tab.SelectedItem = targetItem;
                        if (DiagnosticsEnabled)
                            Debug.WriteLine($"[Restore] Selecting tab key='{last}' -> item Name='{targetItem.Name}', Tag='{targetItem.Tag}'");
                    }
                    else
                    {
                        var adaptersItem = tab.Items.Cast<object>()
                            .Select(item => item as WpfTabItem)
                            .FirstOrDefault(ti =>
                            {
                                if (ti == null) return false;
                                var nameMatch = string.Equals(ti.Name, TabKeyAdapters, StringComparison.OrdinalIgnoreCase);
                                var tagMatch = ti.Tag is string s && string.Equals(s, TabKeyAdapters, StringComparison.OrdinalIgnoreCase);
                                return nameMatch || tagMatch;
                            });
                        if (adaptersItem != null)
                        {
                            tab.SelectedItem = adaptersItem;
                            if (DiagnosticsEnabled)
                                Debug.WriteLine("[Restore Fallback] Could not find '{last}', defaulting to 'adapters'");
                        }
                    }
                }
                finally
                {
                    // restore complete; allow SelectionChanged to persist from now on
                    _isRestoringTab = false;
                }
            }));
        }

        private void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_isRestoringTab) return; // ignore early selection changes during init
            if (sender is not WpfTabControl tab) return;
            if (tab.SelectedItem is not WpfTabItem ti) return;

            var key = GetTabKey(ti);
            if (!string.IsNullOrEmpty(key))
            {
                SaveLastTabKey(key);
                if (DiagnosticsEnabled)
                    Debug.WriteLine($"[SelectionChanged] Saved LastTabKey='{key}' (Name='{ti.Name}', Tag='{ti.Tag}', Header='{ti.Header}')");
            }
            else if (DiagnosticsEnabled)
            {
                Debug.WriteLine("[SelectionChanged] Could not derive a key for selected TabItem.");
            }
        }

        private void OnClosingPersistTab(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            var tab = FindName("MainTabControl") as WpfTabControl;
            if (tab != null && tab.SelectedItem is WpfTabItem ti)
            {
                var key = GetTabKey(ti);
                if (!string.IsNullOrEmpty(key))
                {
                    SaveLastTabKey(key);
                    if (DiagnosticsEnabled)
                        Debug.WriteLine($"[Closing] Persisted LastTabKey='{key}'");
                }
                else if (DiagnosticsEnabled)
                {
                    Debug.WriteLine("[Closing] No key to persist for selected TabItem.");
                }
            }
        }

        private static string GetTabKey(WpfTabItem ti)
        {
            if (ti.Tag is string s && !string.IsNullOrWhiteSpace(s))
                return s.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(ti.Name))
                return ti.Name.Trim().ToLowerInvariant();
            if (ti.Header is string h && !string.IsNullOrWhiteSpace(h))
            {
                var norm = h.Trim().ToLowerInvariant();
                return norm switch
                {
                    "adapters" => TabKeyAdapters,
                    "discovery" => TabKeyDiscovery,
                    "diagnostics" => TabKeyDiagnostics,
                    "settings" => TabKeySettings,
                    _ => norm
                };
            }
            return string.Empty;
        }

        private static void SaveLastTabKey(string key)
        {
            try
            {
                Settings.Default.LastTabKey = key;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Settings Save Error] {ex.Message}");
            }
        }

        private static string LoadLastTabKey()
        {
            try
            {
                return Settings.Default.LastTabKey ?? string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Settings Load Error] {ex.Message}");
                return string.Empty;
            }
        }

        private void NavigateTo(string key)
        {
            var norm = (key ?? string.Empty).Trim().ToLowerInvariant();
            switch (norm)
            {
                case TabKeyAdapters: break;
                case TabKeyDiscovery: break;
                case TabKeyDiagnostics: break;
                case TabKeySettings: break;
                default: break;
            }
        }
    }
}
