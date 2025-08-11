using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Helpers;              // ValidationHelper

namespace NetworkUtilityApp.Tabs
{
    public partial class TabNetworkAdapters : UserControl
    {
        private readonly NetworkController _controller = new();
        private string? _selectedAdapter;

        public Action<string, LogLevel>? Log { get; set; }

        public TabNetworkAdapters()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                ConfigureGridIfNeeded();
                WireEventsIfNeeded();
            }
        }

        public void Initialize()
        {
            _ = RefreshAdaptersAsync();
        }

        // ----------------------------
        // UI Event wiring (pattern matching applied)
        // ----------------------------
        private void WireEventsIfNeeded()
        {
            // line 45,46 – pattern matching
            if (Controls.Find("btnSetDhcp", true).FirstOrDefault() is Button btnSetDhcp)
                btnSetDhcp.Click += async (_, __) => await OnSetDhcpAsync();

            if (Controls.Find("btnSetStatic", true).FirstOrDefault() is Button btnSetStatic)
                btnSetStatic.Click += async (_, __) => await OnSetStaticAsync();

            if (Controls.Find("btnRefresh", true).FirstOrDefault() is Button btnRefresh)
                btnRefresh.Click += async (_, __) => await RefreshAdaptersAsync();

            if (GetGrid() is DataGridView dgv)
                dgv.CellClick += Dgv_CellClick;
        }

        // ----------------------------
        // Adapters load / refresh
        // ----------------------------
        public async Task RefreshAdaptersAsync()
        {
            try
            {
                var dgv = GetGrid();
                var lblSelected = Controls.Find("lblSelectedAdapter", true).FirstOrDefault() as Label;

                if (dgv is not null)
                {
                    dgv.Rows.Clear();
                    dgv.Rows.Add("Loading...", "", "", "", "", "", "", "");
                }

                var adapters = await Task.Run(() => NetworkController.GetAdapters());

                if (dgv is not null)
                {
                    dgv.SuspendLayout();
                    dgv.Rows.Clear();

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
                }

                if (!string.IsNullOrWhiteSpace(_selectedAdapter) && dgv is not null)
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

                Log?.Invoke($"[REFRESH] Found {adapters.Count} adapters.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log?.Invoke("[ERROR] Refresh adapters failed: " + ex.Message, LogLevel.Error);
                MessageBox.Show("Failed to load adapters.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            // line 129 – pattern matching
            if (sender is not DataGridView dgv) return;

            var name = dgv.Rows[e.RowIndex].Cells[0].Value?.ToString();
            _selectedAdapter = string.IsNullOrWhiteSpace(name) ? null : name;

            // line 135 – pattern matching
            if (Controls.Find("lblSelectedAdapter", true).FirstOrDefault() is Label lblSelected)
            {
                lblSelected.Text = "Selected Adapter: " + (_selectedAdapter ?? "None");
            }
        }

        // ----------------------------
        // DHCP / Static handlers
        // ----------------------------
        private async Task OnSetDhcpAsync()
        {
            if (string.IsNullOrWhiteSpace(_selectedAdapter))
            {
                MessageBox.Show("Please select an adapter first.", "No Adapter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Log?.Invoke($"[ACTION] Set DHCP on \"{_selectedAdapter}\" …", LogLevel.Info);

                string result = await Task.Run(() => _controller.SetDhcp(_selectedAdapter!));
                EmitControllerResult(result);

                await RefreshAdaptersAsync();
            }
            catch (Exception ex)
            {
                Log?.Invoke("[ERROR] Set DHCP failed: " + ex.Message, LogLevel.Error);
                MessageBox.Show("Failed to set DHCP.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OnSetStaticAsync()
        {
            if (string.IsNullOrWhiteSpace(_selectedAdapter))
            {
                MessageBox.Show("Please select an adapter first.", "No Adapter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var (okIp, ipOrErr) = ReadIpFromFields("txtIP", "txtIP1", "txtIP2", "txtIP3", "txtIP4");
            if (!okIp) { ShowInputError(ipOrErr); return; }

            var (okMask, maskOrErr) = ReadIpFromFields("txtSubnet", "txtSubnet1", "txtSubnet2", "txtSubnet3", "txtSubnet4");
            if (!okMask) { ShowInputError(maskOrErr); return; }

            var (okGw, gwOrErr) = ReadIpFromFields("txtGateway", "txtGateway1", "txtGateway2", "txtGateway3", "txtGateway4", allowEmpty: true);
            if (!okGw) { ShowInputError(gwOrErr); return; }

            var (valid, err) = ValidationHelper.ValidateStaticConfig(ipOrErr, maskOrErr, string.IsNullOrWhiteSpace(gwOrErr) ? null : gwOrErr, allowEmptyGateway: true);
            if (!valid)
            {
                ShowInputError(err);
                return;
            }

            try
            {
                Log?.Invoke($"[ACTION] Set Static on \"{_selectedAdapter}\" → IP {ipOrErr} / {maskOrErr} gw {gwOrErr}", LogLevel.Info);

                string result = await Task.Run(() => _controller.SetStatic(_selectedAdapter!, ipOrErr, maskOrErr, gwOrErr));
                EmitControllerResult(result);

                await RefreshAdaptersAsync();
            }
            catch (Exception ex)
            {
                Log?.Invoke("[ERROR] Set Static failed: " + ex.Message, LogLevel.Error);
                MessageBox.Show("Failed to set static IP.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ShowInputError(string message)
        {
            MessageBox.Show(message, "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void EmitControllerResult(string result)
        {
            var level = result.StartsWith("[SUCCESS]", StringComparison.OrdinalIgnoreCase) ? LogLevel.Success
                      : result.StartsWith("[ERROR]", StringComparison.OrdinalIgnoreCase) ? LogLevel.Error
                      : result.StartsWith("[PING FAIL]", StringComparison.OrdinalIgnoreCase) ? LogLevel.Warning
                      : LogLevel.Info;

            Log?.Invoke(result, level);
        }

        // ----------------------------
        // Helpers
        // ----------------------------
        private DataGridView? GetGrid()
            => Controls.Find("dgvAdapters", true).FirstOrDefault() as DataGridView;

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

        private (bool ok, string valueOrError) ReadIpFromFields(string singleName, string o1, string o2, string o3, string o4, bool allowEmpty = false)
        {
            // line 283 – pattern matching
            if (Controls.Find(singleName, true).FirstOrDefault() is TextBox single)
            {
                var s = single.Text?.Trim() ?? "";
                if (string.IsNullOrEmpty(s) && allowEmpty) return (true, "");
                if (!ValidationHelper.IsValidIPv4(s)) return (false, $"\"{singleName}\" is not a valid IPv4 address.");
                return (true, s);
            }

            // Fallback to octets
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
