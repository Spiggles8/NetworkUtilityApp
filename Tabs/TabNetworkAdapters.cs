using NetworkUtilityApp.Controllers; // NetworkController
using NetworkUtilityApp.Helpers;     // ValidationHelper
using NetworkUtilityApp.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkUtilityApp.Tabs
{
    public partial class TabNetworkAdapters : UserControl
    {
        private readonly NetworkController _controller = new();
        private string? _selectedAdapter;

        public TabNetworkAdapters()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                ConfigureGridIfNeeded();
                WireEventsIfNeeded();
                FavoriteIpStore.FavoritesChanged += (_, __) => RefreshFavoriteButtons();
                RefreshFavoriteButtons();
            }
        }

        public void Initialize()
        {
            _ = RefreshAdaptersAsync();
        }

        private void WireEventsIfNeeded()
        {
            if (Controls.Find("btnSetDhcp", true).FirstOrDefault() is Button btnSetDhcp)
                btnSetDhcp.Click += async (_, __) => await OnSetDhcpAsync();

            if (Controls.Find("btnSetStatic", true).FirstOrDefault() is Button btnSetStatic)
                btnSetStatic.Click += async (_, __) => await OnSetStaticAsync();

            if (Controls.Find("btnRefresh", true).FirstOrDefault() is Button btnRefresh)
                btnRefresh.Click += async (_, __) => await RefreshAdaptersAsync();

            if (Controls.Find("BtnFavIPAddress1", true).FirstOrDefault() is Button fav1)
            {
                fav1.Click -= BtnFavIPAddress1_Click;
                fav1.Click += BtnFavIPAddress1_Click;
            }
            if (Controls.Find("BtnFavIPAddress2", true).FirstOrDefault() is Button fav2)
            {
                fav2.Click -= BtnFavIPAddress2_Click;
                fav2.Click += BtnFavIPAddress2_Click;
            }
            if (Controls.Find("BtnFavIPAddress3", true).FirstOrDefault() is Button fav3)
            {
                fav3.Click -= BtnFavIPAddress3_Click;
                fav3.Click += BtnFavIPAddress3_Click;
            }
            if (Controls.Find("BtnFavIPAddress4", true).FirstOrDefault() is Button fav4)
            {
                fav4.Click -= BtnFavIPAddress4_Click;
                fav4.Click += BtnFavIPAddress4_Click;
            }

            if (GetGrid() is DataGridView dgv)
                dgv.CellClick += Dgv_CellClick;
        }

        public async Task RefreshAdaptersAsync()
        {
            try
            {
                var dgv = GetGrid();
                var lblSelected = Controls.Find("lblSelectedAdapter", true).FirstOrDefault() as Label;

                if (dgv is null)
                {
                    Debug.WriteLine("TabNetworkAdapters.RefreshAdaptersAsync: dgvAdapters not found.");
                    return;
                }

                dgv.Rows.Clear();
                dgv.Rows.Add("Loading...", "", "", "", "", "", "", "");

                var adapters = await Task.Run(() => NetworkController.GetAdapters());

                AppendLog($"Adapter List Refreshed: Adapters returned = {adapters?.Count ?? 0}");

                dgv.SuspendLayout();
                dgv.Rows.Clear();

                if (adapters == null || adapters.Count == 0)
                {
                    dgv.Rows.Add("Adapter List Refreshed: No adapters found.", "", "", "", "", "", "", "");
                    dgv.ResumeLayout();
                    AutoSizeColumns(dgv);

                    if (lblSelected is not null)
                        lblSelected.Text = "Selected Adapter: None";
                    return;
                }

                foreach (var a in adapters)
                {
                    dgv.Rows.Add(
                        a.AdapterName,
                        a.IsDhcp,
                        a.IpAddress,
                        a.Subnet,
                        a.Gateway,
                        a.Status,
                        a.HardwareDetails,
                        a.MacAddress
                    );
                }

                dgv.ResumeLayout();
                AutoSizeColumns(dgv);

                if (!string.IsNullOrWhiteSpace(_selectedAdapter))
                {
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (!row.IsNewRow && (row.Cells[0].Value?.ToString() ?? "") == _selectedAdapter)
                        {
                            row.Selected = true;
                            dgv.CurrentCell = row.Cells[0];
                            break;
                        }
                    }
                }

                if (lblSelected is not null)
                    lblSelected.Text = "Selected Adapter: " + (_selectedAdapter ?? "None");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load adapters.\n\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (sender is not DataGridView dgv) return;

            var name = dgv.Rows[e.RowIndex].Cells[0].Value?.ToString();
            _selectedAdapter = string.IsNullOrWhiteSpace(name) ? null : name;

            if (Controls.Find("lblSelectedAdapter", true).FirstOrDefault() is Label lblSelected)
                lblSelected.Text = "Selected Adapter: " + (_selectedAdapter ?? "None");
        }

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

        private async Task OnSetDhcpAsync()
        {
            if (string.IsNullOrWhiteSpace(_selectedAdapter))
            {
                MessageBox.Show("Please select an adapter first.", "No Adapter",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string result = await Task.Run(() => NetworkController.SetDhcp(_selectedAdapter!));
                EmitControllerResult(result);
                await RefreshAdaptersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set DHCP.\n\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OnSetStaticAsync()
        {
            if (string.IsNullOrWhiteSpace(_selectedAdapter))
            {
                MessageBox.Show("Please select an adapter first.", "No Adapter",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var (okIp, ip) = ReadIpFromFields("txtIP", "txtIP1", "txtIP2", "txtIP3", "txtIP4");
            if (!okIp) { ShowInputError(ip); return; }

            var (okMask, mask) = ReadIpFromFields("txtSubnet", "txtSubnet1", "txtSubnet2", "txtSubnet3", "txtSubnet4");
            if (!okMask) { ShowInputError(mask); return; }

            var (okGw, gw) = ReadIpFromFields("txtGateway", "txtGateway1", "txtGateway2", "txtGateway3", "txtGateway4", allowEmpty: true);
            if (!okGw) { ShowInputError(gw); return; }

            var (valid, err) = ValidationHelper.ValidateStaticConfig(ip, mask, string.IsNullOrWhiteSpace(gw) ? null : gw, allowEmptyGateway: true);
            if (!valid) { ShowInputError(err); return; }

            try
            {
                string result = await Task.Run(() => NetworkController.SetStatic(_selectedAdapter!, ip, mask, gw));
                EmitControllerResult(result);
                await RefreshAdaptersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set static IP.\n\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

        private static DataGridViewTextBoxColumn MakeTextCol(string name, string header, int width = 140)
            => new()
            {
                Name = name,
                HeaderText = header,
                MinimumWidth = Math.Min(80, width),
                Width = width,
                ReadOnly = true
            };

        private static void AutoSizeColumns(DataGridView dgv)
        {
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgv.AutoResizeColumns();
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private static void ShowInputError(string message)
        {
            MessageBox.Show(message, "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // No instance data accessed; mark static.
        private static void EmitControllerResult(string result)
        {
            if (string.IsNullOrWhiteSpace(result)) return;
            Debug.WriteLine("Controller result: " + result);
            AppendLog(result);
        }

        // No instance data accessed; mark static.
        private static void AppendLog(string message)
        {
            AppLog.Info(message);
        }

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

        // Add back missing helper to fix ReadIpFromFields + deconstruction inference errors.
        private (bool ok, string valueOrError) ReadIpFromFields(
            string singleName, string o1, string o2, string o3, string o4, bool allowEmpty = false)
        {
            if (Controls.Find(singleName, true).FirstOrDefault() is TextBox single)
            {
                var s = single.Text?.Trim() ?? "";
                if (string.IsNullOrEmpty(s) && allowEmpty) return (true, "");
                if (!ValidationHelper.IsValidIPv4(s)) return (false, $"\"{singleName}\" is not a valid IPv4 address.");
                return (true, s);
            }

            var t1 = Controls.Find(o1, true).FirstOrDefault() as TextBox;
            var t2 = Controls.Find(o2, true).FirstOrDefault() as TextBox;
            var t3 = Controls.Find(o3, true).FirstOrDefault() as TextBox;
            var t4 = Controls.Find(o4, true).FirstOrDefault() as TextBox;

            var s1 = t1?.Text?.Trim() ?? "";
            var s2 = t2?.Text?.Trim() ?? "";
            var s3 = t3?.Text?.Trim() ?? "";
            var s4 = t4?.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(s1 + s2 + s3 + s4) && allowEmpty) return (true, "");

            if (!ValidationHelper.AreValidOctets(s1, s2, s3, s4))
                return (false, "IP fields must be numeric 0–255.");

            var ip = ValidationHelper.JoinOctets(s1, s2, s3, s4);
            if (!ValidationHelper.IsValidIPv4(ip))
                return (false, "Composed IP is not valid.");

            return (true, ip);
        }
    }
}