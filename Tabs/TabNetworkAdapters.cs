using NetworkUtilityApp.Controllers; 
using NetworkUtilityApp.Helpers;     
using NetworkUtilityApp.Services;

namespace NetworkUtilityApp.Tabs
{
    /// <summary>
    /// Legacy/secondary WinForms view for network adapters.
    ///
    /// The WebView2-based page is now the primary adapter UI, so this tab
    /// mainly exposes favorite IP buttons and keeps a minimal grid in sync
    /// when used.
    /// </summary>
    public partial class TabNetworkAdapters : UserControl
    {
        // Controller is still kept for potential future use / parity
        private readonly NetworkController _controller = new();

        public TabNetworkAdapters()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                // Configure grid layout if a DataGridView is present
                ConfigureGridIfNeeded();

                // Favorites are managed by a shared store; update our buttons
                // whenever anything changes (from Settings, WebView, etc.).
                FavoriteIpStore.FavoritesChanged += (_, __) => RefreshFavoriteButtons();
                RefreshFavoriteButtons();
            }
        }

        // Kept for API symmetry with other tabs; currently no extra init needed
        public static void Initialize()
        {
        }

        /// <summary>
        /// Refresh adapter info for this tab. The actual adapter list is
        /// owned by the WebView page, so here we only reset any label state.
        /// </summary>
        public void RefreshAdapters()
        {
            try
            {
                // No-op refresh in WinForms tab; WebView2 manages adapter listing.
                var lblSelected = Controls.Find("lblSelectedAdapter", true).FirstOrDefault() as Label;
                if (lblSelected is not null)
                    lblSelected.Text = "Selected Adapter: None";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load adapters.\n\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Placeholder handler kept for designer wiring compatibility
        private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
        }

        /// <summary>
        /// Update the text/enabled state of the four favorite IP buttons
        /// based on the current contents of <see cref="FavoriteIpStore"/>.
        /// </summary>
        private void RefreshFavoriteButtons()
        {
            var pairs = new (string btnName, int slot)[]
            {
                ("BtnFavIPAddress1", 1),
                ("BtnFavIPAddress2", 2),
                ("BtnFavIPAddress3", 3),
                ("BtnFavIPAddress4", 4),
            };

            foreach (var (btnName, slot) in pairs)
            {
                if (Controls.Find(btnName, true).FirstOrDefault() is Button btn)
                {
                    var fav = FavoriteIpStore.Get(slot);
                    var has = fav is not null && !string.IsNullOrWhiteSpace(fav.Ip);
                    btn.Text = has ? fav!.Ip : "(empty)";
                    btn.Enabled = has;
                }
            }
        }

        /// <summary>
        /// Try to locate the DataGridView used to show adapters. Works with
        /// both named lookup and a generic depth-first search through controls.
        /// </summary>
        private DataGridView? GetGrid()
        {
            var byName = Controls.Find("dgvAdapters", true).FirstOrDefault() as DataGridView;
            if (byName is not null) return byName;

            var stack = new Stack<Control>();
            foreach (Control c in Controls) stack.Push(c);

            while (stack.Count > 0)
            {
                var c = stack.Pop();
                if (c is DataGridView dgv) return dgv;
                foreach (Control child in c.Controls)
                    stack.Push(child);
            }

            return null;
        }

        // Apply common grid settings and ensure expected adapter columns exist.
        private void ConfigureGridIfNeeded()
        {
            if (GetGrid() is not DataGridView dgv) return;

            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            if (dgv.Columns.Count == 0)
            {
                dgv.Columns.Add(MakeTextCol("colAdapter", "Adapter"));
                dgv.Columns.Add(MakeTextCol("colDhcp", "DHCP"));
                dgv.Columns.Add(MakeTextCol("colIp", "IP Address"));
                dgv.Columns.Add(MakeTextCol("colMask", "Subnet"));
                dgv.Columns.Add(MakeTextCol("colGw", "Gateway"));
                dgv.Columns.Add(MakeTextCol("colStatus", "Status"));
                dgv.Columns.Add(MakeTextCol("colHw", "Hardware Details", 220));
                dgv.Columns.Add(MakeTextCol("colMac", "MAC Address", 120));
            }
        }

        // Convenience factory for text columns used by the adapters grid.
        private static DataGridViewTextBoxColumn MakeTextCol(string name, string header, int width = 140)
            => new()
            {
                Name = name,
                HeaderText = header,
                MinimumWidth = Math.Min(80, width),
                Width = width,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

        private static void AppendLog(string message)
        {
            AppLog.Info(message);
        }

        /// <summary>
        /// Fill IP/subnet/gateway input fields from a single favorite IP
        /// value. Supports both single-field and octet-split layouts.
        /// </summary>
        private void FillIpSubnetGatewayFrom(string ipText)
        {
            if (string.IsNullOrWhiteSpace(ipText)) return;
            var ip = ipText.Trim();

            if (!ValidationHelper.IsValidIPv4(ip))
            {
                MessageBox.Show($"\"{ip}\" is not a valid IPv4 address.", "Invalid IP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var octets = ip.Split('.');
            if (octets.Length != 4)
            {
                MessageBox.Show($"Unexpected IP format: {ip}", "Invalid IP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string subnet = "255.255.255.0";
            var subnetOctets = subnet.Split('.');
            var gateway = $"{octets[0]}.{octets[1]}.{octets[2]}.1";
            var gwOctets = gateway.Split('.');

            // Depending on layout, either a single IP textbox or 4 octet boxes
            if (Controls.Find("txtIP", true).FirstOrDefault() is TextBox singleIp)
            {
                singleIp.Text = ip;
            }
            else
            {
                var names = new[] { "txtIP1", "txtIP2", "txtIP3", "txtIP4" };
                for (int i = 0; i < 4; i++)
                    if (Controls.Find(names[i], true).FirstOrDefault() is TextBox t)
                        t.Text = octets[i];
            }

            // Subnet single vs octet layout
            if (Controls.Find("txtSubnet", true).FirstOrDefault() is TextBox singleSubnet)
            {
                singleSubnet.Text = subnet;
            }
            else
            {
                var sNames = new[] { "txtSubnet1", "txtSubnet2", "txtSubnet3", "txtSubnet4" };
                for (int i = 0; i < 4; i++)
                    if (Controls.Find(sNames[i], true).FirstOrDefault() is TextBox t)
                        t.Text = subnetOctets[i];
            }

            // Gateway single vs octet layout
            if (Controls.Find("txtGateway", true).FirstOrDefault() is TextBox singleGw)
            {
                singleGw.Text = gateway;
            }
            else
            {
                var gNames = new[] { "txtGateway1", "txtGateway2", "txtGateway3", "txtGateway4" };
                for (int i = 0; i < 4; i++)
                    if (Controls.Find(gNames[i], true).FirstOrDefault() is TextBox t)
                        t.Text = gwOctets[i];
            }

            AppendLog($"Filled IP fields from favorite: {ip}");
        }

        private void BtnFavIPAddress1_Click(object? sender, EventArgs e)
        {
            if (sender is Button b) FillIpSubnetGatewayFrom(b.Text);
        }
        private void BtnFavIPAddress2_Click(object? sender, EventArgs e)
        {
            if (sender is Button b) FillIpSubnetGatewayFrom(b.Text);
        }
        private void BtnFavIPAddress3_Click(object? sender, EventArgs e)
        {
            if (sender is Button b) FillIpSubnetGatewayFrom(b.Text);
        }
        private void BtnFavIPAddress4_Click(object? sender, EventArgs e)
        {
            if (sender is Button b) FillIpSubnetGatewayFrom(b.Text);
        }
    }
}