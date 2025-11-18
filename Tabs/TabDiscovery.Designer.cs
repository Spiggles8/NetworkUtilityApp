using System.Drawing;
using System.Windows.Forms;

namespace NetworkUtilityApp.Tabs
{
    partial class TabDiscovery
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layoutRoot;
        private GroupBox grpRange;
        private ComboBox cboAdapter;
        private Label lblAdapter;
        private Label lblStartIp;
        private TextBox txtStartIp;
        private Label lblEndIp;
        private TextBox txtEndIp;
        private Button btnAutofill;
        private Button btnScan;
        private Button btnCancel;
        private Button btnSave;
        private ProgressBar prgScan;              // NEW
        private Label lblProgressCounts;          // NEW
        private Label lblEta;                     // NEW
        private DataGridView dgvResults;
        private DataGridViewTextBoxColumn colIp;
        private DataGridViewTextBoxColumn colHost;
        private DataGridViewTextBoxColumn colMac;
        private DataGridViewTextBoxColumn colManufacturer;
        private DataGridViewTextBoxColumn colLatency;
        private DataGridViewTextBoxColumn colStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            layoutRoot = new TableLayoutPanel();
            grpRange = new GroupBox();
            cboAdapter = new ComboBox();
            lblAdapter = new Label();
            lblStartIp = new Label();
            txtStartIp = new TextBox();
            lblEndIp = new Label();
            txtEndIp = new TextBox();
            btnAutofill = new Button();
            btnScan = new Button();
            btnCancel = new Button();
            btnSave = new Button();
            prgScan = new ProgressBar();           // NEW
            lblProgressCounts = new Label();       // NEW
            lblEta = new Label();                  // NEW
            dgvResults = new DataGridView();
            colIp = new DataGridViewTextBoxColumn();
            colHost = new DataGridViewTextBoxColumn();
            colMac = new DataGridViewTextBoxColumn();
            colManufacturer = new DataGridViewTextBoxColumn();
            colLatency = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            layoutRoot.SuspendLayout();
            grpRange.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            SuspendLayout();
            // layoutRoot
            layoutRoot.ColumnCount = 1;
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutRoot.RowCount = 2;
            layoutRoot.RowStyles.Add(new RowStyle());
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutRoot.Dock = DockStyle.Fill;
            layoutRoot.Padding = new Padding(8);
            layoutRoot.Controls.Add(grpRange, 0, 0);
            layoutRoot.Controls.Add(dgvResults, 0, 1);
            // grpRange
            grpRange.AutoSize = true;
            grpRange.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpRange.Padding = new Padding(12);
            grpRange.Text = "IP Range Scan";
            grpRange.Controls.Add(lblAdapter);
            grpRange.Controls.Add(cboAdapter);
            grpRange.Controls.Add(lblStartIp);
            grpRange.Controls.Add(txtStartIp);
            grpRange.Controls.Add(lblEndIp);
            grpRange.Controls.Add(txtEndIp);
            grpRange.Controls.Add(btnAutofill);
            grpRange.Controls.Add(btnScan);
            grpRange.Controls.Add(btnCancel);
            grpRange.Controls.Add(btnSave);
            grpRange.Controls.Add(prgScan);          // NEW
            grpRange.Controls.Add(lblProgressCounts);// NEW
            grpRange.Controls.Add(lblEta);           // NEW
            // lblAdapter
            lblAdapter.AutoSize = true;
            lblAdapter.Location = new Point(16, 32);
            lblAdapter.Text = "Adapter:";
            // cboAdapter
            cboAdapter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAdapter.Location = new Point(120, 28);
            cboAdapter.Width = 220;
            // lblStartIp
            lblStartIp.AutoSize = true;
            lblStartIp.Location = new Point(16, 64);
            lblStartIp.Text = "Start IP:";
            // txtStartIp
            txtStartIp.Location = new Point(120, 60);
            txtStartIp.Width = 150;
            // lblEndIp
            lblEndIp.AutoSize = true;
            lblEndIp.Location = new Point(16, 96);
            lblEndIp.Text = "End IP:";
            // txtEndIp
            txtEndIp.Location = new Point(120, 92);
            txtEndIp.Width = 150;
            // btnAutofill
            btnAutofill.AutoSize = true;
            btnAutofill.Location = new Point(290, 60);
            btnAutofill.Text = "Autofill Range";
            // btnScan
            btnScan.AutoSize = true;
            btnScan.Location = new Point(290, 92);
            btnScan.Text = "Scan Now";
            // btnCancel
            btnCancel.AutoSize = true;
            btnCancel.Location = new Point(390, 92);
            btnCancel.Text = "Cancel";
            // btnSave
            btnSave.AutoSize = true;
            btnSave.Location = new Point(470, 92);
            btnSave.Name = "btnSave";
            btnSave.Text = "Save Results...";
            // prgScan
            prgScan.Location = new Point(16, 130);
            prgScan.Name = "prgScan";
            prgScan.Size = new Size(420, 18);
            prgScan.Minimum = 0;
            // lblProgressCounts
            lblProgressCounts.AutoSize = true;
            lblProgressCounts.Location = new Point(16, 152);
            lblProgressCounts.Name = "lblProgressCounts";
            lblProgressCounts.Text = "Scanned: 0 / 0 | Active: 0";
            // lblEta
            lblEta.AutoSize = true;
            lblEta.Location = new Point(280, 152);
            lblEta.Name = "lblEta";
            lblEta.Text = "ETA: --:--:--";
            // dgvResults
            dgvResults.Dock = DockStyle.Fill;
            dgvResults.AllowUserToAddRows = false;
            dgvResults.AllowUserToDeleteRows = false;
            dgvResults.ReadOnly = true;
            dgvResults.RowHeadersVisible = false;
            dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResults.Columns.AddRange(new DataGridViewColumn[]
            {
                colIp, colHost, colMac, colManufacturer, colLatency, colStatus
            });
            // Columns
            colIp.HeaderText = "IP Address"; colIp.Width = 140;
            colHost.HeaderText = "Host Name"; colHost.Width = 160;
            colMac.HeaderText = "MAC Address"; colMac.Width = 140;
            colManufacturer.HeaderText = "Manufacturer"; colManufacturer.Width = 160;
            colLatency.HeaderText = "Latency (ms)"; colLatency.Width = 110;
            colStatus.HeaderText = "Status"; colStatus.Width = 100;
            // TabDiscovery
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(layoutRoot);
            Name = "TabDiscovery";
            Size = new Size(800, 500);
            layoutRoot.ResumeLayout(false);
            layoutRoot.PerformLayout();
            grpRange.ResumeLayout(false);
            grpRange.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            ResumeLayout(false);
        }
    }
}