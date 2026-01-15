using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using NetworkUtilityApp.Services;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// Persistent output log view.
    /// Responsibilities:
    /// - Populate from the global in-memory log snapshot on load.
    /// - Subscribe to live log updates and append entries.
    /// - Provide Clear/Save actions via `AppLog`.
    /// - Marshal log updates to the UI thread.
    /// </summary>
    public partial class LogView : System.Windows.Controls.UserControl
    {
        private readonly ObservableCollection<string> _items = []; // bound to ListBox

        public LogView()
        {
            InitializeComponent();
            // Wire lifecycle and actions
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            BtnClear.Click += (_, __) => AppLog.Clear(); // clears in-memory log and UI
            BtnSave.Click += (_, __) => AppLog.SaveToFile(); // persists to disk and logs the save path
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            // Initial population from snapshot
            LogList.ItemsSource = _items;
            foreach (var entry in AppLog.Snapshot())
                _items.Add(entry.ToString());

            // Subscribe for streaming updates
            AppLog.EntryAdded += OnEntryAdded;
        }

        private void OnUnloaded(object? sender, RoutedEventArgs e)
        {
            // Unsubscribe to avoid memory leaks when view is detached
            AppLog.EntryAdded -= OnEntryAdded;
        }

        private void OnEntryAdded(object? sender, LogEntry e)
        {
            // Ensure updates occur on the UI thread
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OnEntryAdded(sender, e));
                return;
            }

            // Special case: empty message signals a clear
            if (string.IsNullOrWhiteSpace(e.Message))
            {
                _items.Clear();
                return;
            }

            // Append formatted entry and scroll into view
            _items.Add(e.ToString());
            LogList.ScrollIntoView(e.ToString());
        }
    }
}
