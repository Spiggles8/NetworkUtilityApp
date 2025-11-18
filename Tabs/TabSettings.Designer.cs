using System.Drawing;
using System.Windows.Forms;

namespace NetworkUtilityApp.Tabs
{
    partial class TabSettings
    {
        private System.ComponentModel.IContainer components = null;

        private GroupBox grpFavorites;
        private Label lblIp;
        private TextBox txtFavIp;
        private Label lblSubnet;
        private TextBox txtFavSubnet;
        private Label lblGateway;
        private TextBox txtFavGateway;
        private FlowLayoutPanel flowSaveButtons;
        private Button btnSaveFav1;
        private Button btnSaveFav2;
        private Button btnSaveFav3;
        private Button btnSaveFav4;
        private Label lblHint;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            grpFavorites = new GroupBox();
            lblIp = new Label();
            txtFavIp = new TextBox();
            lblSubnet = new Label();
            txtFavSubnet = new TextBox();
            lblGateway = new Label();
            txtFavGateway = new TextBox();
            lblHint = new Label();
            flowSaveButtons = new FlowLayoutPanel();
            btnSaveFav1 = new Button();
            btnSaveFav2 = new Button();
            btnSaveFav3 = new Button();
            btnSaveFav4 = new Button();
            grpFavorites.SuspendLayout();
            flowSaveButtons.SuspendLayout();
            SuspendLayout();
            // 
            // grpFavorites
            // 
            grpFavorites.AutoSize = false; // was true; prevents shrinking when filling
            grpFavorites.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpFavorites.Controls.Add(lblIp);
            grpFavorites.Controls.Add(txtFavIp);
            grpFavorites.Controls.Add(lblSubnet);
            grpFavorites.Controls.Add(txtFavSubnet);
            grpFavorites.Controls.Add(lblGateway);
            grpFavorites.Controls.Add(txtFavGateway);
            grpFavorites.Controls.Add(lblHint);
            grpFavorites.Controls.Add(flowSaveButtons);
            grpFavorites.Dock = DockStyle.Fill; // was DockStyle.Top
            grpFavorites.Location = new Point(0, 0);
            grpFavorites.Name = "grpFavorites";
            grpFavorites.Padding = new Padding(12);
            grpFavorites.Size = new Size(800, 500); // fills tab
            grpFavorites.TabIndex = 0;
            grpFavorites.TabStop = false;
            grpFavorites.Text = "Favorite IP Presets";
            // 
            // lblIp
            // 
            lblIp.AutoSize = true;
            lblIp.Location = new Point(16, 32);
            lblIp.Name = "lblIp";
            lblIp.Size = new Size(65, 15);
            lblIp.TabIndex = 0;
            lblIp.Text = "IP Address:";
            // 
            // txtFavIp
            // 
            txtFavIp.Location = new Point(120, 28);
            txtFavIp.Name = "txtFavIp";
            txtFavIp.Size = new Size(180, 23);
            txtFavIp.TabIndex = 1;
            // 
            // lblSubnet
            // 
            lblSubnet.AutoSize = true;
            lblSubnet.Location = new Point(16, 64);
            lblSubnet.Name = "lblSubnet";
            lblSubnet.Size = new Size(78, 15);
            lblSubnet.TabIndex = 2;
            lblSubnet.Text = "Subnet Mask:";
            // 
            // txtFavSubnet
            // 
            txtFavSubnet.Location = new Point(120, 60);
            txtFavSubnet.Name = "txtFavSubnet";
            txtFavSubnet.Size = new Size(180, 23);
            txtFavSubnet.TabIndex = 3;
            txtFavSubnet.Text = "255.255.255.0";
            // 
            // lblGateway
            // 
            lblGateway.AutoSize = true;
            lblGateway.Location = new Point(16, 96);
            lblGateway.Name = "lblGateway";
            lblGateway.Size = new Size(55, 15);
            lblGateway.TabIndex = 4;
            lblGateway.Text = "Gateway:";
            // 
            // txtFavGateway
            // 
            txtFavGateway.Location = new Point(120, 92);
            txtFavGateway.Name = "txtFavGateway";
            txtFavGateway.Size = new Size(180, 23);
            txtFavGateway.TabIndex = 5;
            // 
            // lblHint
            // 
            lblHint.AutoSize = true;
            lblHint.Location = new Point(16, 124);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(553, 15);
            lblHint.TabIndex = 6;
            lblHint.Text = "Tip: Set IP, Subnet and Gateway, then click one of the Save buttons to store that preset to a Favorite slot.";
            // 
            // flowSaveButtons
            // 
            flowSaveButtons.AutoSize = true;
            flowSaveButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowSaveButtons.Controls.Add(btnSaveFav1);
            flowSaveButtons.Controls.Add(btnSaveFav2);
            flowSaveButtons.Controls.Add(btnSaveFav3);
            flowSaveButtons.Controls.Add(btnSaveFav4);
            flowSaveButtons.Location = new Point(16, 150);
            flowSaveButtons.Name = "flowSaveButtons";
            flowSaveButtons.Size = new Size(466, 31);
            flowSaveButtons.TabIndex = 7;
            flowSaveButtons.WrapContents = false;
            // 
            // btnSaveFav1
            // 
            btnSaveFav1.AutoSize = true;
            btnSaveFav1.Location = new Point(0, 0);
            btnSaveFav1.Margin = new Padding(0, 0, 8, 0);
            btnSaveFav1.Name = "btnSaveFav1";
            btnSaveFav1.Size = new Size(109, 25);
            btnSaveFav1.TabIndex = 0;
            btnSaveFav1.Text = "Save to Favorite 1";
            // 
            // btnSaveFav2
            // 
            btnSaveFav2.AutoSize = true;
            btnSaveFav2.Location = new Point(117, 0);
            btnSaveFav2.Margin = new Padding(0, 0, 8, 0);
            btnSaveFav2.Name = "btnSaveFav2";
            btnSaveFav2.Size = new Size(109, 25);
            btnSaveFav2.TabIndex = 1;
            btnSaveFav2.Text = "Save to Favorite 2";
            // 
            // btnSaveFav3
            // 
            btnSaveFav3.AutoSize = true;
            btnSaveFav3.Location = new Point(234, 0);
            btnSaveFav3.Margin = new Padding(0, 0, 8, 0);
            btnSaveFav3.Name = "btnSaveFav3";
            btnSaveFav3.Size = new Size(109, 25);
            btnSaveFav3.TabIndex = 2;
            btnSaveFav3.Text = "Save to Favorite 3";
            // 
            // btnSaveFav4
            // 
            btnSaveFav4.AutoSize = true;
            btnSaveFav4.Location = new Point(354, 3);
            btnSaveFav4.Name = "btnSaveFav4";
            btnSaveFav4.Size = new Size(109, 25);
            btnSaveFav4.TabIndex = 3;
            btnSaveFav4.Text = "Save to Favorite 4";
            // 
            // TabSettings
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(grpFavorites);
            Name = "TabSettings";
            Size = new Size(800, 500);
            grpFavorites.ResumeLayout(false);
            grpFavorites.PerformLayout();
            flowSaveButtons.ResumeLayout(false);
            flowSaveButtons.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}