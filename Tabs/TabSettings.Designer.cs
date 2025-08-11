using System.Windows.Forms;
using MaterialSkin.Controls;

namespace NetworkUtilityApp.Tabs
{
    partial class TabSettings
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel layoutRoot;

        // Group: UI & Appearance
        private GroupBox grpUi;
        private MaterialSwitch swDarkMode;

        // Group: Logging (Rolling)
        private GroupBox grpLogging;
        private MaterialCheckbox chkRollingEnable;
        private Label lblHead;
        private Label lblTail;
        private Label lblThreshold;
        private NumericUpDown numRollingHead;
        private NumericUpDown numRollingTail;
        private NumericUpDown numRollingThreshold;

        // Group: Diagnostics & Discovery Defaults
        private GroupBox grpDiagDiscovery;
        private Label lblPingInterval;
        private NumericUpDown numPingIntervalMs;
        private MaterialCheckbox chkDiscoveryResolveDns;
        private Label lblDiscoveryTimeout;
        private NumericUpDown numDiscoveryTimeoutMs;
        private Label lblDiscoveryParallel;
        private NumericUpDown numDiscoveryMaxParallel;
        private MaterialCheckbox chkExportActiveOnly;

        // Group: Adapters table
        private GroupBox grpAdapters;
        private MaterialCheckbox chkShowMacInTable;

        // Group: Presets
        private GroupBox grpPresets;
        private FlowLayoutPanel flowPresets;
        private MaterialButton btnPreset1;
        private MaterialButton btnPreset2;
        private MaterialButton btnPreset3;
        private MaterialButton btnPreset4;
        private MaterialButton btnPreset5;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.layoutRoot = new System.Windows.Forms.TableLayoutPanel();

            this.grpUi = new System.Windows.Forms.GroupBox();
            this.swDarkMode = new MaterialSkin.Controls.MaterialSwitch();

            this.grpLogging = new System.Windows.Forms.GroupBox();
            this.chkRollingEnable = new MaterialSkin.Controls.MaterialCheckbox();
            this.lblHead = new System.Windows.Forms.Label();
            this.lblTail = new System.Windows.Forms.Label();
            this.lblThreshold = new System.Windows.Forms.Label();
            this.numRollingHead = new System.Windows.Forms.NumericUpDown();
            this.numRollingTail = new System.Windows.Forms.NumericUpDown();
            this.numRollingThreshold = new System.Windows.Forms.NumericUpDown();

            this.grpDiagDiscovery = new System.Windows.Forms.GroupBox();
            this.lblPingInterval = new System.Windows.Forms.Label();
            this.numPingIntervalMs = new System.Windows.Forms.NumericUpDown();
            this.chkDiscoveryResolveDns = new MaterialSkin.Controls.MaterialCheckbox();
            this.lblDiscoveryTimeout = new System.Windows.Forms.Label();
            this.numDiscoveryTimeoutMs = new System.Windows.Forms.NumericUpDown();
            this.lblDiscoveryParallel = new System.Windows.Forms.Label();
            this.numDiscoveryMaxParallel = new System.Windows.Forms.NumericUpDown();
            this.chkExportActiveOnly = new MaterialSkin.Controls.MaterialCheckbox();

            this.grpAdapters = new System.Windows.Forms.GroupBox();
            this.chkShowMacInTable = new MaterialSkin.Controls.MaterialCheckbox();

            this.grpPresets = new System.Windows.Forms.GroupBox();
            this.flowPresets = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPreset1 = new MaterialSkin.Controls.MaterialButton();
            this.btnPreset2 = new MaterialSkin.Controls.MaterialButton();
            this.btnPreset3 = new MaterialSkin.Controls.MaterialButton();
            this.btnPreset4 = new MaterialSkin.Controls.MaterialButton();
            this.btnPreset5 = new MaterialSkin.Controls.MaterialButton();

            this.layoutRoot.SuspendLayout();
            this.grpUi.SuspendLayout();
            this.grpLogging.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRollingHead)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRollingTail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRollingThreshold)).BeginInit();
            this.grpDiagDiscovery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPingIntervalMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDiscoveryTimeoutMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDiscoveryMaxParallel)).BeginInit();
            this.grpAdapters.SuspendLayout();
            this.grpPresets.SuspendLayout();
            this.flowPresets.SuspendLayout();

            this.SuspendLayout();

            // layoutRoot
            this.layoutRoot.ColumnCount = 1;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.grpUi, 0, 0);
            this.layoutRoot.Controls.Add(this.grpLogging, 0, 1);
            this.layoutRoot.Controls.Add(this.grpDiagDiscovery, 0, 2);
            this.layoutRoot.Controls.Add(this.grpAdapters, 0, 3);
            this.layoutRoot.Controls.Add(this.grpPresets, 0, 4);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Location = new System.Drawing.Point(0, 0);
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 5;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Padding = new Padding(12);
            this.layoutRoot.Size = new System.Drawing.Size(900, 600);
            this.layoutRoot.TabIndex = 0;

            // grpUi
            this.grpUi.Controls.Add(this.swDarkMode);
            this.grpUi.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpUi.Location = new System.Drawing.Point(12, 12);
            this.grpUi.Margin = new Padding(0, 0, 0, 12);
            this.grpUi.Name = "grpUi";
            this.grpUi.Padding = new Padding(12);
            this.grpUi.Size = new System.Drawing.Size(876, 70);
            this.grpUi.TabIndex = 0;
            this.grpUi.TabStop = true;
            this.grpUi.Text = "UI & Appearance";

            // swDarkMode
            this.swDarkMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.swDarkMode.AutoSize = true;
            this.swDarkMode.Depth = 0;
            this.swDarkMode.Location = new System.Drawing.Point(16, 28);
            this.swDarkMode.Margin = new Padding(0);
            this.swDarkMode.MouseLocation = new System.Drawing.Point(-1, -1);
            this.swDarkMode.MouseState = MaterialSkin.MouseState.HOVER;
            this.swDarkMode.Name = "swDarkMode";
            this.swDarkMode.Ripple = true;
            this.swDarkMode.Size = new System.Drawing.Size(133, 37);
            this.swDarkMode.TabIndex = 0;
            this.swDarkMode.Text = "Dark Mode";
            this.swDarkMode.UseVisualStyleBackColor = true;

            // grpLogging
            this.grpLogging.Controls.Add(this.chkRollingEnable);
            this.grpLogging.Controls.Add(this.lblHead);
            this.grpLogging.Controls.Add(this.numRollingHead);
            this.grpLogging.Controls.Add(this.lblTail);
            this.grpLogging.Controls.Add(this.numRollingTail);
            this.grpLogging.Controls.Add(this.lblThreshold);
            this.grpLogging.Controls.Add(this.numRollingThreshold);
            this.grpLogging.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpLogging.Location = new System.Drawing.Point(12, 94);
            this.grpLogging.Margin = new Padding(0, 0, 0, 12);
            this.grpLogging.Name = "grpLogging";
            this.grpLogging.Padding = new Padding(12);
            this.grpLogging.Size = new System.Drawing.Size(876, 120);
            this.grpLogging.TabIndex = 1;
            this.grpLogging.TabStop = true;
            this.grpLogging.Text = "Logging (Rolling Status Console)";

            // chkRollingEnable
            this.chkRollingEnable.AutoSize = true;
            this.chkRollingEnable.Depth = 0;
            this.chkRollingEnable.Location = new System.Drawing.Point(16, 28);
            this.chkRollingEnable.Margin = new Padding(0);
            this.chkRollingEnable.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkRollingEnable.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkRollingEnable.Name = "chkRollingEnable";
            this.chkRollingEnable.ReadOnly = false;
            this.chkRollingEnable.Ripple = true;
            this.chkRollingEnable.Size = new System.Drawing.Size(235, 37);
            this.chkRollingEnable.TabIndex = 0;
            this.chkRollingEnable.Text = "Enable rolling limit";
            this.chkRollingEnable.UseVisualStyleBackColor = true;

            // lblHead
            this.lblHead.AutoSize = true;
            this.lblHead.Location = new System.Drawing.Point(16, 76);
            this.lblHead.Name = "lblHead";
            this.lblHead.Size = new System.Drawing.Size(126, 15);
            this.lblHead.TabIndex = 1;
            this.lblHead.Text = "Keep first (head) lines:";

            // numRollingHead
            this.numRollingHead.Location = new System.Drawing.Point(150, 72);
            this.numRollingHead.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numRollingHead.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            this.numRollingHead.Name = "numRollingHead";
            this.numRollingHead.Size = new System.Drawing.Size(90, 23);
            this.numRollingHead.TabIndex = 2;

            // lblTail
            this.lblTail.AutoSize = true;
            this.lblTail.Location = new System.Drawing.Point(260, 76);
            this.lblTail.Name = "lblTail";
            this.lblTail.Size = new System.Drawing.Size(111, 15);
            this.lblTail.TabIndex = 3;
            this.lblTail.Text = "Keep last (tail) lines:";

            // numRollingTail
            this.numRollingTail.Location = new System.Drawing.Point(380, 72);
            this.numRollingTail.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numRollingTail.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            this.numRollingTail.Name = "numRollingTail";
            this.numRollingTail.Size = new System.Drawing.Size(90, 23);
            this.numRollingTail.TabIndex = 4;

            // lblThreshold
            this.lblThreshold.AutoSize = true;
            this.lblThreshold.Location = new System.Drawing.Point(488, 76);
            this.lblThreshold.Name = "lblThreshold";
            this.lblThreshold.Size = new System.Drawing.Size(175, 15);
            this.lblThreshold.TabIndex = 5;
            this.lblThreshold.Text = "Trim when lines exceed (total):";

            // numRollingThreshold
            this.numRollingThreshold.Location = new System.Drawing.Point(670, 72);
            this.numRollingThreshold.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            this.numRollingThreshold.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            this.numRollingThreshold.Name = "numRollingThreshold";
            this.numRollingThreshold.Size = new System.Drawing.Size(100, 23);
            this.numRollingThreshold.TabIndex = 6;

            // grpDiagDiscovery
            this.grpDiagDiscovery.Controls.Add(this.lblPingInterval);
            this.grpDiagDiscovery.Controls.Add(this.numPingIntervalMs);
            this.grpDiagDiscovery.Controls.Add(this.chkDiscoveryResolveDns);
            this.grpDiagDiscovery.Controls.Add(this.lblDiscoveryTimeout);
            this.grpDiagDiscovery.Controls.Add(this.numDiscoveryTimeoutMs);
            this.grpDiagDiscovery.Controls.Add(this.lblDiscoveryParallel);
            this.grpDiagDiscovery.Controls.Add(this.numDiscoveryMaxParallel);
            this.grpDiagDiscovery.Controls.Add(this.chkExportActiveOnly);
            this.grpDiagDiscovery.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpDiagDiscovery.Location = new System.Drawing.Point(12, 226);
            this.grpDiagDiscovery.Margin = new Padding(0, 0, 0, 12);
            this.grpDiagDiscovery.Name = "grpDiagDiscovery";
            this.grpDiagDiscovery.Padding = new Padding(12);
            this.grpDiagDiscovery.Size = new System.Drawing.Size(876, 140);
            this.grpDiagDiscovery.TabIndex = 2;
            this.grpDiagDiscovery.TabStop = true;
            this.grpDiagDiscovery.Text = "Diagnostics & Discovery Defaults";

            // lblPingInterval
            this.lblPingInterval.AutoSize = true;
            this.lblPingInterval.Location = new System.Drawing.Point(16, 32);
            this.lblPingInterval.Name = "lblPingInterval";
            this.lblPingInterval.Size = new System.Drawing.Size(165, 15);
            this.lblPingInterval.TabIndex = 0;
            this.lblPingInterval.Text = "Continuous ping interval (ms):";

            // numPingIntervalMs
            this.numPingIntervalMs.Location = new System.Drawing.Point(190, 28);
            this.numPingIntervalMs.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            this.numPingIntervalMs.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numPingIntervalMs.Name = "numPingIntervalMs";
            this.numPingIntervalMs.Size = new System.Drawing.Size(90, 23);
            this.numPingIntervalMs.TabIndex = 1;
            this.numPingIntervalMs.Value = new decimal(new int[] { 1000, 0, 0, 0 });

            // chkDiscoveryResolveDns
            this.chkDiscoveryResolveDns.AutoSize = true;
            this.chkDiscoveryResolveDns.Depth = 0;
            this.chkDiscoveryResolveDns.Location = new System.Drawing.Point(16, 70);
            this.chkDiscoveryResolveDns.Margin = new Padding(0);
            this.chkDiscoveryResolveDns.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkDiscoveryResolveDns.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkDiscoveryResolveDns.Name = "chkDiscoveryResolveDns";
            this.chkDiscoveryResolveDns.ReadOnly = false;
            this.chkDiscoveryResolveDns.Ripple = true;
            this.chkDiscoveryResolveDns.Size = new System.Drawing.Size(161, 37);
            this.chkDiscoveryResolveDns.TabIndex = 2;
            this.chkDiscoveryResolveDns.Text = "Resolve DNS";
            this.chkDiscoveryResolveDns.UseVisualStyleBackColor = true;

            // lblDiscoveryTimeout
            this.lblDiscoveryTimeout.AutoSize = true;
            this.lblDiscoveryTimeout.Location = new System.Drawing.Point(200, 78);
            this.lblDiscoveryTimeout.Name = "lblDiscoveryTimeout";
            this.lblDiscoveryTimeout.Size = new System.Drawing.Size(151, 15);
            this.lblDiscoveryTimeout.TabIndex = 3;
            this.lblDiscoveryTimeout.Text = "Discovery timeout (ms):";

            // numDiscoveryTimeoutMs
            this.numDiscoveryTimeoutMs.Location = new System.Drawing.Point(360, 74);
            this.numDiscoveryTimeoutMs.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            this.numDiscoveryTimeoutMs.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numDiscoveryTimeoutMs.Name = "numDiscoveryTimeoutMs";
            this.numDiscoveryTimeoutMs.Size = new System.Drawing.Size(90, 23);
            this.numDiscoveryTimeoutMs.TabIndex = 4;
            this.numDiscoveryTimeoutMs.Value = new decimal(new int[] { 1000, 0, 0, 0 });

            // lblDiscoveryParallel
            this.lblDiscoveryParallel.AutoSize = true;
            this.lblDiscoveryParallel.Location = new System.Drawing.Point(468, 78);
            this.lblDiscoveryParallel.Name = "lblDiscoveryParallel";
            this.lblDiscoveryParallel.Size = new System.Drawing.Size(131, 15);
            this.lblDiscoveryParallel.TabIndex = 5;
            this.lblDiscoveryParallel.Text = "Max parallel workers:";

            // numDiscoveryMaxParallel
            this.numDiscoveryMaxParallel.Location = new System.Drawing.Point(606, 74);
            this.numDiscoveryMaxParallel.Maximum = new decimal(new int[] { 2048, 0, 0, 0 });
            this.numDiscoveryMaxParallel.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numDiscoveryMaxParallel.Name = "numDiscoveryMaxParallel";
            this.numDiscoveryMaxParallel.Size = new System.Drawing.Size(90, 23);
            this.numDiscoveryMaxParallel.TabIndex = 6;
            this.numDiscoveryMaxParallel.Value = new decimal(new int[] { 256, 0, 0, 0 });

            // chkExportActiveOnly
            this.chkExportActiveOnly.AutoSize = true;
            this.chkExportActiveOnly.Depth = 0;
            this.chkExportActiveOnly.Location = new System.Drawing.Point(16, 108);
            this.chkExportActiveOnly.Margin = new Padding(0);
            this.chkExportActiveOnly.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkExportActiveOnly.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkExportActiveOnly.Name = "chkExportActiveOnly";
            this.chkExportActiveOnly.ReadOnly = false;
            this.chkExportActiveOnly.Ripple = true;
            this.chkExportActiveOnly.Size = new System.Drawing.Size(321, 37);
            this.chkExportActiveOnly.TabIndex = 7;
            this.chkExportActiveOnly.Text = "Export discovery: active hosts only";
            this.chkExportActiveOnly.UseVisualStyleBackColor = true;

            // grpAdapters
            this.grpAdapters.Controls.Add(this.chkShowMacInTable);
            this.grpAdapters.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpAdapters.Location = new System.Drawing.Point(12, 378);
            this.grpAdapters.Margin = new Padding(0, 0, 0, 12);
            this.grpAdapters.Name = "grpAdapters";
            this.grpAdapters.Padding = new Padding(12);
            this.grpAdapters.Size = new System.Drawing.Size(876, 70);
            this.grpAdapters.TabIndex = 3;
            this.grpAdapters.TabStop = true;
            this.grpAdapters.Text = "Adapters Table";

            // chkShowMacInTable
            this.chkShowMacInTable.AutoSize = true;
            this.chkShowMacInTable.Depth = 0;
            this.chkShowMacInTable.Location = new System.Drawing.Point(16, 28);
            this.chkShowMacInTable.Margin = new Padding(0);
            this.chkShowMacInTable.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkShowMacInTable.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkShowMacInTable.Name = "chkShowMacInTable";
            this.chkShowMacInTable.ReadOnly = false;
            this.chkShowMacInTable.Ripple = true;
            this.chkShowMacInTable.Size = new System.Drawing.Size(244, 37);
            this.chkShowMacInTable.TabIndex = 0;
            this.chkShowMacInTable.Text = "Show MAC in table";
            this.chkShowMacInTable.UseVisualStyleBackColor = true;

            // grpPresets
            this.grpPresets.Controls.Add(this.flowPresets);
            this.grpPresets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpPresets.Location = new System.Drawing.Point(12, 460);
            this.grpPresets.Margin = new Padding(0);
            this.grpPresets.Name = "grpPresets";
            this.grpPresets.Padding = new Padding(12);
            this.grpPresets.Size = new System.Drawing.Size(876, 128);
            this.grpPresets.TabIndex = 4;
            this.grpPresets.TabStop = true;
            this.grpPresets.Text = "Static IP Presets (click to edit)";

            // flowPresets
            this.flowPresets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPresets.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowPresets.Location = new System.Drawing.Point(12, 28);
            this.flowPresets.Margin = new Padding(0);
            this.flowPresets.Name = "flowPresets";
            this.flowPresets.Padding = new Padding(4);
            this.flowPresets.Size = new System.Drawing.Size(852, 88);
            this.flowPresets.TabIndex = 0;
            this.flowPresets.WrapContents = true;

            // btnPreset1..5
            this.btnPreset1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.btnPreset1.Density = MaterialButton.MaterialButtonDensity.Default;
            this.btnPreset1.HighEmphasis = true;
            this.btnPreset1.Icon = null;
            this.btnPreset1.Name = "btnPreset1";
            this.btnPreset1.Text = "Preset 1";
            this.btnPreset1.Type = MaterialButton.MaterialButtonType.Contained;
            this.btnPreset1.UseAccentColor = false;
            this.btnPreset1.Size = new System.Drawing.Size(120, 36);
            this.btnPreset1.Margin = new Padding(6);

            this.btnPreset2 = ClonePresetButton("btnPreset2", "Preset 2");
            this.btnPreset3 = ClonePresetButton("btnPreset3", "Preset 3");
            this.btnPreset4 = ClonePresetButton("btnPreset4", "Preset 4");
            this.btnPreset5 = ClonePresetButton("btnPreset5", "Preset 5");

            this.flowPresets.Controls.Add(this.btnPreset1);
            this.flowPresets.Controls.Add(this.btnPreset2);
            this.flowPresets.Controls.Add(this.btnPreset3);
            this.flowPresets.Controls.Add(this.btnPreset4);
            this.flowPresets.Controls.Add(this.btnPreset5);

            // TabSettings (UserControl)
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.layoutRoot);
            this.Name = "TabSettings";
            this.Size = new System.Drawing.Size(900, 600);

            this.layoutRoot.ResumeLayout(false);
            this.grpUi.ResumeLayout(false);
            this.grpUi.PerformLayout();
            this.grpLogging.ResumeLayout(false);
            this.grpLogging.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRollingHead)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRollingTail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRollingThreshold)).EndInit();
            this.grpDiagDiscovery.ResumeLayout(false);
            this.grpDiagDiscovery.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPingIntervalMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDiscoveryTimeoutMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDiscoveryMaxParallel)).EndInit();
            this.grpAdapters.ResumeLayout(false);
            this.grpAdapters.PerformLayout();
            this.grpPresets.ResumeLayout(false);
            this.flowPresets.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        // helper to make similar Material buttons
        private MaterialButton ClonePresetButton(string name, string text)
        {
            var b = new MaterialButton
            {
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Density = MaterialButton.MaterialButtonDensity.Default,
                HighEmphasis = true,
                Icon = null,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = false,
                Size = new System.Drawing.Size(120, 36),
                Margin = new Padding(6),
                Name = name,
                Text = text
            };
            return b;
        }
    }
}
