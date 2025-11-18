using NetworkUtilityApp.Helpers;
using NetworkUtilityApp.Services;
using System;
using System.ComponentModel;    
using System.Windows.Forms;

namespace NetworkUtilityApp.Tabs
{
    public partial class TabSettings : UserControl
    {
        public TabSettings()
        {
            InitializeComponent();

            // Guard: avoid running runtime-only logic in the WinForms designer
            if (IsDesignMode()) return;

            // Ensure defaults exist (FavoriteIpStore.LoadAll seeds defaults if file is missing)
            _ = FavoriteIpStore.LoadAll();

            // Wire save buttons here (keep designer clean)
            btnSaveFav1.Click += (_, __) => SaveFavorite(1);
            btnSaveFav2.Click += (_, __) => SaveFavorite(2);
            btnSaveFav3.Click += (_, __) => SaveFavorite(3);
            btnSaveFav4.Click += (_, __) => SaveFavorite(4);

            // Prefill inputs with first favorite if available
            LoadFavoritesIntoFields(1);

            // Optional helper: when IP changes and gateway is empty, suggest .1
            txtFavIp.TextChanged += (_, __) => SuggestGateway();
        }

        private bool IsDesignMode()
        => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        // Save current inputs into a favorite slot and notify other tabs
        private void SaveFavorite(int slot)
        {
            var ip = txtFavIp.Text.Trim();
            var subnet = txtFavSubnet.Text.Trim();
            var gateway = txtFavGateway.Text.Trim();

            // Basic validation using existing helper
            if (!ValidationHelper.IsValidIPv4(ip))
            {
                AppendLog("IP Address is not a valid IPv4.");
                return;
            }
            if (!ValidationHelper.IsValidIPv4(subnet))
            {
                AppendLog("Subnet Mask is not a valid IPv4.");
                return;
            }
            if (!string.IsNullOrWhiteSpace(gateway) && !ValidationHelper.IsValidIPv4(gateway))
            {
                AppendLog("Gateway is not a valid IPv4.");
                return;
            }

            FavoriteIpStore.Save(slot, new FavoriteIpEntry
            {
                Ip = ip,
                Subnet = subnet,
                Gateway = string.IsNullOrWhiteSpace(gateway) ? "" : gateway
            });

            AppendLog($"Saved Favorite {slot}.");
        }

        // Append a message to the log textbox with timestamp (static per CA1822)
        private static void AppendLog(string message)
        {
            AppLog.Info(message);
        }

        // Load a favorite slot into the inputs
        private void LoadFavoritesIntoFields(int slot)
        {
            var fav = FavoriteIpStore.Get(slot);
            if (fav is null) return;

            txtFavIp.Text = fav.Ip;
            txtFavSubnet.Text = string.IsNullOrWhiteSpace(fav.Subnet) ? "255.255.255.0" : fav.Subnet;
            txtFavGateway.Text = fav.Gateway;
        }

        // If IP is valid and gateway is empty, suggest gateway as .1
        private void SuggestGateway()
        {
            var ip = txtFavIp.Text.Trim();
            if (!ValidationHelper.IsValidIPv4(ip)) return;
            if (!string.IsNullOrWhiteSpace(txtFavGateway.Text)) return;
            var parts = ip.Split('.');
            if (parts.Length != 4) return;
            txtFavGateway.Text = $"{parts[0]}.{parts[1]}.{parts[2]}.1";
        }       
    }
}