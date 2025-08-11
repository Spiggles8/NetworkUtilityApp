using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;
using NetworkUtilityApp.Tabs;

namespace NetworkUtilityApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private MaterialTabSelector tabSelector;
        private MaterialTabControl tabMain;
        private TabPage tabPageAdapters;
        private TabPage tabPageDiagnostics;
        private TabPage tabPageDiscovery;
        private TabPage tabPageSettings;

        private TabNetworkAdapters tabNetworkAdapters;
        private TabDiagnostics tabDiagnostics;
        private TabNetworkDiscovery tabNetworkDiscovery;
        private TabSettings tabSettings;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tabSelector = new MaterialTabSelector();
            tabMain = new MaterialTabControl();
            tabPageAdapters = new TabPage();
            tabNetworkAdapters = new TabNetworkAdapters();
            tabPageDiagnostics = new TabPage();
            tabDiagnostics = new TabDiagnostics();
            tabPageDiscovery = new TabPage();
            tabNetworkDiscovery = new TabNetworkDiscovery();
            tabPageSettings = new TabPage();
            tabSettings = new TabSettings();
            tabMain.SuspendLayout();
            tabPageAdapters.SuspendLayout();
            tabPageDiagnostics.SuspendLayout();
            tabPageDiscovery.SuspendLayout();
            tabPageSettings.SuspendLayout();
            SuspendLayout();
            // 
            // tabSelector
            // 
            tabSelector.BaseTabControl = tabMain;
            tabSelector.Dock = DockStyle.Top;
            tabSelector.Name = "tabSelector";
            tabSelector.Height = 48;
            tabSelector.TabIndex = 0;
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabPageAdapters);
            tabMain.Controls.Add(tabPageDiagnostics);
            tabMain.Controls.Add(tabPageDiscovery);
            tabMain.Controls.Add(tabPageSettings);
            tabMain.Depth = 0;
            tabMain.Dock = DockStyle.Fill;
            tabMain.MouseState = MaterialSkin.MouseState.HOVER;
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.TabIndex = 1;
            // 
            // tabPageAdapters
            // 
            tabPageAdapters.Controls.Add(tabNetworkAdapters);
            tabPageAdapters.Name = "tabPageAdapters";
            tabPageAdapters.Text = "Network Adapters";
            tabPageAdapters.UseVisualStyleBackColor = true;
            // 
            // tabNetworkAdapters
            // 
            tabNetworkAdapters.Dock = DockStyle.Fill;
            tabNetworkAdapters.Name = "tabNetworkAdapters";
            tabNetworkAdapters.TabIndex = 0;
            // 
            // tabPageDiagnostics
            // 
            tabPageDiagnostics.Controls.Add(tabDiagnostics);
            tabPageDiagnostics.Name = "tabPageDiagnostics";
            tabPageDiagnostics.Text = "Diagnostics";
            tabPageDiagnostics.UseVisualStyleBackColor = true;
            // 
            // tabDiagnostics
            // 
            tabDiagnostics.Dock = DockStyle.Fill;
            tabDiagnostics.Name = "tabDiagnostics";
            tabDiagnostics.TabIndex = 0;
            // 
            // tabPageDiscovery
            // 
            tabPageDiscovery.Controls.Add(tabNetworkDiscovery);
            tabPageDiscovery.Name = "tabPageDiscovery";
            tabPageDiscovery.Text = "Network Discovery";
            tabPageDiscovery.UseVisualStyleBackColor = true;
            // 
            // tabNetworkDiscovery
            // 
            tabNetworkDiscovery.Dock = DockStyle.Fill;
            tabNetworkDiscovery.Name = "tabNetworkDiscovery";
            tabNetworkDiscovery.TabIndex = 0;
            // 
            // tabPageSettings
            // 
            tabPageSettings.Controls.Add(tabSettings);
            tabPageSettings.Name = "tabPageSettings";
            tabPageSettings.Text = "Settings";
            tabPageSettings.UseVisualStyleBackColor = true;
            // 
            // tabSettings
            // 
            tabSettings.Dock = DockStyle.Fill;
            tabSettings.Name = "tabSettings";
            tabSettings.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1116, 759);
            Controls.Add(tabMain);
            Controls.Add(tabSelector);
            FormBorderStyle = FormBorderStyle.Sizable;
            FormStyle = FormStyles.ActionBar_56;   // <<< enable Material header (title + window buttons)
            ControlBox = true;
            MinimizeBox = true;
            MaximizeBox = true;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Network Utility App";          // shows in the Material header
            Load += Form1_Load;
            tabMain.ResumeLayout(false);
            tabPageAdapters.ResumeLayout(false);
            tabPageDiagnostics.ResumeLayout(false);
            tabPageDiscovery.ResumeLayout(false);
            tabPageSettings.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
