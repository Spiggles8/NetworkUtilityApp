using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace NetworkUtilityApp
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Disable WPF data binding trace output
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Off;
            PresentationTraceSources.ResourceDictionarySource.Switch.Level = SourceLevels.Off;
            PresentationTraceSources.FreezableSource.Switch.Level = SourceLevels.Off;
            PresentationTraceSources.AnimationSource.Switch.Level = SourceLevels.Off;

            // Optionally, remove the default debug listener to silence Trace/Debug writes globally:
            // Debug.Listeners.Clear();
            // Trace.Listeners.Clear();
        }
    }
}