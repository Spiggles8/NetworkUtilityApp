using System.Collections.ObjectModel;
using System.Windows;
using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Helpers;
using NetworkUtilityApp.Services;

namespace NetworkUtilityApp.Views
{
  /// <summary>
  /// Adapters tab view (partial). Wires UI events; logic split into partials.
  /// </summary>
  public partial class AdaptersView : System.Windows.Controls.UserControl
  {
    private readonly ObservableCollection<AdapterRow> _rows = [];

    public AdaptersView()
    {
      InitializeComponent();
      Loaded += OnLoaded;

      // Wire UI actions
      BtnRefresh.Click += async (_, __) => await LoadAdapters(true);
      DgvAdapters.SelectionChanged += DgvAdapters_SelectionChanged;
      BtnSetDhcp.Click += (_, __) => OnSetDhcp();
      BtnSetStatic.Click += (_, __) => OnSetStatic();

      // Favorite presets (quick-fill)
      BtnFavIPAddress1.Click += (_, __) => FillFavoriteSlot(1);
      BtnFavIPAddress2.Click += (_, __) => FillFavoriteSlot(2);
      BtnFavIPAddress3.Click += (_, __) => FillFavoriteSlot(3);
      BtnFavIPAddress4.Click += (_, __) => FillFavoriteSlot(4);

      // Keep selection synced even when the tab isn't currently visible.
      AppSettings.Changed -= OnAppSettingsChanged_Adapters;
      AppSettings.Changed += OnAppSettingsChanged_Adapters;
    }
  }
}
