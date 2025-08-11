using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetworkUtilityApp.Controllers;
using NetworkUtilityApp.Helpers;

namespace NetworkUtilityApp.Tabs
{
    public partial class TabNetworkDiscovery : UserControl
    {
        private readonly DiscoveryController _discovery = new();
        private readonly List<DiscoveryController.DiscoveryResult> _results = [];

        /// <summary>
        /// Optional logger callback provided by host form.
        /// </summary>
        public Action<string, LogLevel>? Log { get; set; }

        public TabNetworkDiscovery()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                EnsureGrid();
                WireEvents();
            }
        }

        public void Initialize()
        {
            // Populate NIC dropdown on first show
            PopulateNicDropdown();
            UpdateButtons(scanning: false);
            SetDefaultNumbersIfEmpty();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { _discovery.Stop(); } catch { }
                _discovery.Dispose();
            }
            base.Dispose(disposing);
        }

        // ===========================
        // UI Wiring
        // ===========================
        private void WireEvents()
        {
            Find<Button>("btnStartDiscovery")?.Apply(b => b.Click += async (_, __) => await OnStartAsync());
            Find<Button>("btnStopDiscovery")?.Apply(b => b.Click += (_, __) => OnStop());
            Find<Button>("btnExportDiscoveryCsv")?.Apply(b => b.Click += (_, __) => OnExportCsv());

            Find<ComboBox>("cboNic")?.Apply(c =>
            {
                c.DropDown += (_, __) => PopulateNicDropdown(); // refresh when opened
                c.SelectedIndexChanged += (_, __) => ApplySelectedNicToCidr();
            });

            Find<CheckBox>("chkScanAllNics")?.Apply(chk =>
            {
                chk.CheckedChanged += (_, __) =>
                {
                    var enableNic = !chk.Checked;
                    var cbo = Find<ComboBox>("cboNic");
                    if (cbo != null) cbo.Enabled = enableNic;
                    if (enableNic) ApplySelectedNicToCidr();
                };
            });
        }

        private void SetDefaultNumbersIfEmpty()
        {
            SetIfMissing("numTimeoutMs", 1000);
            SetIfMissing("numMaxParallel", 256);
        }

        // ===========================
        // Start / Stop
        // ===========================
        private async Task OnStartAsync()
        {
            // guard
            if (Find<DataGridView>("dgvDiscovery") is { } grid)
            {
                grid.Rows.Clear();
            }
            _results.Clear();
            UpdateButtons(scanning: true);
            SetProgress(0, 0, 0);

            // Build options
            var options = new DiscoveryController.DiscoveryOptions
            {
                ResolveDns = GetBool("chkResolveDns", false),
                TimeoutMs = GetInt("numTimeoutMs", 1000),
                MaxParallel = GetInt("numMaxParallel", 256),
                PortsToTest = ValidationHelper.ParsePortList(GetText("txtPorts"))
            };

            try
            {
                // Mode A: Scan all NICs
                if (GetBool("chkScanAllNics", false))
                {
                    var ranges = DiscoveryController.GetIPv4NicSubnets(includeDown: false)
                                   .Select(n => (n.Start, n.End))
                                   .ToList();

                    if (ranges.Count == 0)
                    {
                        MessageBox.Show("No active IPv4 NIC subnets found to scan.", "Network Discovery",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UpdateButtons(scanning: false);
                        return;
                    }

                    var targets = DiscoveryController.BuildTargetSet(ranges);
                    Log?.Invoke($"[DISCOVERY] Scanning all NIC subnets ({targets.Count} addresses)…", LogLevel.Info);

                    var results = await _discovery.ScanTargetsAsync(
                        targets,
                        options,
                        onProgress: p => BeginInvoke(new Action(() => SetProgress(p.Scanned, p.Total, p.Active))),
                        onHostFound: r => BeginInvoke(new Action(() => AddResultRow(r)))
                    );

                    // Cache for export
                    _results.AddRange(results);
                }
                else
                {
                    // Mode B: CIDR
                    var cidr = GetText("txtCidr");
                    if (ValidationHelper.IsValidCidr(cidr) &&
                        DiscoveryController.TryParseCidr(cidr, out uint s, out uint e, out int prefix))
                    {
                        Log?.Invoke($"[DISCOVERY] Scanning {cidr} ({e - s + 1} addresses)…", LogLevel.Info);

                        var results = await _discovery.ScanRangeAsync(
                            s, e, options,
                            onProgress: p => BeginInvoke(new Action(() => SetProgress(p.Scanned, p.Total, p.Active))),
                            onHostFound: r => BeginInvoke(new Action(() => AddResultRow(r)))
                        );

                        _results.AddRange(results);
                    }
                    else
                    {
                        // Mode C: Start/End
                        var startStr = GetText("txtRangeStart");
                        var endStr = GetText("txtRangeEnd");

                        if (!ValidationHelper.IsValidIPv4(startStr) || !ValidationHelper.IsValidIPv4(endStr))
                        {
                            MessageBox.Show("Enter a valid CIDR or a valid Start/End IPv4 range.", "Network Discovery",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            UpdateButtons(scanning: false);
                            return;
                        }

                        if (!ValidationHelper.TryParseIPv4(startStr, out uint sU) ||
                            !ValidationHelper.TryParseIPv4(endStr, out uint eU))
                        {
                            MessageBox.Show("Failed to parse range.", "Network Discovery",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            UpdateButtons(scanning: false);
                            return;
                        }

                        if (eU < sU) (sU, eU) = (eU, sU);

                        Log?.Invoke($"[DISCOVERY] Scanning range {startStr} → {endStr} ({eU - sU + 1} addresses)…", LogLevel.Info);

                        var results = await _discovery.ScanRangeAsync(
                            sU, eU, options,
                            onProgress: p => BeginInvoke(new Action(() => SetProgress(p.Scanned, p.Total, p.Active))),
                            onHostFound: r => BeginInvoke(new Action(() => AddResultRow(r)))
                        );

                        _results.AddRange(results);
                    }
                }

                Log?.Invoke($"[DISCOVERY DONE] Active hosts: {_results.Count}", LogLevel.Success);
            }
            catch (Exception ex)
            {
                Log?.Invoke("[ERROR] Discovery failed: " + ex.Message, LogLevel.Error);
                MessageBox.Show("Discovery failed.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateButtons(scanning: false);
            }
        }

        private void OnStop()
        {
            _discovery.Stop();
            UpdateButtons(scanning: false);
            Log?.Invoke("[DISCOVERY] Scan cancelled.", LogLevel.Warning);
        }

        // ===========================
        // NIC dropdown → CIDR autofill
        // ===========================
        private void PopulateNicDropdown()
        {
            var cbo = Find<ComboBox>("cboNic");
            if (cbo == null) return;

            var items = DiscoveryController.GetIPv4NicSubnets(includeDown: false);

            cbo.BeginUpdate();
            try
            {
                cbo.Items.Clear();
                foreach (var n in items)
                {
                    cbo.Items.Add(new NicItem
                    {
                        Display = $"{n.AdapterName}  —  {n.IPv4}/{n.Prefix}",
                        AdapterName = n.AdapterName,
                        IPv4 = n.IPv4,
                        Prefix = n.Prefix
                    });
                }

                if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
            }
            finally
            {
                cbo.EndUpdate();
            }

            // If not "scan all", apply selected
            if (!GetBool("chkScanAllNics", false)) ApplySelectedNicToCidr();
        }

        private void ApplySelectedNicToCidr()
        {
            var cbo = Find<ComboBox>("cboNic");
            var txt = Find<TextBox>("txtCidr");
            if (cbo == null || txt == null) return;
            if (GetBool("chkScanAllNics", false)) return;

            if (cbo.SelectedItem is NicItem nic)
            {
                txt.Text = $"{nic.IPv4}/{nic.Prefix}";
            }
        }

        private sealed class NicItem
        {
            public string Display { get; set; } = "";
            public string AdapterName { get; set; } = "";
            public string IPv4 { get; set; } = "";
            public int Prefix { get; set; }
            public override string ToString() => Display;
        }

        // ===========================
        // Export CSV
        // ===========================
        private void OnExportCsv()
        {
            if (_results.Count == 0)
            {
                Log?.Invoke("No discovery data to export.", LogLevel.Warning);
                MessageBox.Show("No discovery results to export.", "Export CSV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Title = "Export Discovery Results",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = $"NetworkDiscovery_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    bool activeOnly = Properties.Settings.Default.ExportActiveOnly;
                    var csv = DiscoveryController.BuildCsv(_results, includeHeaders: true, activeOnly: activeOnly);

                    File.WriteAllText(sfd.FileName, csv, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

                    Log?.Invoke($"Discovery CSV exported: {sfd.FileName}", LogLevel.Success);
                }
                catch (Exception ex)
                {
                    Log?.Invoke($"Export failed: {ex.Message}", LogLevel.Error);
                    MessageBox.Show("Export failed.\n\n" + ex.Message, "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Log?.Invoke("Export cancelled by user.", LogLevel.Info);
            }
        }

        // ===========================
        // UI helpers
        // ===========================
        private void UpdateButtons(bool scanning)
        {
            Find<Button>("btnStartDiscovery")?.Apply(b => b.Enabled = !scanning);
            Find<Button>("btnStopDiscovery")?.Apply(b => b.Enabled = scanning);
            Find<Button>("btnExportDiscoveryCsv")?.Apply(b => b.Enabled = !scanning && _results.Count > 0);
        }

        private void SetProgress(int scanned, int total, int active)
        {
            var lbl = Find<Label>("lblDiscoveryProgress");
            if (lbl != null)
            {
                lbl.Text = $"Scanned {scanned}/{total} — Active {active}";
            }
        }

        private void AddResultRow(DiscoveryController.DiscoveryResult r)
        {
            var grid = Find<DataGridView>("dgvDiscovery");
            if (grid == null) return;

            // Append to cached list for export
            _results.Add(r);

            grid.Rows.Add(
                r.IP,
                r.Hostname,
                r.MAC,
                r.LatencyMs >= 0 ? r.LatencyMs.ToString() : "",
                r.OpenPorts is { Count: > 0 } ? string.Join(";", r.OpenPorts) : ""
            );
        }

        private void EnsureGrid()
        {
            var grid = Find<DataGridView>("dgvDiscovery");
            if (grid == null) return;

            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            if (grid.Columns.Count == 0)
            {
                grid.Columns.Add(MakeCol("colDiscIp", "IP Address", 140));
                grid.Columns.Add(MakeCol("colDiscHost", "Hostname", 200));
                grid.Columns.Add(MakeCol("colDiscMac", "MAC Address", 140));
                grid.Columns.Add(MakeCol("colDiscLatency", "Latency (ms)", 110));
                grid.Columns.Add(MakeCol("colDiscPorts", "Open Ports", 160));
            }

            AutoSizeColumns(grid);
        }

        private static DataGridViewTextBoxColumn MakeCol(string name, string header, int width)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                Width = width,
                MinimumWidth = Math.Min(80, width),
                ReadOnly = true
            };
        }

        private static void AutoSizeColumns(DataGridView dgv)
        {
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgv.AutoResizeColumns();
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // Generic control helpers
        private T? Find<T>(string name) where T : Control
            => Controls.Find(name, true).FirstOrDefault() as T;

        private string GetText(string name)
            => (Find<TextBox>(name)?.Text ?? string.Empty).Trim();

        private int GetInt(string name, int fallback)
        {
            var nud = Find<NumericUpDown>(name);
            if (nud != null) return (int)nud.Value;
            var txt = Find<TextBox>(name);
            if (txt != null && int.TryParse(txt.Text, out var v)) return v;
            return fallback;
        }

        private bool GetBool(string name, bool fallback)
        {
            var cb = Find<CheckBox>(name);
            if (cb != null) return cb.Checked;
            return fallback;
        }

        private void SetIfMissing(string name, int value)
        {
            var nud = Find<NumericUpDown>(name);
            if (nud != null && nud.Value == 0) nud.Value = value;
            var txt = Find<TextBox>(name);
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text)) txt.Text = value.ToString();
        }
    }
}