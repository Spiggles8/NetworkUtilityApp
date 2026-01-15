using System.Diagnostics;
using System.Linq;
using System.Windows; // for MessageBox enums
using NetworkUtilityApp.Models;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// DiscoveryView partial: save/export results to a text file.
    /// </summary>
    public partial class DiscoveryView
    {
        private async Task SaveResultsAsync()
        {
            try
            {
                if (_rows.Count == 0) { System.Windows.MessageBox.Show("Nothing to save.", "Discovery", MessageBoxButton.OK, MessageBoxImage.Information); return; }
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save Discovery Results",
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FileName = $"NetworkDiscovery_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"
                };
                if (dlg.ShowDialog() != true) return;
                var lines = _rows.Select(r => $"{r.Ip}\t{r.Hostname}\t{r.Mac}\t{r.Manufacturer}\t{r.LatencyMs}\t{r.Status}");
                await System.IO.File.WriteAllLinesAsync(dlg.FileName, lines);
            }
            catch (Exception ex)
            { System.Windows.MessageBox.Show("Failed to save: " + ex.Message, "Discovery", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}
