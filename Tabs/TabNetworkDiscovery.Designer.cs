using System.Windows.Forms;
using MaterialSkin.Controls;

namespace NetworkUtilityApp.Tabs
{
    partial class TabNetworkDiscovery
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel layoutRoot;

        // Row 0: Source Range
        private GroupBox grpSource;
        private MaterialComboBox cboNic;
        private MaterialCheckbox chkScanAllNics;
        private MaterialLabel lblCidr;
        private MaterialTextBox2 txtCidr;
        private MaterialLabel lblRangeStart;
        private MaterialTextBox2 txtRangeStart;
        private MaterialLabel lblRangeEnd;
        private MaterialTextBox2 txtRangeEnd;

        // Row 1: Options
        private GroupBox grpOptions;
        private MaterialCheckbox chkResolveDns;
        private MaterialLabel lblTimeout;
        private NumericUpDown numTimeoutMs;
        private MaterialLabel lblMaxParallel;
        private NumericUpDown numMaxParallel;
        private MaterialLabel lblPorts;
        private MaterialTextBox2 txtPorts;

        // Row 2: Actions + Progress
        private FlowLayoutPanel flowActions;
        private MaterialButton btnStartDiscovery;
        private MaterialButton btnStopDiscovery;
        private MaterialButton btnExportDiscoveryCsv;
        private Label lblDiscoveryProgress;

        // Row 3: Results
        private DataGridView dgvDiscovery;
        private DataGridViewTextBoxColumn colDiscIp;
        private DataGridViewTextBoxColumn colDiscHost;
        private DataGridViewTextBoxColumn colDiscMac;
        private DataGridViewTextBoxColumn colDiscLatency;
        private DataGridViewTextBoxColumn colDiscPorts;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.layoutRoot = new System.Windows.Forms.TableLayoutPanel();

            this.grpSource = new System.Windows.Forms.GroupBox();
            this.cboNic = new MaterialSkin.Controls.MaterialComboBox();
            this.chkScanAllNics = new MaterialSkin.Controls.MaterialCheckbox();
            this.lblCidr = new MaterialSkin.Controls.MaterialLabel();
            this.txtCidr = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblRangeStart = new MaterialSkin.Controls.MaterialLabel();
            this.txtRangeStart = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblRangeEnd = new MaterialSkin.Controls.MaterialLabel();
            this.txtRangeEnd = new MaterialSkin.Controls.MaterialTextBox2();

            this.grpOptions = new System.Windows.Forms.GroupBox();
            this.chkResolveDns = new MaterialSkin.Controls.MaterialCheckbox();
            this.lblTimeout = new MaterialSkin.Controls.MaterialLabel();
            this.numTimeoutMs = new System.Windows.Forms.NumericUpDown();
            this.lblMaxParallel = new MaterialSkin.Controls.MaterialLabel();
            this.numMaxParallel = new System.Windows.Forms.NumericUpDown();
            this.lblPorts = new MaterialSkin.Controls.MaterialLabel();
            this.txtPorts = new MaterialSkin.Controls.MaterialTextBox2();

            this.flowActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnStartDiscovery = new MaterialSkin.Controls.MaterialButton();
            this.btnStopDiscovery = new MaterialSkin.Controls.MaterialButton();
            this.btnExportDiscoveryCsv = new MaterialSkin.Controls.MaterialButton();
            this.lblDiscoveryProgress = new System.Windows.Forms.Label();

            this.dgvDiscovery = new System.Windows.Forms.DataGridView();
            this.colDiscIp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDiscHost = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDiscMac = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDiscLatency = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDiscPorts = new System.Windows.Forms.DataGridViewTextBoxColumn();

            this.layoutRoot.SuspendLayout();
            this.grpSource.SuspendLayout();
            this.grpOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeoutMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxParallel)).BeginInit();
            this.flowActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDiscovery)).BeginInit();
            this.SuspendLayout();

            // layoutRoot
            this.layoutRoot.ColumnCount = 1;
            this.layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.layoutRoot.RowCount = 4;
            this.layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // grpSource
            this.layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // grpOptions
            this.layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // actions
            this.layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // grid
            this.layoutRoot.Dock = DockStyle.Fill;
            this.layoutRoot.Padding = new Padding(12);
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.Size = new System.Drawing.Size(1000, 650);

            // =========================
            // grpSource (Row 0)
            // =========================
            this.grpSource.Dock = DockStyle.Top;
            this.grpSource.Padding = new Padding(12);
            this.grpSource.Margin = new Padding(0, 0, 0, 12);
            this.grpSource.Text = "Source Range (choose NIC and/or enter CIDR or Start/End)";
            this.grpSource.Name = "grpSource";
            this.grpSource.Size = new System.Drawing.Size(976, 124);

            // cboNic
            this.cboNic.AutoResize = false;
            this.cboNic.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.cboNic.Depth = 0;
            this.cboNic.DrawMode = DrawMode.OwnerDrawVariable;
            this.cboNic.DropDownHeight = 174;
            this.cboNic.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboNic.DropDownWidth = 121;
            this.cboNic.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.cboNic.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
            this.cboNic.FormattingEnabled = true;
            this.cboNic.IntegralHeight = false;
            this.cboNic.ItemHeight = 43;
            this.cboNic.Location = new System.Drawing.Point(16, 28);
            this.cboNic.MaxDropDownItems = 4;
            this.cboNic.MouseState = MaterialSkin.MouseState.OUT;
            this.cboNic.Name = "cboNic";
            this.cboNic.Size = new System.Drawing.Size(420, 49);
            this.cboNic.StartIndex = -1;
            this.cboNic.TabIndex = 0;
            this.cboNic.Hint = "Select NIC (auto-fills CIDR)";

            // chkScanAllNics
            this.chkScanAllNics.AutoSize = true;
            this.chkScanAllNics.Depth = 0;
            this.chkScanAllNics.Location = new System.Drawing.Point(452, 34);
            this.chkScanAllNics.Margin = new Padding(0);
            this.chkScanAllNics.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkScanAllNics.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkScanAllNics.Name = "chkScanAllNics";
            this.chkScanAllNics.ReadOnly = false;
            this.chkScanAllNics.Ripple = true;
            this.chkScanAllNics.Size = new System.Drawing.Size(165, 37);
            this.chkScanAllNics.TabIndex = 1;
            this.chkScanAllNics.Text = "Scan all NICs";
            this.chkScanAllNics.UseVisualStyleBackColor = true;

            // lblCidr
            this.lblCidr.AutoSize = true;
            this.lblCidr.Depth = 0;
            this.lblCidr.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblCidr.Location = new System.Drawing.Point(16, 84);
            this.lblCidr.Name = "lblCidr";
            this.lblCidr.Size = new System.Drawing.Size(36, 19);
            this.lblCidr.Text = "CIDR";

            // txtCidr
            this.txtCidr.AnimateReadOnly = false;
           // this.txtCidr.BorderStyle = BorderStyle.None;
            this.txtCidr.Depth = 0;
            this.txtCidr.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txtCidr.LeadingIcon = null;
            this.txtCidr.Location = new System.Drawing.Point(62, 74);
            this.txtCidr.MaxLength = 32767;
            this.txtCidr.MouseState = MaterialSkin.MouseState.OUT;
            this.txtCidr.Name = "txtCidr";
            this.txtCidr.PasswordChar = '\0';
            this.txtCidr.PrefixSuffixText = null;
            this.txtCidr.ReadOnly = false;
            this.txtCidr.RightToLeft = RightToLeft.No;
            this.txtCidr.SelectedText = "";
            this.txtCidr.SelectionLength = 0;
            this.txtCidr.SelectionStart = 0;
            this.txtCidr.ShortcutsEnabled = true;
            this.txtCidr.Size = new System.Drawing.Size(200, 36);
            this.txtCidr.TabIndex = 2;
            this.txtCidr.TabStop = false;
            this.txtCidr.Text = "";
            this.txtCidr.TrailingIcon = null;
            this.txtCidr.Hint = "e.g. 192.168.1.0/24";

            // lblRangeStart
            this.lblRangeStart.AutoSize = true;
            this.lblRangeStart.Depth = 0;
            this.lblRangeStart.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblRangeStart.Location = new System.Drawing.Point(280, 84);
            this.lblRangeStart.Name = "lblRangeStart";
            this.lblRangeStart.Size = new System.Drawing.Size(34, 19);
            this.lblRangeStart.Text = "Start";

            // txtRangeStart
            this.txtRangeStart.AnimateReadOnly = false;
            //this.txtRangeStart.BorderStyle = BorderStyle.None;
            this.txtRangeStart.Depth = 0;
            this.txtRangeStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txtRangeStart.LeadingIcon = null;
            this.txtRangeStart.Location = new System.Drawing.Point(322, 74);
            this.txtRangeStart.MaxLength = 32767;
            this.txtRangeStart.MouseState = MaterialSkin.MouseState.OUT;
            this.txtRangeStart.Name = "txtRangeStart";
            this.txtRangeStart.PasswordChar = '\0';
            this.txtRangeStart.PrefixSuffixText = null;
            this.txtRangeStart.ReadOnly = false;
            this.txtRangeStart.RightToLeft = RightToLeft.No;
            this.txtRangeStart.SelectedText = "";
            this.txtRangeStart.SelectionLength = 0;
            this.txtRangeStart.SelectionStart = 0;
            this.txtRangeStart.ShortcutsEnabled = true;
            this.txtRangeStart.Size = new System.Drawing.Size(180, 36);
            this.txtRangeStart.TabIndex = 3;
            this.txtRangeStart.TabStop = false;
            this.txtRangeStart.Text = "";
            this.txtRangeStart.TrailingIcon = null;
            this.txtRangeStart.Hint = "e.g. 192.168.1.10";

            // lblRangeEnd
            this.lblRangeEnd.AutoSize = true;
            this.lblRangeEnd.Depth = 0;
            this.lblRangeEnd.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblRangeEnd.Location = new System.Drawing.Point(518, 84);
            this.lblRangeEnd.Name = "lblRangeEnd";
            this.lblRangeEnd.Size = new System.Drawing.Size(30, 19);
            this.lblRangeEnd.Text = "End";

            // txtRangeEnd
            this.txtRangeEnd.AnimateReadOnly = false;
            //this.txtRangeEnd.BorderStyle = BorderStyle.None;
            this.txtRangeEnd.Depth = 0;
            this.txtRangeEnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txtRangeEnd.LeadingIcon = null;
            this.txtRangeEnd.Location = new System.Drawing.Point(556, 74);
            this.txtRangeEnd.MaxLength = 32767;
            this.txtRangeEnd.MouseState = MaterialSkin.MouseState.OUT;
            this.txtRangeEnd.Name = "txtRangeEnd";
            this.txtRangeEnd.PasswordChar = '\0';
            this.txtRangeEnd.PrefixSuffixText = null;
            this.txtRangeEnd.ReadOnly = false;
            this.txtRangeEnd.RightToLeft = RightToLeft.No;
            this.txtRangeEnd.SelectedText = "";
            this.txtRangeEnd.SelectionLength = 0;
            this.txtRangeEnd.SelectionStart = 0;
            this.txtRangeEnd.ShortcutsEnabled = true;
            this.txtRangeEnd.Size = new System.Drawing.Size(180, 36);
            this.txtRangeEnd.TabIndex = 4;
            this.txtRangeEnd.TabStop = false;
            this.txtRangeEnd.Text = "";
            this.txtRangeEnd.TrailingIcon = null;
            this.txtRangeEnd.Hint = "e.g. 192.168.1.200";

            this.grpSource.Controls.Add(this.cboNic);
            this.grpSource.Controls.Add(this.chkScanAllNics);
            this.grpSource.Controls.Add(this.lblCidr);
            this.grpSource.Controls.Add(this.txtCidr);
            this.grpSource.Controls.Add(this.lblRangeStart);
            this.grpSource.Controls.Add(this.txtRangeStart);
            this.grpSource.Controls.Add(this.lblRangeEnd);
            this.grpSource.Controls.Add(this.txtRangeEnd);

            // =========================
            // grpOptions (Row 1)
            // =========================
            this.grpOptions.Dock = DockStyle.Top;
            this.grpOptions.Padding = new Padding(12);
            this.grpOptions.Margin = new Padding(0, 0, 0, 12);
            this.grpOptions.Text = "Options";
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Size = new System.Drawing.Size(976, 106);

            // chkResolveDns
            this.chkResolveDns.AutoSize = true;
            this.chkResolveDns.Depth = 0;
            this.chkResolveDns.Location = new System.Drawing.Point(16, 28);
            this.chkResolveDns.Margin = new Padding(0);
            this.chkResolveDns.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkResolveDns.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkResolveDns.Name = "chkResolveDns";
            this.chkResolveDns.ReadOnly = false;
            this.chkResolveDns.Ripple = true;
            this.chkResolveDns.Size = new System.Drawing.Size(161, 37);
            this.chkResolveDns.TabIndex = 0;
            this.chkResolveDns.Text = "Resolve DNS";
            this.chkResolveDns.UseVisualStyleBackColor = true;

            // lblTimeout
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Depth = 0;
            this.lblTimeout.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblTimeout.Location = new System.Drawing.Point(200, 36);
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Size = new System.Drawing.Size(108, 19);
            this.lblTimeout.Text = "Timeout (ms)";

            // numTimeoutMs
            this.numTimeoutMs.Location = new System.Drawing.Point(316, 32);
            this.numTimeoutMs.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            this.numTimeoutMs.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numTimeoutMs.Name = "numTimeoutMs";
            this.numTimeoutMs.Size = new System.Drawing.Size(90, 23);
            this.numTimeoutMs.TabIndex = 1;
            this.numTimeoutMs.Value = new decimal(new int[] { 1000, 0, 0, 0 });

            // lblMaxParallel
            this.lblMaxParallel.AutoSize = true;
            this.lblMaxParallel.Depth = 0;
            this.lblMaxParallel.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblMaxParallel.Location = new System.Drawing.Point(424, 36);
            this.lblMaxParallel.Name = "lblMaxParallel";
            this.lblMaxParallel.Size = new System.Drawing.Size(132, 19);
            this.lblMaxParallel.Text = "Max parallelism";

            // numMaxParallel
            this.numMaxParallel.Location = new System.Drawing.Point(564, 32);
            this.numMaxParallel.Maximum = new decimal(new int[] { 2048, 0, 0, 0 });
            this.numMaxParallel.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMaxParallel.Name = "numMaxParallel";
            this.numMaxParallel.Size = new System.Drawing.Size(90, 23);
            this.numMaxParallel.TabIndex = 2;
            this.numMaxParallel.Value = new decimal(new int[] { 256, 0, 0, 0 });

            // lblPorts
            this.lblPorts.AutoSize = true;
            this.lblPorts.Depth = 0;
            this.lblPorts.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblPorts.Location = new System.Drawing.Point(670, 36);
            this.lblPorts.Name = "lblPorts";
            this.lblPorts.Size = new System.Drawing.Size(136, 19);
            this.lblPorts.Text = "Ports (comma list)";

            // txtPorts
            this.txtPorts.AnimateReadOnly = false;
            //this.txtPorts.BorderStyle = BorderStyle.None;
            this.txtPorts.Depth = 0;
            this.txtPorts.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txtPorts.LeadingIcon = null;
            this.txtPorts.Location = new System.Drawing.Point(812, 26);
            this.txtPorts.MaxLength = 32767;
            this.txtPorts.MouseState = MaterialSkin.MouseState.OUT;
            this.txtPorts.Name = "txtPorts";
            this.txtPorts.PasswordChar = '\0';
            this.txtPorts.PrefixSuffixText = null;
            this.txtPorts.ReadOnly = false;
            this.txtPorts.RightToLeft = RightToLeft.No;
            this.txtPorts.SelectedText = "";
            this.txtPorts.SelectionLength = 0;
            this.txtPorts.SelectionStart = 0;
            this.txtPorts.ShortcutsEnabled = true;
            this.txtPorts.Size = new System.Drawing.Size(150, 36);
            this.txtPorts.TabIndex = 3;
            this.txtPorts.TabStop = false;
            this.txtPorts.Text = "";
            this.txtPorts.TrailingIcon = null;
            this.txtPorts.Hint = "e.g. 22,80,443";

            this.grpOptions.Controls.Add(this.chkResolveDns);
            this.grpOptions.Controls.Add(this.lblTimeout);
            this.grpOptions.Controls.Add(this.numTimeoutMs);
            this.grpOptions.Controls.Add(this.lblMaxParallel);
            this.grpOptions.Controls.Add(this.numMaxParallel);
            this.grpOptions.Controls.Add(this.lblPorts);
            this.grpOptions.Controls.Add(this.txtPorts);

            // =========================
            // flowActions (Row 2)
            // =========================
            this.flowActions.Dock = DockStyle.Top;
            this.flowActions.FlowDirection = FlowDirection.LeftToRight;
            this.flowActions.WrapContents = false;
            this.flowActions.Margin = new Padding(0, 0, 0, 8);
            this.flowActions.Padding = new Padding(0);
            this.flowActions.Name = "flowActions";
            this.flowActions.Size = new System.Drawing.Size(976, 44);

            // btnStartDiscovery
            this.btnStartDiscovery.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.btnStartDiscovery.Density = MaterialButton.MaterialButtonDensity.Default;
            this.btnStartDiscovery.HighEmphasis = true;
            this.btnStartDiscovery.Icon = null;
            this.btnStartDiscovery.Name = "btnStartDiscovery";
            this.btnStartDiscovery.Text = "Start Scan";
            this.btnStartDiscovery.Type = MaterialButton.MaterialButtonType.Contained;
            this.btnStartDiscovery.UseAccentColor = false;
            this.btnStartDiscovery.Size = new System.Drawing.Size(110, 36);
            this.btnStartDiscovery.Margin = new Padding(0, 0, 8, 0);
            this.btnStartDiscovery.TabIndex = 0;

            // btnStopDiscovery
            this.btnStopDiscovery.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.btnStopDiscovery.Density = MaterialButton.MaterialButtonDensity.Default;
            this.btnStopDiscovery.HighEmphasis = false;
            this.btnStopDiscovery.Icon = null;
            this.btnStopDiscovery.Name = "btnStopDiscovery";
            this.btnStopDiscovery.Text = "Stop";
            this.btnStopDiscovery.Type = MaterialButton.MaterialButtonType.Outlined;
            this.btnStopDiscovery.UseAccentColor = false;
            this.btnStopDiscovery.Size = new System.Drawing.Size(80, 36);
            this.btnStopDiscovery.Margin = new Padding(0, 0, 8, 0);
            this.btnStopDiscovery.TabIndex = 1;

            // btnExportDiscoveryCsv
            this.btnExportDiscoveryCsv.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.btnExportDiscoveryCsv.Density = MaterialButton.MaterialButtonDensity.Default;
            this.btnExportDiscoveryCsv.HighEmphasis = false;
            this.btnExportDiscoveryCsv.Icon = null;
            this.btnExportDiscoveryCsv.Name = "btnExportDiscoveryCsv";
            this.btnExportDiscoveryCsv.Text = "Export CSV";
            this.btnExportDiscoveryCsv.Type = MaterialButton.MaterialButtonType.Outlined;
            this.btnExportDiscoveryCsv.UseAccentColor = false;
            this.btnExportDiscoveryCsv.Size = new System.Drawing.Size(110, 36);
            this.btnExportDiscoveryCsv.Margin = new Padding(0, 0, 16, 0);
            this.btnExportDiscoveryCsv.TabIndex = 2;

            // lblDiscoveryProgress
            this.lblDiscoveryProgress.AutoSize = true;
            this.lblDiscoveryProgress.Text = "Scanned 0/0 — Active 0";
            this.lblDiscoveryProgress.Margin = new Padding(8, 8, 0, 0);
            this.lblDiscoveryProgress.Name = "lblDiscoveryProgress";
            this.lblDiscoveryProgress.Anchor = AnchorStyles.Left;
            this.lblDiscoveryProgress.AutoEllipsis = true;

            this.flowActions.Controls.Add(this.btnStartDiscovery);
            this.flowActions.Controls.Add(this.btnStopDiscovery);
            this.flowActions.Controls.Add(this.btnExportDiscoveryCsv);
            this.flowActions.Controls.Add(this.lblDiscoveryProgress);

            // =========================
            // dgvDiscovery (Row 3)
            // =========================
            this.dgvDiscovery.AllowUserToAddRows = false;
            this.dgvDiscovery.AllowUserToDeleteRows = false;
            this.dgvDiscovery.ReadOnly = true;
            this.dgvDiscovery.MultiSelect = false;
            this.dgvDiscovery.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvDiscovery.RowHeadersVisible = false;
            this.dgvDiscovery.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDiscovery.Dock = DockStyle.Fill;
            this.dgvDiscovery.Name = "dgvDiscovery";
            this.dgvDiscovery.TabIndex = 99;

            this.colDiscIp = new System.Windows.Forms.DataGridViewTextBoxColumn { HeaderText = "IP Address", Name = "colDiscIp", ReadOnly = true, MinimumWidth = 100 };
            this.colDiscHost = new System.Windows.Forms.DataGridViewTextBoxColumn { HeaderText = "Hostname", Name = "colDiscHost", ReadOnly = true, MinimumWidth = 140 };
            this.colDiscMac = new System.Windows.Forms.DataGridViewTextBoxColumn { HeaderText = "MAC Address", Name = "colDiscMac", ReadOnly = true, MinimumWidth = 120 };
            this.colDiscLatency = new System.Windows.Forms.DataGridViewTextBoxColumn { HeaderText = "Latency (ms)", Name = "colDiscLatency", ReadOnly = true, MinimumWidth = 110 };
            this.colDiscPorts = new System.Windows.Forms.DataGridViewTextBoxColumn { HeaderText = "Open Ports", Name = "colDiscPorts", ReadOnly = true, MinimumWidth = 140 };

            this.dgvDiscovery.Columns.AddRange(new DataGridViewColumn[] {
                this.colDiscIp, this.colDiscHost, this.colDiscMac, this.colDiscLatency, this.colDiscPorts
            });

            // Add to layoutRoot
            this.layoutRoot.Controls.Add(this.grpSource, 0, 0);
            this.layoutRoot.Controls.Add(this.grpOptions, 0, 1);
            this.layoutRoot.Controls.Add(this.flowActions, 0, 2);
            this.layoutRoot.Controls.Add(this.dgvDiscovery, 0, 3);

            // TabNetworkDiscovery (UserControl)
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Controls.Add(this.layoutRoot);
            this.Name = "TabNetworkDiscovery";
            this.Size = new System.Drawing.Size(1000, 650);

            this.layoutRoot.ResumeLayout(false);
            this.grpSource.ResumeLayout(false);
            this.grpSource.PerformLayout();
            this.grpOptions.ResumeLayout(false);
            this.grpOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeoutMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxParallel)).EndInit();
            this.flowActions.ResumeLayout(false);
            this.flowActions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDiscovery)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
