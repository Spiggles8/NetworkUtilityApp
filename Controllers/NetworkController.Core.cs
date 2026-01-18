using System.Security.Principal;
using System.Text.RegularExpressions;

namespace NetworkUtilityApp.Controllers
{
  /// <summary>
  /// Lightweight controller exposing network operations (partial). Methods are split by concern.
  /// </summary>
  public partial class NetworkController
  {
    // Returns true when the current process is running with Administrator privileges.
    private static bool IsAdministrator()
    {
      using var id = WindowsIdentity.GetCurrent();
      var pr = new WindowsPrincipal(id);
      return pr.IsInRole(WindowsBuiltInRole.Administrator);
    }

    [GeneratedRegex(@"^\s*(\d+)\s+(\*|<*\d+\s*ms)\s+(\*|<*\d+\s*ms)\s+(\*|<*\d+\s*ms)\s+(.+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MyRegex();
  }

  /// <summary>
  /// Adapter info returned to the UI.
  /// </summary>
  public sealed class NetworkAdapterInfo
  {
    public string Id { get; set; } = string.Empty;
    public string AdapterName { get; set; } = string.Empty;
    public string IsDhcp { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Subnet { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string HardwareDetails { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
  }
}
