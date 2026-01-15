using System.Threading.Tasks;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// DiagnosticsView partial: pathping external process runner.
    /// </summary>
    public partial class DiagnosticsView
    {
        private async Task RunPathpingAsync()
        {
            await RunToolAsync("pathping", PathpingTarget.Text.Trim(), 180000, tag: "PATHPING", argsPrefix: "-n ");
        }
    }
}
