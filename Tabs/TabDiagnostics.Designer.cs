using System.Windows.Forms;
using MaterialSkin.Controls;

namespace NetworkUtilityApp.Tabs
{
    partial class TabDiagnostics
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel layoutRoot;

        // Target host shared by Ping & Traceroute
        private GroupBox grpTarget;
        private MaterialLabel lblTarget;
        private MaterialTextBox2 txtDiagTarget;

        // Ping group
        private GroupBox grpPing;
        private MaterialButton btnPingOnce;
        private MaterialButton btnPingStart;
        private MaterialButton btnPingStop;
        private MaterialLabel lblPingInterval;
        private NumericUpDown numPingIntervalMs;
        private MaterialLabel lblPingTimeout;
        private NumericUpDown numPingTimeoutMs;
        private MaterialLabel lblPingSize;
        private NumericUpDown numPingSize;
        private MaterialLabel lblPingTtl;
        private NumericUpDown numPingTtl;
        private MaterialCheckbox chkPingDontFragment;

        // Traceroute group
        private GroupBox grpTrace;
        private MaterialButton btnTraceroute;
        private MaterialButton btnTracerouteStop;
        private MaterialCheckbox chkTraceResolve;
        private MaterialLabel lblTraceHops;
        private NumericUpDown numTraceMaxHops;
        private MaterialLabel lblTraceTimeout;
        private NumericUpDown numTracePerHopTimeoutMs;

        // TCP test group
        private GroupBox grpTcp;
        private MaterialLabel lblTcpHost;
        private MaterialTextBox2 txtTcpHost;
        private MaterialLabel lblTcpPort;
        private NumericUpDown numTcpPort;
        private MaterialLabel lblTcpTimeout;
        private NumericUpDown numTcpTimeoutMs;
        private MaterialButton btnTcpTest;

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

            grpTarget = new GroupBox();
            lblTarget = new MaterialLabel();
            txtDiagTarget = new MaterialTextBox2();

            grpPing = new GroupBox();
            btnPingOnce = new MaterialButton();
            btnPingStart = new MaterialButton();
            btnPingStop = new MaterialButton();
            lblPingInterval = new MaterialLabel();
            numPingIntervalMs = new NumericUpDown();
            lblPingTimeout = new MaterialLabel();
            numPingTimeoutMs = new NumericUpDown();
            lblPingSize = new MaterialLabel();
            numPingSize = new NumericUpDown();
            lblPingTtl = new MaterialLabel();
            numPingTtl = new NumericUpDown();
            chkPingDontFragment = new MaterialCheckbox();

            grpTrace = new GroupBox();
            btnTraceroute = new MaterialButton();
            btnTracerouteStop = new MaterialButton();
            chkTraceResolve = new MaterialCheckbox();
            lblTraceHops = new MaterialLabel();
            numTraceMaxHops = new NumericUpDown();
            lblTraceTimeout = new MaterialLabel();
            numTracePerHopTimeoutMs = new NumericUpDown();

            grpTcp = new GroupBox();
            lblTcpHost = new MaterialLabel();
            txtTcpHost = new MaterialTextBox2();
            lblTcpPort = new MaterialLabel();
            numTcpPort = new NumericUpDown();
            lblTcpTimeout = new MaterialLabel();
            numTcpTimeoutMs = new NumericUpDown();
            btnTcpTest = new MaterialButton();

            layoutRoot.SuspendLayout();
            grpTarget.SuspendLayout();
            grpPing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(numPingIntervalMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(numPingTimeoutMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(numPingSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(numPingTtl)).BeginInit();
            grpTrace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(numTraceMaxHops)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(numTracePerHopTimeoutMs)).BeginInit();
            grpTcp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(numTcpPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(numTcpTimeoutMs)).BeginInit();
            SuspendLayout();

            // layoutRoot
            layoutRoot.ColumnCount = 1;
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutRoot.RowCount = 4;
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // target
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // ping
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // traceroute
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // tcp
            layoutRoot.Dock = DockStyle.Fill;
            layoutRoot.Padding = new Padding(12);
            layoutRoot.Name = "layoutRoot";
            layoutRoot.Size = new System.Drawing.Size(900, 600);

            // =======================
            // grpTarget
            // =======================
            grpTarget.Text = "Target Host / IP";
            grpTarget.Padding = new Padding(12);
            grpTarget.Margin = new Padding(0, 0, 0, 12);
            grpTarget.Dock = DockStyle.Top;
            grpTarget.Size = new System.Drawing.Size(876, 82);

            // lblTarget
            lblTarget.Text = "Host/IP";
            lblTarget.Location = new System.Drawing.Point(16, 32);
            lblTarget.AutoSize = true;

            // txtDiagTarget
            txtDiagTarget.Name = "txtDiagTarget";
            txtDiagTarget.Location = new System.Drawing.Point(80, 24);
            txtDiagTarget.Size = new System.Drawing.Size(300, 36);
            txtDiagTarget.Hint = "e.g. 8.8.8.8 or example.com";
            //txtDiagTarget.BorderStyle = BorderStyle.None;

            grpTarget.Controls.Add(lblTarget);
            grpTarget.Controls.Add(txtDiagTarget);

            // =======================
            // grpPing
            // =======================
            grpPing.Text = "Ping";
            grpPing.Padding = new Padding(12);
            grpPing.Margin = new Padding(0, 0, 0, 12);
            grpPing.Dock = DockStyle.Top;
            grpPing.Size = new System.Drawing.Size(876, 136);

            // Buttons
            btnPingOnce.Text = "Ping Once";
            btnPingOnce.Name = "btnPingOnce";
            btnPingOnce.Type = MaterialButton.MaterialButtonType.Contained;
            btnPingOnce.HighEmphasis = true;
            btnPingOnce.Size = new System.Drawing.Size(110, 36);
            btnPingOnce.Location = new System.Drawing.Point(16, 28);

            btnPingStart.Text = "Start";
            btnPingStart.Name = "btnPingStart";
            btnPingStart.Type = MaterialButton.MaterialButtonType.Outlined;
            btnPingStart.Size = new System.Drawing.Size(80, 36);
            btnPingStart.Location = new System.Drawing.Point(136, 28);

            btnPingStop.Text = "Stop";
            btnPingStop.Name = "btnPingStop";
            btnPingStop.Type = MaterialButton.MaterialButtonType.Outlined;
            btnPingStop.Size = new System.Drawing.Size(80, 36);
            btnPingStop.Location = new System.Drawing.Point(224, 28);

            // Interval
            lblPingInterval.Text = "Interval (ms)";
            lblPingInterval.AutoSize = true;
            lblPingInterval.Location = new System.Drawing.Point(16, 84);

            numPingIntervalMs.Name = "numPingIntervalMs";
            numPingIntervalMs.Minimum = 100;
            numPingIntervalMs.Maximum = 60000;
            numPingIntervalMs.Value = 1000;
            numPingIntervalMs.Size = new System.Drawing.Size(80, 23);
            numPingIntervalMs.Location = new System.Drawing.Point(100, 80);

            // Timeout
            lblPingTimeout.Text = "Timeout (ms)";
            lblPingTimeout.AutoSize = true;
            lblPingTimeout.Location = new System.Drawing.Point(196, 84);

            numPingTimeoutMs.Name = "numPingTimeoutMs";
            numPingTimeoutMs.Minimum = 100;
            numPingTimeoutMs.Maximum = 60000;
            numPingTimeoutMs.Value = 2000;
            numPingTimeoutMs.Size = new System.Drawing.Size(80, 23);
            numPingTimeoutMs.Location = new System.Drawing.Point(284, 80);

            // Size
            lblPingSize.Text = "Size (bytes)";
            lblPingSize.AutoSize = true;
            lblPingSize.Location = new System.Drawing.Point(380, 84);

            numPingSize.Name = "numPingSize";
            numPingSize.Minimum = 1;
            numPingSize.Maximum = 65500;
            numPingSize.Value = 32;
            numPingSize.Size = new System.Drawing.Size(80, 23);
            numPingSize.Location = new System.Drawing.Point(464, 80);

            // TTL
            lblPingTtl.Text = "TTL";
            lblPingTtl.AutoSize = true;
            lblPingTtl.Location = new System.Drawing.Point(560, 84);

            numPingTtl.Name = "numPingTtl";
            numPingTtl.Minimum = 1;
            numPingTtl.Maximum = 255;
            numPingTtl.Value = 128;
            numPingTtl.Size = new System.Drawing.Size(60, 23);
            numPingTtl.Location = new System.Drawing.Point(592, 80);

            // Don't Fragment
            chkPingDontFragment.Text = "Don't Fragment";
            chkPingDontFragment.Name = "chkPingDontFragment";
            chkPingDontFragment.Location = new System.Drawing.Point(668, 76);
            chkPingDontFragment.AutoSize = true;

            grpPing.Controls.Add(btnPingOnce);
            grpPing.Controls.Add(btnPingStart);
            grpPing.Controls.Add(btnPingStop);
            grpPing.Controls.Add(lblPingInterval);
            grpPing.Controls.Add(numPingIntervalMs);
            grpPing.Controls.Add(lblPingTimeout);
            grpPing.Controls.Add(numPingTimeoutMs);
            grpPing.Controls.Add(lblPingSize);
            grpPing.Controls.Add(numPingSize);
            grpPing.Controls.Add(lblPingTtl);
            grpPing.Controls.Add(numPingTtl);
            grpPing.Controls.Add(chkPingDontFragment);

            // =======================
            // grpTrace
            // =======================
            grpTrace.Text = "Traceroute";
            grpTrace.Padding = new Padding(12);
            grpTrace.Margin = new Padding(0, 0, 0, 12);
            grpTrace.Dock = DockStyle.Top;
            grpTrace.Size = new System.Drawing.Size(876, 120);

            btnTraceroute.Text = "Run Traceroute";
            btnTraceroute.Name = "btnTraceroute";
            btnTraceroute.Type = MaterialButton.MaterialButtonType.Contained;
            btnTraceroute.Size = new System.Drawing.Size(150, 36);
            btnTraceroute.Location = new System.Drawing.Point(16, 28);

            btnTracerouteStop.Text = "Stop";
            btnTracerouteStop.Name = "btnTracerouteStop";
            btnTracerouteStop.Type = MaterialButton.MaterialButtonType.Outlined;
            btnTracerouteStop.Size = new System.Drawing.Size(80, 36);
            btnTracerouteStop.Location = new System.Drawing.Point(176, 28);

            chkTraceResolve.Text = "Resolve hostnames";
            chkTraceResolve.Name = "chkTraceResolve";
            chkTraceResolve.Location = new System.Drawing.Point(276, 32);
            chkTraceResolve.AutoSize = true;

            lblTraceHops.Text = "Max hops";
            lblTraceHops.AutoSize = true;
            lblTraceHops.Location = new System.Drawing.Point(16, 80);

            numTraceMaxHops.Name = "numTraceMaxHops";
            numTraceMaxHops.Minimum = 1;
            numTraceMaxHops.Maximum = 64;
            numTraceMaxHops.Value = 30;
            numTraceMaxHops.Size = new System.Drawing.Size(70, 23);
            numTraceMaxHops.Location = new System.Drawing.Point(84, 76);

            lblTraceTimeout.Text = "Per-hop timeout (ms)";
            lblTraceTimeout.AutoSize = true;
            lblTraceTimeout.Location = new System.Drawing.Point(170, 80);

            numTracePerHopTimeoutMs.Name = "numTracePerHopTimeoutMs";
            numTracePerHopTimeoutMs.Minimum = 100;
            numTracePerHopTimeoutMs.Maximum = 60000;
            numTracePerHopTimeoutMs.Value = 2000;
            numTracePerHopTimeoutMs.Size = new System.Drawing.Size(90, 23);
            numTracePerHopTimeoutMs.Location = new System.Drawing.Point(312, 76);

            grpTrace.Controls.Add(btnTraceroute);
            grpTrace.Controls.Add(btnTracerouteStop);
            grpTrace.Controls.Add(chkTraceResolve);
            grpTrace.Controls.Add(lblTraceHops);
            grpTrace.Controls.Add(numTraceMaxHops);
            grpTrace.Controls.Add(lblTraceTimeout);
            grpTrace.Controls.Add(numTracePerHopTimeoutMs);

            // =======================
            // grpTcp
            // =======================
            grpTcp.Text = "TCP Port Test";
            grpTcp.Padding = new Padding(12);
            grpTcp.Margin = new Padding(0, 0, 0, 12);
            grpTcp.Dock = DockStyle.Top;
            grpTcp.Size = new System.Drawing.Size(876, 108);

            lblTcpHost.Text = "Host/IP";
            lblTcpHost.AutoSize = true;
            lblTcpHost.Location = new System.Drawing.Point(16, 32);

            txtTcpHost.Name = "txtTcpHost";
            txtTcpHost.Location = new System.Drawing.Point(72, 24);
            txtTcpHost.Size = new System.Drawing.Size(220, 36);
            txtTcpHost.Hint = "(optional, uses Target if empty)";
            //txtTcpHost.BorderStyle = BorderStyle.None;

            lblTcpPort.Text = "Port";
            lblTcpPort.AutoSize = true;
            lblTcpPort.Location = new System.Drawing.Point(308, 32);

            numTcpPort.Name = "numTcpPort";
            numTcpPort.Minimum = 1;
            numTcpPort.Maximum = 65535;
            numTcpPort.Value = 80;
            numTcpPort.Size = new System.Drawing.Size(80, 23);
            numTcpPort.Location = new System.Drawing.Point(344, 28);

            lblTcpTimeout.Text = "Timeout (ms)";
            lblTcpTimeout.AutoSize = true;
            lblTcpTimeout.Location = new System.Drawing.Point(440, 32);

            numTcpTimeoutMs.Name = "numTcpTimeoutMs";
            numTcpTimeoutMs.Minimum = 100;
            numTcpTimeoutMs.Maximum = 60000;
            numTcpTimeoutMs.Value = 1500;
            numTcpTimeoutMs.Size = new System.Drawing.Size(90, 23);
            numTcpTimeoutMs.Location = new System.Drawing.Point(528, 28);

            btnTcpTest.Text = "Test Port";
            btnTcpTest.Name = "btnTcpTest";
            btnTcpTest.Type = MaterialButton.MaterialButtonType.Outlined;
            btnTcpTest.Size = new System.Drawing.Size(100, 36);
            btnTcpTest.Location = new System.Drawing.Point(636, 24);

            grpTcp.Controls.Add(lblTcpHost);
            grpTcp.Controls.Add(txtTcpHost);
            grpTcp.Controls.Add(lblTcpPort);
            grpTcp.Controls.Add(numTcpPort);
            grpTcp.Controls.Add(lblTcpTimeout);
            grpTcp.Controls.Add(numTcpTimeoutMs);
            grpTcp.Controls.Add(btnTcpTest);

            // Add groups to layout
            layoutRoot.Controls.Add(grpTarget, 0, 0);
            layoutRoot.Controls.Add(grpPing, 0, 1);
            layoutRoot.Controls.Add(grpTrace, 0, 2);
            layoutRoot.Controls.Add(grpTcp, 0, 3);

            // TabDiagnostics (UserControl)
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(layoutRoot);
            Name = "TabDiagnostics";
            Size = new System.Drawing.Size(900, 600);

            layoutRoot.ResumeLayout(false);
            grpTarget.ResumeLayout(false);
            grpTarget.PerformLayout();
            grpPing.ResumeLayout(false);
            grpPing.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(numPingIntervalMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(numPingTimeoutMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(numPingSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(numPingTtl)).EndInit();
            grpTrace.ResumeLayout(false);
            grpTrace.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(numTraceMaxHops)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(numTracePerHopTimeoutMs)).EndInit();
            grpTcp.ResumeLayout(false);
            grpTcp.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(numTcpPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(numTcpTimeoutMs)).EndInit();
            ResumeLayout(false);
        }
    }
}
