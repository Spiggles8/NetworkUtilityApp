using NetworkUtilityApp.Helpers;

namespace NetworkUtilityApp.Views
{
  /// <summary>
  /// AdaptersView partial: favorite presets and octet helpers.
  /// </summary>
  public partial class AdaptersView
  {
    // Refresh the Favorite IP buttons to reflect current presets
    private void RefreshFavoriteButtons()
    {
      static void Set(System.Windows.Controls.Button btn, int slot)
      {
        var fav = FavoriteIpStore.Get(slot);
        var has = fav is not null && !string.IsNullOrWhiteSpace(fav.Ip);
        btn.Content = has ? fav!.Ip : "(empty)";               // show IP or placeholder
        btn.IsEnabled = has;                                     // disable if preset missing
        var gw = string.IsNullOrWhiteSpace(fav?.Gateway) ? "(none)" : fav!.Gateway;
        btn.ToolTip = has ? $"IP: {fav!.Ip}\nSubnet: {fav!.Subnet}\nGateway: {gw}" : null; // quick view
      }
      Set(BtnFavIPAddress1, 1);
      Set(BtnFavIPAddress2, 2);
      Set(BtnFavIPAddress3, 3);
      Set(BtnFavIPAddress4, 4);
    }

    // Fill IP/Subnet/Gateway inputs from a preset slot
    private void FillFavoriteSlot(int slot)
    {
      var fav = FavoriteIpStore.Get(slot);
      if (fav is null || string.IsNullOrWhiteSpace(fav.Ip)) return;
      FillIpSubnetGateway(fav.Ip, fav.Subnet, fav.Gateway);
    }

    // Populate octet text boxes with provided values, using defaults when missing
    private void FillIpSubnetGateway(string ip, string? subnet, string? gateway)
    {
      var parts = ip.Split('.');
      if (parts.Length == 4)
      { TxtIP1.Text = parts[0]; TxtIP2.Text = parts[1]; TxtIP3.Text = parts[2]; TxtIP4.Text = parts[3]; }

      // If subnet missing/invalid, default to 255.255.255.0
      var mask = !string.IsNullOrWhiteSpace(subnet) && ValidationHelper.IsValidIPv4(subnet) ? subnet : "255.255.255.0";
      var m = mask.Split('.');
      if (m.Length == 4)
      { TxtSubnet1.Text = m[0]; TxtSubnet2.Text = m[1]; TxtSubnet3.Text = m[2]; TxtSubnet4.Text = m[3]; }

      // Gateway optional: clear when blank/invalid
      if (!string.IsNullOrWhiteSpace(gateway) && ValidationHelper.IsValidIPv4(gateway))
      {
        var g = gateway.Split('.');
        if (g.Length == 4)
        { TxtGateway1.Text = g[0]; TxtGateway2.Text = g[1]; TxtGateway3.Text = g[2]; TxtGateway4.Text = g[3]; }
      }
      else
      {
        TxtGateway1.Text = string.Empty; TxtGateway2.Text = string.Empty; TxtGateway3.Text = string.Empty; TxtGateway4.Text = string.Empty;
      }
    }

    // Validate a single IP octet (0..255); return empty string if invalid
    private static string Octet(string? s)
    {
      if (string.IsNullOrWhiteSpace(s)) return string.Empty;
      if (int.TryParse(s, out var v) && v >= 0 && v <= 255) return v.ToString();
      return string.Empty;
    }

    // Combine four octets into an IP string; returns empty string if any octet invalid
    private static string OctetsToIp(string a, string b, string c, string d)
    {
      var o1 = Octet(a); var o2 = Octet(b); var o3 = Octet(c); var o4 = Octet(d);
      if (string.IsNullOrEmpty(o1) || string.IsNullOrEmpty(o2) || string.IsNullOrEmpty(o3) || string.IsNullOrEmpty(o4)) return string.Empty;
      return string.Join('.', o1, o2, o3, o4);
    }

    // Row backing model for the adapters grid
    private sealed class AdapterRow
    {
      public string AdapterId { get; set; } = string.Empty;
      public string AdapterName { get; set; } = string.Empty;
      public string Dhcp { get; set; } = string.Empty;
      public string IpAddress { get; set; } = string.Empty;
      public string Subnet { get; set; } = string.Empty;
      public string Gateway { get; set; } = string.Empty;
      public string Status { get; set; } = string.Empty;
      public string HardwareDetails { get; set; } = string.Empty;
      public string MacAddress { get; set; } = string.Empty;
    }
  }
}
