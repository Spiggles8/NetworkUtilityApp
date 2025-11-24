using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace NetworkUtilityApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private WebView2 webView;          // main HTML UI
        private Panel pnlGlobalLogInner;   // log panel
        private Label lblGlobalLog;
        private TextBox txtGlobalLog;
        private FlowLayoutPanel flowGlobalLogButtons;
        private Button btnGlobalLogClear;
        private Button btnGlobalLogSave;
        private SplitContainer splitMain;  // new split container hosting web view and log

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                webView?.Dispose();
                splitMain?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            webView = new WebView2();
            pnlGlobalLogInner = new Panel();
            lblGlobalLog = new Label();
            txtGlobalLog = new TextBox();
            flowGlobalLogButtons = new FlowLayoutPanel();
            btnGlobalLogClear = new Button();
            btnGlobalLogSave = new Button();
            splitMain = new SplitContainer();
            pnlGlobalLogInner.SuspendLayout();
            flowGlobalLogButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitMain)).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            SuspendLayout();
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.Orientation = Orientation.Horizontal;
            splitMain.Name = "splitMain";
            splitMain.SplitterWidth = 1; // minimal visual bar
            splitMain.IsSplitterFixed = true;
            splitMain.BackColor = Color.White; // unify background
            splitMain.Panel1MinSize = 300;
            splitMain.Panel2MinSize = 140;
            splitMain.SplitterDistance = 650; // will adjust based on form size
            // 
            // webView (placed in Panel1)
            // 
            webView.CreationProperties = null;
            webView.DefaultBackgroundColor = Color.White;
            webView.Dock = DockStyle.Fill;
            webView.Location = new Point(0, 0);
            webView.Name = "webView";
            webView.ZoomFactor = 1D;
            splitMain.Panel1.Controls.Add(webView);
            // 
            // pnlGlobalLogInner (placed in Panel2, fills)
            // 
            pnlGlobalLogInner.Dock = DockStyle.Fill;
            pnlGlobalLogInner.Padding = new Padding(10, 0, 10, 10);
            pnlGlobalLogInner.Name = "pnlGlobalLogInner";
            pnlGlobalLogInner.BackColor = Color.White;
            pnlGlobalLogInner.Controls.Add(txtGlobalLog);
            pnlGlobalLogInner.Controls.Add(flowGlobalLogButtons);
            pnlGlobalLogInner.Controls.Add(lblGlobalLog);
            splitMain.Panel2.Controls.Add(pnlGlobalLogInner);
            // 
            // lblGlobalLog
            // 
            lblGlobalLog.Dock = DockStyle.Top;
            lblGlobalLog.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblGlobalLog.Text = "Output Log";
            lblGlobalLog.Height = 32;
            lblGlobalLog.TextAlign = ContentAlignment.MiddleLeft;
            lblGlobalLog.Padding = new Padding(0, 4, 0, 0);
            lblGlobalLog.BorderStyle = BorderStyle.None;
            // underline effect panel
            var underline = new Panel { Dock = DockStyle.Top, Height = 2, BackColor = Color.FromArgb(224,224,224) };
            pnlGlobalLogInner.Controls.Add(underline);
            pnlGlobalLogInner.Controls.SetChildIndex(underline, 1); // place right under label
            // 
            // txtGlobalLog
            // 
            txtGlobalLog.Dock = DockStyle.Fill;
            txtGlobalLog.Font = new Font("Consolas", 9F);
            txtGlobalLog.Multiline = true;
            txtGlobalLog.ReadOnly = true;
            txtGlobalLog.ScrollBars = ScrollBars.Vertical;
            txtGlobalLog.BorderStyle = BorderStyle.FixedSingle;
            txtGlobalLog.Name = "txtGlobalLog";
            // 
            // flowGlobalLogButtons
            // 
            flowGlobalLogButtons.Dock = DockStyle.Bottom;
            flowGlobalLogButtons.Height = 46;
            flowGlobalLogButtons.Padding = new Padding(0, 8, 0, 0);
            flowGlobalLogButtons.WrapContents = false;
            flowGlobalLogButtons.Name = "flowGlobalLogButtons";
            flowGlobalLogButtons.FlowDirection = FlowDirection.LeftToRight;
            flowGlobalLogButtons.Controls.Add(btnGlobalLogClear);
            flowGlobalLogButtons.Controls.Add(btnGlobalLogSave);
            // 
            // btnGlobalLogClear styled
            // 
            btnGlobalLogClear.AutoSize = false;
            btnGlobalLogClear.Size = new Size(110, 30);
            btnGlobalLogClear.Name = "btnGlobalLogClear";
            btnGlobalLogClear.Text = "Clear Log";
            StyleActionButton(btnGlobalLogClear, primary:true);
            btnGlobalLogClear.BackColor = Color.FromArgb(255,199,0); // yellow
            btnGlobalLogClear.FlatAppearance.BorderColor = Color.FromArgb(214,167,0);
            btnGlobalLogClear.ForeColor = Color.Black;
            btnGlobalLogClear.Margin = new Padding(0,0,18,0); // more space like DHCP/Static
            // 
            // btnGlobalLogSave styled dark green
            // 
            btnGlobalLogSave.AutoSize = false;
            btnGlobalLogSave.Size = new Size(130, 30);
            btnGlobalLogSave.Name = "btnGlobalLogSave";
            btnGlobalLogSave.Text = "Save Log As";
            StyleActionButton(btnGlobalLogSave, primary:false);
            btnGlobalLogSave.BackColor = Color.FromArgb(20,111,20); // dark green
            btnGlobalLogSave.FlatAppearance.BorderColor = Color.FromArgb(15,85,15);
            btnGlobalLogSave.ForeColor = Color.White;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1116, 950);
            MinimumSize = new Size(1000, 800);
            Controls.Add(splitMain);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Network Utility App";
            Load += Form1_Load;
            Resize += Form1_Resize; // ensure dynamic splitter adjustment
            FormClosing += Form1_FormClosing;
            pnlGlobalLogInner.ResumeLayout(false);
            pnlGlobalLogInner.PerformLayout();
            flowGlobalLogButtons.ResumeLayout(false);
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitMain)).EndInit();
            ResumeLayout(false);
        }

        private void StyleActionButton(Button b, bool primary)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            if (primary)
            {
                b.BackColor = Color.FromArgb(14,99,156);
                b.FlatAppearance.BorderColor = Color.FromArgb(14,99,156);
            }
            else
            {
                b.BackColor = Color.FromArgb(0,120,212);
                b.FlatAppearance.BorderColor = Color.FromArgb(0,120,212);
            }
            b.ForeColor = Color.White;
            b.Cursor = Cursors.Hand;
            b.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        }
    }
}