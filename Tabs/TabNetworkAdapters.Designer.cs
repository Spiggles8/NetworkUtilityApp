using System.Windows.Forms;

namespace NetworkUtilityApp.Tabs
{
    partial class TabNetworkAdapters
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel layoutRoot;

        // Top actions + selected label
        private FlowLayoutPanel flowHeader;
        private Button btnRefresh;
        private Label lblSelectedAdapter;

        // Grid
        private DataGridView dgvAdapters;
        private DataGridViewTextBoxColumn colAdapterName;
        private DataGridViewTextBoxColumn colDhcp;
        private DataGridViewTextBoxColumn colIpAddress;
        private DataGridViewTextBoxColumn colSubnet;
        private DataGridViewTextBoxColumn colGateway;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colHardwareDetails;
        private DataGridViewTextBoxColumn colMacAddress;

        // Static config group
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

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
            ((System.ComponentModel.ISupportInitialize)(dgvAdapters)).BeginInit();
            grpConfig.SuspendLayout();
            flowConfigButtons.SuspendLayout();
            SuspendLayout();

            // layoutRoot
            layoutRoot.ColumnCount = 1;
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutRoot.RowCount = 3;
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // header
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // grid
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // config group
            layoutRoot.Dock = DockStyle.Fill;
            layoutRoot.Padding = new Padding(12);
            layoutRoot.Name = "layoutRoot";
            layoutRoot.Size = new System.Drawing.Size(1000, 650);

            // flowHeader
            flowHeader.AutoSize = true;
            flowHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowHeader.Dock = DockStyle.Top;
            flowHeader.FlowDirection = FlowDirection.LeftToRight;
            flowHeader.WrapContents = false;
            flowHeader.Padding = new Padding(0);
            flowHeader.Margin = new Padding(0, 0, 0, 8);
            flowHeader.Name = "flowHeader";
            flowHeader.Size = new System.Drawing.Size(976, 35);

            // btnRefresh
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Text = "Refresh Adapters";
            btnRefresh.AutoSize = true;
            btnRefresh.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRefresh.Margin = new Padding(0, 0, 12, 0);

            // lblSelectedAdapter
            lblSelectedAdapter.Name = "lblSelectedAdapter";
            lblSelectedAdapter.Text = "Selected Adapter: None";
            lblSelectedAdapter.AutoSize = true;
            lblSelectedAdapter.Margin = new Padding(0, 6, 0, 0);

            flowHeader.Controls.Add(btnRefresh);
            flowHeader.Controls.Add(lblSelectedAdapter);

            // dgvAdapters
            dgvAdapters.Name = "dgvAdapters";
            dgvAdapters.Dock = DockStyle.Fill;
            dgvAdapters.AllowUserToAddRows = false;
            dgvAdapters.AllowUserToDeleteRows = false;
            dgvAdapters.MultiSelect = false;
            dgvAdapters.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAdapters.ReadOnly = true;
            dgvAdapters.RowHeadersVisible = false;
            dgvAdapters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            colAdapterName.HeaderText = "Adapter";
            colAdapterName.Name = "colAdapterName";
            colAdapterName.ReadOnly = true;
            colAdapterName.MinimumWidth = 120;

            colDhcp.HeaderText = "DHCP";
            colDhcp.Name = "colDhcp";
            colDhcp.ReadOnly = true;
            colDhcp.MinimumWidth = 80;

            colIpAddress.HeaderText = "IP Address";
            colIpAddress.Name = "colIpAddress";
            colIpAddress.ReadOnly = true;
            colIpAddress.MinimumWidth = 120;

            colSubnet.HeaderText = "Subnet";
            colSubnet.Name = "colSubnet";
            colSubnet.ReadOnly = true;
            colSubnet.MinimumWidth = 120;

            colGateway.HeaderText = "Gateway";
            colGateway.Name = "colGateway";
            colGateway.ReadOnly = true;
            colGateway.MinimumWidth = 120;

            colStatus.HeaderText = "Status";
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            colStatus.MinimumWidth = 100;

            colHardwareDetails.HeaderText = "Hardware Details";
            colHardwareDetails.Name = "colHardwareDetails";
            colHardwareDetails.ReadOnly = true;
            colHardwareDetails.MinimumWidth = 180;

            colMacAddress.HeaderText = "MAC Address";
            colMacAddress.Name = "colMacAddress";
            colMacAddress.ReadOnly = true;
            colMacAddress.MinimumWidth = 140;

            dgvAdapters.Columns.AddRange(new DataGridViewColumn[]
            {
                colAdapterName, colDhcp, colIpAddress, colSubnet, colGateway, colStatus, colHardwareDetails, colMacAddress
            });

            // grpConfig
            grpConfig.Name = "grpConfig";
            grpConfig.Text = "Set Network Configuration";
            grpConfig.Dock = DockStyle.Top;
            grpConfig.Padding = new Padding(12);
            grpConfig.Margin = new Padding(0, 8, 0, 0);
            grpConfig.AutoSize = true;
            grpConfig.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // Labels
            lblIP.AutoSize = true;
            lblIP.Name = "lblIP";
            lblIP.Text = "IP Address:";
            lblIP.Location = new System.Drawing.Point(16, 32);

            lblSubnet.AutoSize = true;
            lblSubnet.Name = "lblSubnet";
            lblSubnet.Text = "Subnet Mask:";
            lblSubnet.Location = new System.Drawing.Point(16, 64);

            lblGateway.AutoSize = true;
            lblGateway.Name = "lblGateway";
            lblGateway.Text = "Gateway:";
            lblGateway.Location = new System.Drawing.Point(16, 96);

            // IP Octets
            txtIP1.Name = "txtIP1"; txtIP1.Size = new System.Drawing.Size(44, 23); txtIP1.Location = new System.Drawing.Point(120, 28);
            txtIP2.Name = "txtIP2"; txtIP2.Size = new System.Drawing.Size(44, 23); txtIP2.Location = new System.Drawing.Point(170, 28);
            txtIP3.Name = "txtIP3"; txtIP3.Size = new System.Drawing.Size(44, 23); txtIP3.Location = new System.Drawing.Point(220, 28);
            txtIP4.Name = "txtIP4"; txtIP4.Size = new System.Drawing.Size(44, 23); txtIP4.Location = new System.Drawing.Point(270, 28);

            // Subnet Octets
            txtSubnet1.Name = "txtSubnet1"; txtSubnet1.Size = new System.Drawing.Size(44, 23); txtSubnet1.Location = new System.Drawing.Point(120, 60);
            txtSubnet2.Name = "txtSubnet2"; txtSubnet2.Size = new System.Drawing.Size(44, 23); txtSubnet2.Location = new System.Drawing.Point(170, 60);
            txtSubnet3.Name = "txtSubnet3"; txtSubnet3.Size = new System.Drawing.Size(44, 23); txtSubnet3.Location = new System.Drawing.Point(220, 60);
            txtSubnet4.Name = "txtSubnet4"; txtSubnet4.Size = new System.Drawing.Size(44, 23); txtSubnet4.Location = new System.Drawing.Point(270, 60);

            // Gateway Octets
            txtGateway1.Name = "txtGateway1"; txtGateway1.Size = new System.Drawing.Size(44, 23); txtGateway1.Location = new System.Drawing.Point(120, 92);
            txtGateway2.Name = "txtGateway2"; txtGateway2.Size = new System.Drawing.Size(44, 23); txtGateway2.Location = new System.Drawing.Point(170, 92);
            txtGateway3.Name = "txtGateway3"; txtGateway3.Size = new System.Drawing.Size(44, 23); txtGateway3.Location = new System.Drawing.Point(220, 92);
            txtGateway4.Name = "txtGateway4"; txtGateway4.Size = new System.Drawing.Size(44, 23); txtGateway4.Location = new System.Drawing.Point(270, 92);

            // Buttons panel
            flowConfigButtons.AutoSize = true;
            flowConfigButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowConfigButtons.FlowDirection = FlowDirection.LeftToRight;
            flowConfigButtons.Location = new System.Drawing.Point(16, 128);
            flowConfigButtons.Margin = new Padding(0);
            flowConfigButtons.Name = "flowConfigButtons";
            flowConfigButtons.Size = new System.Drawing.Size(300, 32);
            flowConfigButtons.WrapContents = false;

            // btnSetDhcp
            btnSetDhcp.Name = "btnSetDhcp";
            btnSetDhcp.Text = "Set DHCP";
            btnSetDhcp.AutoSize = true;
            btnSetDhcp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSetDhcp.Margin = new Padding(0, 0, 8, 0);

            // btnSetStatic
            btnSetStatic.Name = "btnSetStatic";
            btnSetStatic.Text = "Set Static";
            btnSetStatic.AutoSize = true;
            btnSetStatic.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            flowConfigButtons.Controls.Add(btnSetDhcp);
            flowConfigButtons.Controls.Add(btnSetStatic);

            // Add controls to grpConfig
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

            // Add to layoutRoot
            layoutRoot.Controls.Add(flowHeader, 0, 0);
            layoutRoot.Controls.Add(dgvAdapters, 0, 1);
            layoutRoot.Controls.Add(grpConfig, 0, 2);

            // TabNetworkAdapters (UserControl)
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(layoutRoot);
            Name = "TabNetworkAdapters";
            Size = new System.Drawing.Size(1000, 650);

            layoutRoot.ResumeLayout(false);
            layoutRoot.PerformLayout();
            flowHeader.ResumeLayout(false);
            flowHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(dgvAdapters)).EndInit();
            grpConfig.ResumeLayout(false);
            grpConfig.PerformLayout();
            flowConfigButtons.ResumeLayout(false);
            flowConfigButtons.PerformLayout();
            ResumeLayout(false);
        }
    }
}
