using MaterialSkin;
using MaterialSkin.Controls;
using NetworkUtilityApp.Tabs;
using System;
using System.Windows.Forms;

namespace NetworkUtilityApp
{
    public partial class Form1 : MaterialForm
    {
        private readonly MaterialSkinManager _skin = MaterialSkinManager.Instance;

        public Form1()
        {
            InitializeComponent();

            var mgr = MaterialSkin.MaterialSkinManager.Instance;
            mgr.EnforceBackcolorOnAllComponents = true;
            mgr.AddFormToManage(this);


            // MaterialSkin setup
            _skin.EnforceBackcolorOnAllComponents = true;
            _skin.AddFormToManage(this);

            // Apply saved theme (default light if setting not present)
            var isDark = false;
            try { isDark = Properties.Settings.Default.DarkMode; } catch { /* first run */ }
            ApplyTheme(isDark);

            // Optional: smooth out repaints
            this.DoubleBuffered = true;
        }

        public void ApplyTheme(bool isDark)
        {
            _skin.Theme = isDark
                ? MaterialSkinManager.Themes.DARK
                : MaterialSkinManager.Themes.LIGHT;

            _skin.ColorScheme = new ColorScheme(
                Primary.Teal700,
                Primary.Teal900,
                Primary.Teal200,
                Accent.Cyan200,
                TextShade.WHITE
            );
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Default to first tab
            tabMain.SelectedIndex = 0;

            // Initialize each tab's content
            tabNetworkAdapters.Initialize();
            tabDiagnostics.Initialize();
            tabNetworkDiscovery.Initialize();
            tabSettings.Initialize();
        }
    }
}
