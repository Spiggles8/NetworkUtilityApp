using System.Windows.Controls;
using NetworkUtilityApp.Services;
using NetworkUtilityApp.Helpers;
using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Models;

namespace NetworkUtilityApp.Views
{
    /// <summary>
    /// AdaptersView partial: selection and actions (DHCP/Static).
    /// </summary>
    public partial class AdaptersView
    {
        private AdaptersRow? CurrentRow => DgvAdapters.SelectedItem as AdaptersRow;

        private void DgvAdapters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = CurrentRow;
            LblSelectedAdapter.Text = row == null ? "Selected Adapter: None" : $"Selected Adapter: {row.AdapterName}";
        }

        private async void OnSetDhcp()
        {
            var row = CurrentRow;
            if (row == null) { AppLog.Warn("Set DHCP: Select an adapter first."); return; }
            var result = NetworkController.SetDhcp(row.AdapterName);
            if (result.StartsWith("[ERROR]", StringComparison.OrdinalIgnoreCase)) AppLog.Error(result);
            else if (result.StartsWith("[SUCCESS]", StringComparison.OrdinalIgnoreCase)) AppLog.Success(result);
            else AppLog.Info(result);
            await LoadAdapters(true);
        }

        private async void OnSetStatic()
        {
            var row = CurrentRow;
            if (row == null) { AppLog.Warn("Set Static: Select an adapter first."); return; }

            var ip = OctetsToIp(TxtIP1.Text, TxtIP2.Text, TxtIP3.Text, TxtIP4.Text);
            var mask = OctetsToIp(TxtSubnet1.Text, TxtSubnet2.Text, TxtSubnet3.Text, TxtSubnet4.Text);
            var gw = OctetsToIp(TxtGateway1.Text, TxtGateway2.Text, TxtGateway3.Text, TxtGateway4.Text);

            if (string.IsNullOrWhiteSpace(ip) || !ValidationHelper.IsValidIPv4(ip))
            { AppLog.Warn("Set Static: Enter a valid IP address."); return; }
            if (string.IsNullOrWhiteSpace(mask) || !ValidationHelper.IsValidIPv4(mask))
            { AppLog.Warn("Set Static: Enter a valid subnet mask."); return; }
            if (!string.IsNullOrWhiteSpace(gw) && !ValidationHelper.IsValidIPv4(gw))
            { AppLog.Warn("Set Static: Enter a valid gateway or leave empty."); return; }

            var result = NetworkController.SetStatic(row.AdapterName, ip, mask, gw);
            if (result.StartsWith("[ERROR]", StringComparison.OrdinalIgnoreCase)) AppLog.Error(result);
            else if (result.StartsWith("[SUCCESS]", StringComparison.OrdinalIgnoreCase)) AppLog.Success(result);
            else AppLog.Info(result);
            await LoadAdapters(true);
        }
    }
}
