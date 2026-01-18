using System.Net.NetworkInformation;

namespace NetworkUtilityApp.Controllers
{
  /// <summary>
  /// NetworkController partial: adapter enumeration utilities.
  /// </summary>
  public partial class NetworkController
  {
    public static List<NetworkAdapterInfo> GetAdapters()
    {
      var adaptersList = new List<NetworkAdapterInfo>();
      try
      {
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
          var props = nic.GetIPProperties();
          var ipv4 = props.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
          var gateway = props.GatewayAddresses.FirstOrDefault(g => g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
          adaptersList.Add(new NetworkAdapterInfo
          {
            Id = nic.Id,
            AdapterName = nic.Name,
            IsDhcp = props.GetIPv4Properties()?.IsDhcpEnabled == true ? "DHCP" : "STATIC",
            IpAddress = ipv4?.Address.ToString() ?? string.Empty,
            Subnet = ipv4?.IPv4Mask?.ToString() ?? string.Empty,
            Gateway = gateway?.Address.ToString() ?? string.Empty,
            Status = nic.OperationalStatus.ToString(),
            HardwareDetails = nic.Description,
            MacAddress = NormalizeMac(nic.GetPhysicalAddress().ToString())
          });
        }
      }
      catch (Exception ex)
      {
        adaptersList.Add(new NetworkAdapterInfo { AdapterName = "Error", Status = $"Failed to enumerate adapters: {ex.Message}" });
      }
      return adaptersList;
    }

    private static string NormalizeMac(string raw)
    {
      if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
      var hex = new string([.. raw.Where(c => Uri.IsHexDigit(c))]);
      if (hex.Length < 12) return raw;
      hex = hex[..12].ToUpperInvariant();
      return string.Join(":", Enumerable.Range(0, 6).Select(i => hex.Substring(i * 2, 2)));
    }
  }
}
