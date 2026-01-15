using System.Threading.Tasks;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// DiagnosticsView partial: Traceroute tool runner via controller.
    /// </summary>
    public partial class DiagnosticsView
    {
        private async Task RunTracerouteAsync()
        {
            var target = TraceTarget.Text.Trim();
            if (string.IsNullOrWhiteSpace(target)) { Append("[ERROR] Enter a traceroute target."); return; }
            try
            {
                var res = Controllers.NetworkController.Traceroute(target, resolveNames: ChkTraceResolve.IsChecked == true);
                Append($"[TRACE] Target: {res.Target}");
                foreach (var h in res.Hops)
                {
                    var r1 = h.Rtt1Ms?.ToString() ?? "*";
                    var r2 = h.Rtt2Ms?.ToString() ?? "*";
                    var r3 = h.Rtt3Ms?.ToString() ?? "*";
                    Append($"{h.Hop,2}  {r1,4} ms  {r2,4} ms  {r3,4} ms  {h.HostnameOrAddress}");
                }
                if (res.Hops.Count == 0)
                {
                    Append("[TRACE] No hops parsed. Raw output:");
                    Append(res.RawOutput);
                }
            }
            catch (System.Exception ex)
            {
                Append("[ERROR] Traceroute failed: " + ex.Message);
            }
            await Task.CompletedTask;
        }
    }
}
