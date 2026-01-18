using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace NetworkUtilityApp.Views
{
  /// <summary>
  /// SettingsView partial: favorite IP/Subnet/Gateway split fields and octet behaviors.
  /// </summary>
  public partial class SettingsView
  {
    // Wire auto-advance and digit-only behavior for IP/Subnet/Gateway octets
    private void WireOctetAutoAdvance()
    {
      // Helper to wire a 4-octet row (a,b,c,d)
      void Wire(System.Windows.Controls.TextBox a, System.Windows.Controls.TextBox b, System.Windows.Controls.TextBox c, System.Windows.Controls.TextBox d)
      {
        // Auto-advance focus when an octet hits 3 digits
        void Handler(object? s, TextChangedEventArgs e)
        {
          if (s is System.Windows.Controls.TextBox tb && tb.Text.Length >= 3)
          {
            if (tb == a) b.Focus();
            else if (tb == b) c.Focus();
            else if (tb == c) d.Focus();
            else if (tb == d) BtnFavSave.Focus(); // final octet -> move focus to Save
          }
        }
        a.TextChanged += Handler; b.TextChanged += Handler; c.TextChanged += Handler; d.TextChanged += Handler;

        // Restrict input to digits only for cleaner entry (no dots or letters)
        void Preview(object? s, TextCompositionEventArgs e) { e.Handled = !e.Text.All(char.IsDigit); }
        a.PreviewTextInput += Preview; b.PreviewTextInput += Preview; c.PreviewTextInput += Preview; d.PreviewTextInput += Preview;
      }
      Wire(FavIp1, FavIp2, FavIp3, FavIp4);
      Wire(FavSubnet1, FavSubnet2, FavSubnet3, FavSubnet4);
      Wire(FavGateway1, FavGateway2, FavGateway3, FavGateway4);
    }

    // Fill four octet TextBoxes from a dotted value, or clear them if invalid/missing
    private static void FillOctetsOrClear(string? value, System.Windows.Controls.TextBox o1, System.Windows.Controls.TextBox o2, System.Windows.Controls.TextBox o3, System.Windows.Controls.TextBox o4)
    {
      var parts = !string.IsNullOrWhiteSpace(value) ? value!.Split('.') : Array.Empty<string>();
      if (parts.Length == 4) { o1.Text = parts[0]; o2.Text = parts[1]; o3.Text = parts[2]; o4.Text = parts[3]; }
      else { o1.Text = string.Empty; o2.Text = string.Empty; o3.Text = string.Empty; o4.Text = string.Empty; }
    }

    // Refresh octet fields from the favorite store when slot changes
    private void RefreshFavoriteFieldsFromStore()
    {
      try
      {
        if (FavSlot.SelectedItem is ComboBoxItem cbi && int.TryParse(cbi.Tag?.ToString(), out var slot))
        {
          var fav = Helpers.FavoriteIpStore.Get(slot);
          FillOctetsOrClear(fav?.Ip, FavIp1, FavIp2, FavIp3, FavIp4);
          FillOctetsOrClear(fav?.Subnet, FavSubnet1, FavSubnet2, FavSubnet3, FavSubnet4);
          FillOctetsOrClear(fav?.Gateway, FavGateway1, FavGateway2, FavGateway3, FavGateway4);
        }
      }
      catch { }
    }

    // Save the current octet values into the selected favorite slot
    private void SaveFavorite()
    {
      try
      {
        // Determine selected slot from ComboBox item tag
        if (FavSlot.SelectedItem is not ComboBoxItem cbi || !int.TryParse(cbi.Tag?.ToString(), out var slot)) { FavSaveStatus.Text = "Select a favorite slot."; return; }
        // Assemble dotted strings from octets
        var ip = JoinOctets(FavIp1.Text, FavIp2.Text, FavIp3.Text, FavIp4.Text);
        var subnet = JoinOctets(FavSubnet1.Text, FavSubnet2.Text, FavSubnet3.Text, FavSubnet4.Text);
        var gateway = JoinOctets(FavGateway1.Text, FavGateway2.Text, FavGateway3.Text, FavGateway4.Text);
        // Minimal validation: require IP and subnet (gateway optional)
        if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(subnet)) { FavSaveStatus.Text = "IP and Subnet required."; return; }
        // Persist to store and update status
        Helpers.FavoriteIpStore.Save(slot, new Helpers.FavoriteIpEntry { Ip = ip, Subnet = subnet, Gateway = gateway });
        FavSaveStatus.Text = $"Saved favorite #{slot}.";
      }
      catch (Exception ex) { FavSaveStatus.Text = "Save failed: " + ex.Message; }
    }
  }
}
