using System.Threading.Tasks;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// DiagnosticsView partial: Ping tool runner (single echo via controller).
    /// </summary>
    public partial class DiagnosticsView
    {
        private async Task RunPingOnceAsync()
        {
            var target = PingTarget.Text.Trim();
            if (string.IsNullOrWhiteSpace(target)) { Append("[ERROR] Enter a ping target."); return; }
            var result = Controllers.NetworkController.PingHost(target);
            Append(result);
            await Task.CompletedTask;
        }
    }
}
