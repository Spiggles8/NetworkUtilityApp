using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using NetworkUtilityApp.Properties;
using NetworkUtilityApp.Services;

using WpfTabControl = System.Windows.Controls.TabControl;
using WpfTabItem = System.Windows.Controls.TabItem;

namespace NetworkUtilityApp.Views
{
  /// <summary>
  /// Main application window (partial). Wires lifecycle events.
  /// Layout restore/persist and tab selection logic are split into other partial files.
  /// </summary>
  public partial class MainWindow : Window
  {
    private static bool _startupLogged;

    public MainWindow()
    {
      InitializeComponent();

      // Hook lifecycle events implemented in partials
      Loaded += OnLoadedRestoreLayout;
      Closing += OnClosingPersistLayout;
      Loaded += OnLoadedLogStartup;
      Closed += OnClosedSaveLogAndMessage;
    }

    private void OnLoadedLogStartup(object? sender, RoutedEventArgs e)
    {
      if (_startupLogged) return;
      _startupLogged = true;
      AppLog.Info("Application started.");
    }

    private void OnClosedSaveLogAndMessage(object? sender, System.EventArgs e)
    {
      try
      {
        // Persist snapshot and emit save confirmation (which is also appended to file)
        AppLog.SaveToFile();
        // Append definitive closed message after the save line so ordering is consistent in file and memory
        AppLog.AppendToFileAndMemory(LogLevel.Info, "Application closed.");
      }
      catch { }
    }
  }
}
