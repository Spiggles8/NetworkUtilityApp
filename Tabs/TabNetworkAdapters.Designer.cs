using System.Drawing;
using System.Windows.Forms;

namespace NetworkUtilityApp.Tabs
{
    partial class TabNetworkAdapters
    {
        private System.ComponentModel.IContainer components = null;

        // -----------------------------
        // Designer-managed control fields
        // -----------------------------
        // Root layout: 4 rows (Header, Grid, Config, Log)
        private TableLayoutPanel layoutRoot;

        // Header row: actions + selected label
        private FlowLayoutPanel flowHeader;
        private Button btnRefresh;
        private Label lblSelectedAdapter;

        // Main grid for adapters
        private DataGridView dgvAdapters;
        private DataGridViewTextBoxColumn colAdapterName;
        private DataGridViewTextBoxColumn colDhcp;
        private DataGridViewTextBoxColumn colIpAddress;
        private DataGridViewTextBoxColumn colSubnet;
        private DataGridViewTextBoxColumn colGateway;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colHardwareDetails;
        private DataGridViewTextBoxColumn colMacAddress;

        // Static configuration section controls (IP / Mask / Gateway)
        private GroupBox grpConfig;
        private Label lblIP;
        private TextBox txtIP1;
        private TextBox txtIP2;
        private TextBox txtIP3;
        private TextBox txtIP4;

        private Label lblSubnet;
        private TextBox txtSubnet1;
        private TextBox txtSubnet2;
        private TextBox txtSubnet3;
        private TextBox txtSubnet4;

        private Label lblGateway;
        private TextBox txtGateway1;
        private TextBox txtGateway2;
        private TextBox txtGateway3;
        private TextBox txtGateway4;

        private FlowLayoutPanel flowConfigButtons;
        private Button btnSetDhcp;
        private Button btnSetStatic;

        // Favorite IP Address Buttons
        private Button BtnFavIPAddress3;
        private Button BtnFavIPAddress2;
        private Label lblFavIPAddresses;
        private Button BtnFavIPAddress1;
        private Button BtnFavIPAddress4;


        /// <summary>
        /// Dispose pattern for designer components.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Initialize the visual tree and basic properties for controls.
        /// This method is designer-generated style but annotated with comments
        /// to make structure and intent clearer for maintainers.
        /// </summary>
        private void InitializeComponent()
        {
            layoutRoot = new TableLayoutPanel();
            flowHeader = new FlowLayoutPanel();
            btnRefresh = new Button();
            lblSelectedAdapter = new Label();
            dgvAdapters = new DataGridView();
            colAdapterName = new DataGridViewTextBoxColumn();
            colDhcp = new DataGridViewTextBoxColumn();
            colIpAddress = new DataGridViewTextBoxColumn();
            colSubnet = new DataGridViewTextBoxColumn();
            colGateway = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            colHardwareDetails = new DataGridViewTextBoxColumn();
            colMacAddress = new DataGridViewTextBoxColumn();
            grpConfig = new GroupBox();
            BtnFavIPAddress4 = new Button();
            BtnFavIPAddress3 = new Button();
            BtnFavIPAddress2 = new Button();
            lblFavIPAddresses = new Label();
            BtnFavIPAddress1 = new Button();
            lblIP = new Label();
            txtIP1 = new TextBox();
            txtIP2 = new TextBox();
            txtIP3 = new TextBox();
            txtIP4 = new TextBox();
            lblSubnet = new Label();
            txtSubnet1 = new TextBox();
            txtSubnet2 = new TextBox();
            txtSubnet3 = new TextBox();
            txtSubnet4 = new TextBox();
            lblGateway = new Label();
            txtGateway1 = new TextBox();
            txtGateway2 = new TextBox();
            txtGateway3 = new TextBox();
            txtGateway4 = new TextBox();
            flowConfigButtons = new FlowLayoutPanel();
            btnSetDhcp = new Button();
            btnSetStatic = new Button();
            layoutRoot.SuspendLayout();
            flowHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAdapters).BeginInit();
            grpConfig.SuspendLayout();
            flowConfigButtons.SuspendLayout();
            SuspendLayout();
            // 
            // layoutRoot
            // 
            layoutRoot.ColumnCount = 1;
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutRoot.Controls.Add(flowHeader, 0, 0);
            layoutRoot.Controls.Add(dgvAdapters, 0, 1);
            layoutRoot.Controls.Add(grpConfig, 0, 2);
            layoutRoot.Dock = DockStyle.Fill;
            layoutRoot.Location = new Point(0, 0);
            layoutRoot.Name = "layoutRoot";
            layoutRoot.Padding = new Padding(12);
            layoutRoot.RowCount = 3;
            layoutRoot.RowStyles.Add(new RowStyle());
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutRoot.RowStyles.Add(new RowStyle());
            layoutRoot.Size = new Size(1000, 650);
            layoutRoot.TabIndex = 0;
            // 
            // flowHeader
            // 
            flowHeader.AutoSize = true;
            flowHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowHeader.Controls.Add(btnRefresh);
            flowHeader.Controls.Add(lblSelectedAdapter);
            flowHeader.Dock = DockStyle.Top;
            flowHeader.Location = new Point(12, 12);
            flowHeader.Margin = new Padding(0, 0, 0, 8);
            flowHeader.Name = "flowHeader";
            flowHeader.Size = new Size(976, 25);
            flowHeader.TabIndex = 0;
            flowHeader.WrapContents = false;
            // 
            // btnRefresh
            // 
            btnRefresh.AutoSize = true;
            btnRefresh.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRefresh.Location = new Point(0, 0);
            btnRefresh.Margin = new Padding(0, 0, 12, 0);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(106, 25);
            btnRefresh.TabIndex = 0;
            btnRefresh.Text = "Refresh Adapters";
            // 
            // lblSelectedAdapter
            // 
            lblSelectedAdapter.AutoSize = true;
            lblSelectedAdapter.Location = new Point(118, 6);
            lblSelectedAdapter.Margin = new Padding(0, 6, 0, 0);
            lblSelectedAdapter.Name = "lblSelectedAdapter";
            lblSelectedAdapter.Size = new Size(131, 15);
            lblSelectedAdapter.TabIndex = 1;
            lblSelectedAdapter.Text = "Selected Adapter: None";
            // 
            // dgvAdapters
            // 
            dgvAdapters.AllowUserToAddRows = false;
            dgvAdapters.AllowUserToDeleteRows = false;
            dgvAdapters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAdapters.Columns.AddRange(new DataGridViewColumn[] { colAdapterName, colDhcp, colIpAddress, colSubnet, colGateway, colStatus, colHardwareDetails, colMacAddress });
            dgvAdapters.Dock = DockStyle.Fill;
            dgvAdapters.Location = new Point(15, 48);
            dgvAdapters.MultiSelect = false;
            dgvAdapters.Name = "dgvAdapters";
            dgvAdapters.ReadOnly = true;
            dgvAdapters.RowHeadersVisible = false;
            dgvAdapters.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAdapters.Size = new Size(970, 201);
            dgvAdapters.TabIndex = 1;
            // 
            // colAdapterName
            // 
            colAdapterName.HeaderText = "Adapter";
            colAdapterName.MinimumWidth = 120;
            colAdapterName.Name = "colAdapterName";
            colAdapterName.ReadOnly = true;
            // 
            // colDhcp
            // 
            colDhcp.HeaderText = "DHCP";
            colDhcp.MinimumWidth = 80;
            colDhcp.Name = "colDhcp";
            colDhcp.ReadOnly = true;
            // 
            // colIpAddress
            // 
            colIpAddress.HeaderText = "IP Address";
            colIpAddress.MinimumWidth = 120;
            colIpAddress.Name = "colIpAddress";
            colIpAddress.ReadOnly = true;
            // 
            // colSubnet
            // 
            colSubnet.HeaderText = "Subnet";
            colSubnet.MinimumWidth = 120;
            colSubnet.Name = "colSubnet";
            colSubnet.ReadOnly = true;
            // 
            // colGateway
            // 
            colGateway.HeaderText = "Gateway";
            colGateway.MinimumWidth = 120;
            colGateway.Name = "colGateway";
            colGateway.ReadOnly = true;
            // 
            // colStatus
            // 
            colStatus.HeaderText = "Status";
            colStatus.MinimumWidth = 100;
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            // 
            // colHardwareDetails
            // 
            colHardwareDetails.HeaderText = "Hardware Details";
            colHardwareDetails.MinimumWidth = 180;
            colHardwareDetails.Name = "colHardwareDetails";
            colHardwareDetails.ReadOnly = true;
            // 
            // colMacAddress
            // 
            colMacAddress.HeaderText = "MAC Address";
            colMacAddress.MinimumWidth = 140;
            colMacAddress.Name = "colMacAddress";
            colMacAddress.ReadOnly = true;
            // 
            // grpConfig
            // 
            grpConfig.AutoSize = true;
            grpConfig.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpConfig.Controls.Add(BtnFavIPAddress4);
            grpConfig.Controls.Add(BtnFavIPAddress3);
            grpConfig.Controls.Add(BtnFavIPAddress2);
            grpConfig.Controls.Add(lblFavIPAddresses);
            grpConfig.Controls.Add(BtnFavIPAddress1);
            grpConfig.Controls.Add(lblIP);
            grpConfig.Controls.Add(txtIP1);
            grpConfig.Controls.Add(txtIP2);
            grpConfig.Controls.Add(txtIP3);
            grpConfig.Controls.Add(txtIP4);
            grpConfig.Controls.Add(lblSubnet);
            grpConfig.Controls.Add(txtSubnet1);
            grpConfig.Controls.Add(txtSubnet2);
            grpConfig.Controls.Add(txtSubnet3);
            grpConfig.Controls.Add(txtSubnet4);
            grpConfig.Controls.Add(lblGateway);
            grpConfig.Controls.Add(txtGateway1);
            grpConfig.Controls.Add(txtGateway2);
            grpConfig.Controls.Add(txtGateway3);
            grpConfig.Controls.Add(txtGateway4);
            grpConfig.Controls.Add(flowConfigButtons);
            grpConfig.Dock = DockStyle.Top;
            grpConfig.Location = new Point(12, 260);
            grpConfig.Margin = new Padding(0, 8, 0, 0);
            grpConfig.Name = "grpConfig";
            grpConfig.Padding = new Padding(12);
            grpConfig.Size = new Size(976, 198);
            grpConfig.TabIndex = 2;
            grpConfig.TabStop = false;
            grpConfig.Text = "Set Network Configuration";
            // 
            // BtnFavIPAddress4
            // 
            BtnFavIPAddress4.Location = new Point(451, 144);
            BtnFavIPAddress4.Name = "BtnFavIPAddress4";
            BtnFavIPAddress4.Size = new Size(125, 23);
            BtnFavIPAddress4.TabIndex = 21;
            BtnFavIPAddress4.Text = "10.0.0.8";
            BtnFavIPAddress4.UseVisualStyleBackColor = true;
            BtnFavIPAddress4.Click += BtnFavIPAddress4_Click;
            // 
            // BtnFavIPAddress3
            // 
            BtnFavIPAddress3.Location = new Point(451, 115);
            BtnFavIPAddress3.Name = "BtnFavIPAddress3";
            BtnFavIPAddress3.Size = new Size(125, 23);
            BtnFavIPAddress3.TabIndex = 20;
            BtnFavIPAddress3.Text = "172.22.0.8";
            BtnFavIPAddress3.UseVisualStyleBackColor = true;
            BtnFavIPAddress3.Click += BtnFavIPAddress3_Click;
            // 
            // BtnFavIPAddress2
            // 
            BtnFavIPAddress2.Location = new Point(451, 86);
            BtnFavIPAddress2.Name = "BtnFavIPAddress2";
            BtnFavIPAddress2.Size = new Size(125, 23);
            BtnFavIPAddress2.TabIndex = 19;
            BtnFavIPAddress2.Text = "192.168.8.8";
            BtnFavIPAddress2.UseVisualStyleBackColor = true;
            BtnFavIPAddress2.Click += BtnFavIPAddress2_Click;
            // 
            // lblFavIPAddresses
            // 
            lblFavIPAddresses.AutoSize = true;
            lblFavIPAddresses.Location = new Point(458, 25);
            lblFavIPAddresses.Name = "lblFavIPAddresses";
            lblFavIPAddresses.Size = new Size(118, 15);
            lblFavIPAddresses.TabIndex = 17;
            lblFavIPAddresses.Text = "Favorite IP Addresses";
            // 
            // BtnFavIPAddress1
            // 
            BtnFavIPAddress1.Location = new Point(451, 56);
            BtnFavIPAddress1.Name = "BtnFavIPAddress1";
            BtnFavIPAddress1.Size = new Size(125, 23);
            BtnFavIPAddress1.TabIndex = 16;
            BtnFavIPAddress1.Text = "192.168.1.8";
            BtnFavIPAddress1.UseVisualStyleBackColor = true;
            BtnFavIPAddress1.Click += BtnFavIPAddress1_Click;
            // 
            // lblIP
            // 
            lblIP.AutoSize = true;
            lblIP.Location = new Point(16, 32);
            lblIP.Name = "lblIP";
            lblIP.Size = new Size(65, 15);
            lblIP.TabIndex = 0;
            lblIP.Text = "IP Address:";
            // 
            // txtIP1
            // 
            txtIP1.Location = new Point(120, 28);
            txtIP1.Name = "txtIP1";
            txtIP1.Size = new Size(44, 23);
            txtIP1.TabIndex = 1;
            // 
            // txtIP2
            // 
            txtIP2.Location = new Point(170, 28);
            txtIP2.Name = "txtIP2";
            txtIP2.Size = new Size(44, 23);
            txtIP2.TabIndex = 2;
            // 
            // txtIP3
            // 
            txtIP3.Location = new Point(220, 28);
            txtIP3.Name = "txtIP3";
            txtIP3.Size = new Size(44, 23);
            txtIP3.TabIndex = 3;
            // 
            // txtIP4
            // 
            txtIP4.Location = new Point(270, 28);
            txtIP4.Name = "txtIP4";
            txtIP4.Size = new Size(44, 23);
            txtIP4.TabIndex = 4;
            // 
            // lblSubnet
            // 
            lblSubnet.AutoSize = true;
            lblSubnet.Location = new Point(16, 64);
            lblSubnet.Name = "lblSubnet";
            lblSubnet.Size = new Size(78, 15);
            lblSubnet.TabIndex = 5;
            lblSubnet.Text = "Subnet Mask:";
            // 
            // txtSubnet1
            // 
            txtSubnet1.Location = new Point(120, 60);
            txtSubnet1.Name = "txtSubnet1";
            txtSubnet1.Size = new Size(44, 23);
            txtSubnet1.TabIndex = 6;
            // 
            // txtSubnet2
            // 
            txtSubnet2.Location = new Point(170, 60);
            txtSubnet2.Name = "txtSubnet2";
            txtSubnet2.Size = new Size(44, 23);
            txtSubnet2.TabIndex = 7;
            // 
            // txtSubnet3
            // 
            txtSubnet3.Location = new Point(220, 60);
            txtSubnet3.Name = "txtSubnet3";
            txtSubnet3.Size = new Size(44, 23);
            txtSubnet3.TabIndex = 8;
            // 
            // txtSubnet4
            // 
            txtSubnet4.Location = new Point(270, 60);
            txtSubnet4.Name = "txtSubnet4";
            txtSubnet4.Size = new Size(44, 23);
            txtSubnet4.TabIndex = 9;
            // 
            // lblGateway
            // 
            lblGateway.AutoSize = true;
            lblGateway.Location = new Point(16, 96);
            lblGateway.Name = "lblGateway";
            lblGateway.Size = new Size(55, 15);
            lblGateway.TabIndex = 10;
            lblGateway.Text = "Gateway:";
            // 
            // txtGateway1
            // 
            txtGateway1.Location = new Point(120, 92);
            txtGateway1.Name = "txtGateway1";
            txtGateway1.Size = new Size(44, 23);
            txtGateway1.TabIndex = 11;
            // 
            // txtGateway2
            // 
            txtGateway2.Location = new Point(170, 92);
            txtGateway2.Name = "txtGateway2";
            txtGateway2.Size = new Size(44, 23);
            txtGateway2.TabIndex = 12;
            // 
            // txtGateway3
            // 
            txtGateway3.Location = new Point(220, 92);
            txtGateway3.Name = "txtGateway3";
            txtGateway3.Size = new Size(44, 23);
            txtGateway3.TabIndex = 13;
            // 
            // txtGateway4
            // 
            txtGateway4.Location = new Point(270, 92);
            txtGateway4.Name = "txtGateway4";
            txtGateway4.Size = new Size(44, 23);
            txtGateway4.TabIndex = 14;
            // 
            // flowConfigButtons
            // 
            flowConfigButtons.AutoSize = true;
            flowConfigButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowConfigButtons.Controls.Add(btnSetDhcp);
            flowConfigButtons.Controls.Add(btnSetStatic);
            flowConfigButtons.Location = new Point(16, 128);
            flowConfigButtons.Margin = new Padding(0);
            flowConfigButtons.Name = "flowConfigButtons";
            flowConfigButtons.Size = new Size(147, 31);
            flowConfigButtons.TabIndex = 15;
            flowConfigButtons.WrapContents = false;
            // 
            // btnSetDhcp
            // 
            btnSetDhcp.AutoSize = true;
            btnSetDhcp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSetDhcp.Location = new Point(0, 0);
            btnSetDhcp.Margin = new Padding(0, 0, 8, 0);
            btnSetDhcp.Name = "btnSetDhcp";
            btnSetDhcp.Size = new Size(68, 25);
            btnSetDhcp.TabIndex = 0;
            btnSetDhcp.Text = "Set DHCP";
            // 
            // btnSetStatic
            // 
            btnSetStatic.AutoSize = true;
            btnSetStatic.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSetStatic.Location = new Point(79, 3);
            btnSetStatic.Name = "btnSetStatic";
            btnSetStatic.Size = new Size(65, 25);
            btnSetStatic.TabIndex = 1;
            btnSetStatic.Text = "Set Static";
            // 
            // TabNetworkAdapters
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(layoutRoot);
            Name = "TabNetworkAdapters";
            Size = new Size(1000, 650);
            layoutRoot.ResumeLayout(false);
            layoutRoot.PerformLayout();
            flowHeader.ResumeLayout(false);
            flowHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAdapters).EndInit();
            grpConfig.ResumeLayout(false);
            grpConfig.PerformLayout();
            flowConfigButtons.ResumeLayout(false);
            flowConfigButtons.PerformLayout();
            ResumeLayout(false);
        }
    }
}