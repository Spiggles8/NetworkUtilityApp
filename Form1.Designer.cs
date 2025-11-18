using System.Drawing;
using System.Windows.Forms;
using NetworkUtilityApp.Tabs;

namespace NetworkUtilityApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private TabControl tabMain;
        private TabPage tabPageAdapters;
        private TabPage tabPageDiagnostics;
        private TabPage tabPageDiscovery;
        private TabPage tabPageSettings;

        private TabNetworkAdapters tabNetworkAdapters;
        private TabDiagnostics tabDiagnostics;
        private TabSettings tabSettings;
        private TabDiscovery tabDiscovery; // Added declaration for TabDiscovery

        // Global log controls (+ new buttons)
        private Panel pnlGlobalLog;
        private Label lblGlobalLog;
        private TextBox txtGlobalLog;
        private FlowLayoutPanel flowGlobalLogButtons;
        private Button btnGlobalLogClear;
        private Button btnGlobalLogSave;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tabMain = new TabControl();
            tabPageAdapters = new TabPage();
            tabNetworkAdapters = new TabNetworkAdapters();
            tabDiagnostics = new TabDiagnostics();
            tabPageDiagnostics = new TabPage();
            tabPageDiscovery = new TabPage();
            tabPageSettings = new TabPage();
            tabSettings = new TabSettings();
            tabDiscovery = new TabDiscovery(); // Instantiation of TabDiscovery

            // Global log controls
            pnlGlobalLog = new Panel();
            lblGlobalLog = new Label();
            txtGlobalLog = new TextBox();
            flowGlobalLogButtons = new FlowLayoutPanel();
            btnGlobalLogClear = new Button();
            btnGlobalLogSave = new Button();

            tabMain.SuspendLayout();
            tabPageAdapters.SuspendLayout();
            tabPageSettings.SuspendLayout();
            pnlGlobalLog.SuspendLayout();
            flowGlobalLogButtons.SuspendLayout();
            SuspendLayout();
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabPageAdapters);
            tabMain.Controls.Add(tabPageDiagnostics);
            tabMain.Controls.Add(tabPageDiscovery);
            tabMain.Controls.Add(tabPageSettings);
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 0);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(1116, 579);
            tabMain.TabIndex = 0;
            // 
            // tabPageAdapters
            // 
            tabPageAdapters.Controls.Add(tabNetworkAdapters);
            tabPageAdapters.Location = new Point(4, 24);
            tabPageAdapters.Name = "tabPageAdapters";
            tabPageAdapters.Size = new Size(1108, 551);
            tabPageAdapters.TabIndex = 0;
            tabPageAdapters.Text = "Network Adapters";
            tabPageAdapters.UseVisualStyleBackColor = true;
            // 
            // tabNetworkAdapters
            // 
            tabNetworkAdapters.Dock = DockStyle.Fill;
            tabNetworkAdapters.Location = new Point(0, 0);
            tabNetworkAdapters.Name = "tabNetworkAdapters";
            tabNetworkAdapters.Size = new Size(1108, 551);
            tabNetworkAdapters.TabIndex = 0;
            // 
            // tabPageDiagnostics
            // 
            tabPageDiagnostics.Location = new Point(4, 24);
            tabPageDiagnostics.Name = "tabPageDiagnostics";
            tabPageDiagnostics.Size = new Size(1108, 551);
            tabPageDiagnostics.TabIndex = 1;
            tabPageDiagnostics.Text = "Diagnostics";
            tabPageDiagnostics.UseVisualStyleBackColor = true;
            // 
            // tabDiagnostics
            // 
            tabDiagnostics.Dock = DockStyle.Fill;
            tabDiagnostics.Location = new Point(0, 0);
            tabDiagnostics.Name = "tabDiagnostics";
            tabDiagnostics.Size = new Size(1108, 551);
            tabDiagnostics.TabIndex = 0;
            tabPageDiagnostics.Controls.Add(tabDiagnostics);
            // 
            // tabPageDiscovery
            // 
            tabPageDiscovery.Location = new Point(4, 24);
            tabPageDiscovery.Name = "tabPageDiscovery";
            tabPageDiscovery.Size = new Size(1108, 551);
            tabPageDiscovery.TabIndex = 2;
            tabPageDiscovery.Text = "Network Discovery";
            tabPageDiscovery.UseVisualStyleBackColor = true;
            // 
            // tabDiscovery
            // 
            tabDiscovery.Dock = DockStyle.Fill;
            tabDiscovery.Location = new Point(0, 0);
            tabDiscovery.Name = "tabDiscovery";
            tabDiscovery.Size = new Size(1108, 551);
            tabDiscovery.TabIndex = 0;
            tabPageDiscovery.Controls.Add(tabDiscovery); // Added tabDiscovery to tabPageDiscovery
            // 
            // tabPageSettings
            // 
            tabPageSettings.Controls.Add(tabSettings);
            tabPageSettings.Location = new Point(4, 24);
            tabPageSettings.Name = "tabPageSettings";
            tabPageSettings.Size = new Size(1108, 551);
            tabPageSettings.TabIndex = 3;
            tabPageSettings.Text = "Settings";
            tabPageSettings.UseVisualStyleBackColor = true;
            // 
            // tabSettings
            // 
            tabSettings.Dock = DockStyle.Fill;
            tabSettings.Location = new Point(0, 0);
            tabSettings.Name = "tabSettings";
            tabSettings.Size = new Size(1108, 551);
            tabSettings.TabIndex = 0;
            // 
            // pnlGlobalLog
            // 
            pnlGlobalLog.Dock = DockStyle.Bottom;
            pnlGlobalLog.Height = 300; // increased from 150/180 to show more lines
            pnlGlobalLog.Padding = new Padding(6, 0, 6, 6);
            pnlGlobalLog.Name = "pnlGlobalLog";
            pnlGlobalLog.Controls.Add(txtGlobalLog);
            pnlGlobalLog.Controls.Add(flowGlobalLogButtons);
            pnlGlobalLog.Controls.Add(lblGlobalLog);
            // 
            // lblGlobalLog
            // 
            lblGlobalLog.Dock = DockStyle.Top;
            lblGlobalLog.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblGlobalLog.Text = "Output Log"; // removed "(Global)"
            lblGlobalLog.Height = 20;
            lblGlobalLog.TextAlign = ContentAlignment.MiddleLeft;
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
            flowGlobalLogButtons.Height = 34;
            flowGlobalLogButtons.Padding = new Padding(0, 4, 0, 0);
            flowGlobalLogButtons.WrapContents = false;
            flowGlobalLogButtons.Name = "flowGlobalLogButtons";
            flowGlobalLogButtons.Controls.Add(btnGlobalLogClear);
            flowGlobalLogButtons.Controls.Add(btnGlobalLogSave);
            // 
            // btnGlobalLogClear
            // 
            btnGlobalLogClear.AutoSize = true;
            btnGlobalLogClear.Name = "btnGlobalLogClear";
            btnGlobalLogClear.Text = "Clear Log";
            btnGlobalLogClear.Margin = new Padding(0, 0, 8, 0);
            // 
            // btnGlobalLogSave
            // 
            btnGlobalLogSave.AutoSize = true;
            btnGlobalLogSave.Name = "btnGlobalLogSave";
            btnGlobalLogSave.Text = "Save Log As...";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1116, 950); // optional: give overall window more room
            MinimumSize = new Size(1000, 800); // optional: prevent shrinking too small
            Controls.Add(tabMain);
            Controls.Add(pnlGlobalLog);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Network Utility App";
            Load += Form1_Load;

            // Adjust global log height to free more vertical space (optional)
            pnlGlobalLog.Height = 300; // changed from 150
            tabMain.Size = new Size(1116, 600); // (adjust height so tabs don't get squeezed by taller log panel)
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1116, 950); // optional: give overall window more room
            MinimumSize = new Size(1000, 800); // optional: prevent shrinking too small
            Controls.Add(tabMain);
            Controls.Add(pnlGlobalLog);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Network Utility App";
            Load += Form1_Load;
            tabMain.ResumeLayout(false);
            tabPageAdapters.ResumeLayout(false);
            tabPageSettings.ResumeLayout(false);
            flowGlobalLogButtons.ResumeLayout(false);
            flowGlobalLogButtons.PerformLayout();
            pnlGlobalLog.ResumeLayout(false);
            pnlGlobalLog.PerformLayout();
            ResumeLayout(false);
        }
    }
}