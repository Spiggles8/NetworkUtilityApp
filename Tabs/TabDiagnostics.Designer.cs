using System.Drawing;
using System.Windows.Forms;

namespace NetworkUtilityApp.Tabs
{
    partial class TabDiagnostics
    {
        private System.ComponentModel.IContainer components = null;

        private FlowLayoutPanel flowRoot;

        private GroupBox grpPing;
        private Label lblPingTarget;
        private TextBox txtPingTarget;
        private Button btnPing;
        private CheckBox chkPingContinuous; // NEW

        private GroupBox grpTrace;
        private Label lblTraceTarget;
        private TextBox txtTraceTarget;
        private CheckBox chkResolveNames;
        private Button btnTrace;

        private GroupBox grpNslookup;
        private Label lblNslookupTarget;
        private TextBox txtNslookupTarget;
        private Button btnNslookup;

        private GroupBox grpPathPing;
        private Label lblPathPingTarget;
        private TextBox txtPathPingTarget;
        private Button btnPathPing;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            flowRoot = new FlowLayoutPanel();
            grpPing = new GroupBox();
            lblPingTarget = new Label();
            txtPingTarget = new TextBox();
            btnPing = new Button();
            chkPingContinuous = new CheckBox(); // NEW

            grpTrace = new GroupBox();
            lblTraceTarget = new Label();
            txtTraceTarget = new TextBox();
            chkResolveNames = new CheckBox();
            btnTrace = new Button();

            grpNslookup = new GroupBox();
            lblNslookupTarget = new Label();
            txtNslookupTarget = new TextBox();
            btnNslookup = new Button();

            grpPathPing = new GroupBox();
            lblPathPingTarget = new Label();
            txtPathPingTarget = new TextBox();
            btnPathPing = new Button();

            SuspendLayout();

            // flowRoot
            flowRoot.Dock = DockStyle.Fill;
            flowRoot.FlowDirection = FlowDirection.TopDown;
            flowRoot.WrapContents = false;
            flowRoot.AutoScroll = true;
            flowRoot.Padding = new Padding(8);
            flowRoot.Name = "flowRoot";
            flowRoot.TabIndex = 0;

            // grpPing
            grpPing.AutoSize = true;
            grpPing.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpPing.Padding = new Padding(12);
            grpPing.Text = "Ping";
            grpPing.Margin = new Padding(0, 0, 0, 8);

            lblPingTarget.AutoSize = true;
            lblPingTarget.Location = new Point(16, 32);
            lblPingTarget.Text = "Target (IP or host):";

            txtPingTarget.Location = new Point(140, 28);
            txtPingTarget.Size = new Size(220, 23);
            txtPingTarget.Text = "8.8.8.8";

            btnPing.AutoSize = true;
            btnPing.Location = new Point(370, 27);
            btnPing.Text = "Ping";

            // NEW: chkPingContinuous
            chkPingContinuous.AutoSize = true;
            chkPingContinuous.Location = new Point(460, 30);
            chkPingContinuous.Name = "chkPingContinuous";
            chkPingContinuous.Text = "Ping continuously";

            grpPing.Controls.Add(lblPingTarget);
            grpPing.Controls.Add(txtPingTarget);
            grpPing.Controls.Add(btnPing);
            grpPing.Controls.Add(chkPingContinuous); // NEW

            // grpTrace (Traceroute)
            grpTrace.AutoSize = true;
            grpTrace.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpTrace.Padding = new Padding(12);
            grpTrace.Text = "Traceroute";
            grpTrace.Margin = new Padding(0, 0, 0, 8);

            lblTraceTarget.AutoSize = true;
            lblTraceTarget.Location = new Point(16, 32);
            lblTraceTarget.Text = "Target (IP or host):";

            txtTraceTarget.Location = new Point(140, 28);
            txtTraceTarget.Size = new Size(220, 23);
            txtTraceTarget.Text = "8.8.8.8";

            chkResolveNames.AutoSize = true;
            chkResolveNames.Checked = true;
            chkResolveNames.Location = new Point(370, 30);
            chkResolveNames.Text = "Resolve names";

            btnTrace.AutoSize = true;
            btnTrace.Location = new Point(500, 27);
            btnTrace.Text = "Run Traceroute";

            grpTrace.Controls.Add(lblTraceTarget);
            grpTrace.Controls.Add(txtTraceTarget);
            grpTrace.Controls.Add(chkResolveNames);
            grpTrace.Controls.Add(btnTrace);

            // grpNslookup
            grpNslookup.AutoSize = true;
            grpNslookup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpNslookup.Padding = new Padding(12);
            grpNslookup.Text = "DNS (nslookup)";
            grpNslookup.Margin = new Padding(0, 0, 0, 8);

            lblNslookupTarget.AutoSize = true;
            lblNslookupTarget.Location = new Point(16, 32);
            lblNslookupTarget.Text = "Target (IP or host):";

            txtNslookupTarget.Location = new Point(140, 28);
            txtNslookupTarget.Size = new Size(220, 23);
            txtNslookupTarget.Text = "www.microsoft.com";

            btnNslookup.AutoSize = true;
            btnNslookup.Location = new Point(370, 27);
            btnNslookup.Text = "nslookup";

            grpNslookup.Controls.Add(lblNslookupTarget);
            grpNslookup.Controls.Add(txtNslookupTarget);
            grpNslookup.Controls.Add(btnNslookup);

            // grpPathPing
            grpPathPing.AutoSize = true;
            grpPathPing.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpPathPing.Padding = new Padding(12);
            grpPathPing.Text = "PathPing";
            grpPathPing.Margin = new Padding(0, 0, 0, 8);

            lblPathPingTarget.AutoSize = true;
            lblPathPingTarget.Location = new Point(16, 32);
            lblPathPingTarget.Text = "Target (IP or host):";

            txtPathPingTarget.Location = new Point(140, 28);
            txtPathPingTarget.Size = new Size(220, 23);

            btnPathPing.AutoSize = true;
            btnPathPing.Location = new Point(370, 27);
            btnPathPing.Text = "pathping";

            grpPathPing.Controls.Add(lblPathPingTarget);
            grpPathPing.Controls.Add(txtPathPingTarget);
            grpPathPing.Controls.Add(btnPathPing);

            // add groups to flow
            flowRoot.Controls.Add(grpPing);
            flowRoot.Controls.Add(grpTrace);
            flowRoot.Controls.Add(grpNslookup);
            flowRoot.Controls.Add(grpPathPing);

            // TabDiagnostics
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(flowRoot);
            Name = "TabDiagnostics";
            Size = new Size(800, 500);

            ResumeLayout(false);
        }
    }
}