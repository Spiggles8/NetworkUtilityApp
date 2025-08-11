using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NetworkUtilityApp.Tabs
{
    public partial class TabSettings : UserControl
    {
        public Action? PresetsChanged; // optional: host can refresh quick buttons elsewhere

        private const int PRESET_COUNT = 5;

        public TabSettings()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                WireEvents();
                LoadSettingsIntoUi();
                RefreshPresetButtonTexts();
            }
        }

        // Call from Form1 after you add the control to its tab, if you want.
        public void Initialize()
        {
            // nothing required; kept for symmetry with other tabs
        }

        #region Wire events

        private void WireEvents()
        {
            // Theme
            Find<CheckBox>("swDarkMode")?.Apply(sw =>
            {
                sw.CheckedChanged += (_, __) =>
                {
                    SaveBool(nameof(Properties.Settings.Default.DarkMode), sw.Checked);
                    ApplyTheme(sw.Checked);
                };
            });

            // Rolling log options
            Find<CheckBox>("chkRollingEnable")?.Apply(cb =>
            {
                cb.CheckedChanged += (_, __) =>
                {
                    SaveBool(nameof(Properties.Settings.Default.RollingEnable), cb.Checked);
                    ToggleRollingEnabledUi(cb.Checked);
                };
            });
            HookNumeric(nameof(Properties.Settings.Default.RollingHead), "numRollingHead");
            HookNumeric(nameof(Properties.Settings.Default.RollingTail), "numRollingTail");
            HookNumeric(nameof(Properties.Settings.Default.RollingThreshold), "numRollingThreshold");

            // Ping
            HookNumeric(nameof(Properties.Settings.Default.PingIntervalMs), "numPingIntervalMs");

            // Discovery
            Find<CheckBox>("chkDiscoveryResolveDns")?.Apply(cb =>
                cb.CheckedChanged += (_, __) => SaveBool(nameof(Properties.Settings.Default.DiscoveryResolveDns), cb.Checked));
            HookNumeric(nameof(Properties.Settings.Default.DiscoveryTimeoutMs), "numDiscoveryTimeoutMs");
            HookNumeric(nameof(Properties.Settings.Default.DiscoveryMaxParallel), "numDiscoveryMaxParallel");
            Find<CheckBox>("chkExportActiveOnly")?.Apply(cb =>
                cb.CheckedChanged += (_, __) => SaveBool(nameof(Properties.Settings.Default.ExportActiveOnly), cb.Checked));

            // Adapters table
            Find<CheckBox>("chkShowMacInTable")?.Apply(cb =>
                cb.CheckedChanged += (_, __) => SaveBool(nameof(Properties.Settings.Default.ShowMacInTable), cb.Checked));

            // Preset buttons
            for (int i = 1; i <= PRESET_COUNT; i++)
            {
                var btn = Find<Button>($"btnPreset{i}");
                if (btn != null)
                {
                    int slot = i;
                    btn.Click += (_, __) => EditPreset(slot);
                }
            }
        }

        private void HookNumeric(string settingName, string controlName)
        {
            var nud = Find<NumericUpDown>(controlName);
            if (nud == null) return;

            nud.ValueChanged += (_, __) =>
            {
                try
                {
                    var v = (int)nud.Value;
                    SaveInt(settingName, v);
                }
                catch { /* ignore */ }
            };
        }

        #endregion

        #region Load / Save settings

        private void LoadSettingsIntoUi()
        {
            // Theme
            SetCheck("swDarkMode", GetBool(nameof(Properties.Settings.Default.DarkMode), false));
            ApplyTheme(GetBool(nameof(Properties.Settings.Default.DarkMode), false));

            // Rolling
            var rollEnable = GetBool(nameof(Properties.Settings.Default.RollingEnable), true);
            SetCheck("chkRollingEnable", rollEnable);
            SetNumeric("numRollingHead", GetInt(nameof(Properties.Settings.Default.RollingHead), 100));
            SetNumeric("numRollingTail", GetInt(nameof(Properties.Settings.Default.RollingTail), 200));
            SetNumeric("numRollingThreshold", GetInt(nameof(Properties.Settings.Default.RollingThreshold), 400));
            ToggleRollingEnabledUi(rollEnable);

            // Ping
            SetNumeric("numPingIntervalMs", GetInt(nameof(Properties.Settings.Default.PingIntervalMs), 1000));

            // Discovery
            SetCheck("chkDiscoveryResolveDns", GetBool(nameof(Properties.Settings.Default.DiscoveryResolveDns), false));
            SetNumeric("numDiscoveryTimeoutMs", GetInt(nameof(Properties.Settings.Default.DiscoveryTimeoutMs), 1000));
            SetNumeric("numDiscoveryMaxParallel", GetInt(nameof(Properties.Settings.Default.DiscoveryMaxParallel), 256));
            SetCheck("chkExportActiveOnly", GetBool(nameof(Properties.Settings.Default.ExportActiveOnly), true));

            // Adapters table
            SetCheck("chkShowMacInTable", GetBool(nameof(Properties.Settings.Default.ShowMacInTable), true));
        }

        private void ToggleRollingEnabledUi(bool enabled)
        {
            Find<NumericUpDown>("numRollingHead")?.Apply(n => n.Enabled = enabled);
            Find<NumericUpDown>("numRollingTail")?.Apply(n => n.Enabled = enabled);
            Find<NumericUpDown>("numRollingThreshold")?.Apply(n => n.Enabled = enabled);
        }

        private static void ApplyTheme(bool dark)
        {
            // Ask the host MaterialForm to switch theme
            if (Application.OpenForms.Count == 0) return;
            var form = Application.OpenForms.Cast<Form>().FirstOrDefault();
            if (form is NetworkUtilityApp.Form1 f1)
            {
                f1.ApplyTheme(dark);
            }
        }

        private static void SaveBool(string name, bool value)
        {
            try
            {
                Properties.Settings.Default[name] = value;
                Properties.Settings.Default.Save();
            }
            catch { /* ignore missing settings at dev time */ }
        }

        private static void SaveInt(string name, int value)
        {
            try
            {
                Properties.Settings.Default[name] = value;
                Properties.Settings.Default.Save();
            }
            catch { /* ignore missing settings at dev time */ }
        }

        private static bool GetBool(string name, bool fallback)
        {
            try
            {
                var o = Properties.Settings.Default[name];
                return o is bool b ? b : fallback;
            }
            catch { return fallback; }
        }

        private static int GetInt(string name, int fallback)
        {
            try
            {
                var o = Properties.Settings.Default[name];
                if (o is int i) return i;
                if (o is string s && int.TryParse(s, out var v)) return v;
                return fallback;
            }
            catch { return fallback; }
        }

        private static string GetString(string name, string fallback = "")
        {
            try
            {
                var o = Properties.Settings.Default[name];
                return o?.ToString() ?? fallback;
            }
            catch { return fallback; }
        }

        private static void SaveString(string name, string value)
        {
            try
            {
                Properties.Settings.Default[name] = value ?? string.Empty;
                Properties.Settings.Default.Save();
            }
            catch { /* ignore */ }
        }

        #endregion

        #region Presets (5 slots)

        private void RefreshPresetButtonTexts()
        {
            for (int i = 1; i <= PRESET_COUNT; i++)
            {
                var btn = Find<Button>($"btnPreset{i}");
                if (btn == null) continue;

                string name = GetString(PresetKey(i, "Name"), $"Preset {i}");
                btn.Text = name;
                btn.AutoSize = false;
                btn.AutoEllipsis = true;
            }
        }

        private void EditPreset(int slot)
        {
            string name = GetString(PresetKey(slot, "Name"), $"Preset {slot}");
            string ip = GetString(PresetKey(slot, "Ip"));
            string mask = GetString(PresetKey(slot, "Subnet"));
            string gw = GetString(PresetKey(slot, "Gateway"));

            using var dlg = new PresetDialog($"Edit Preset {slot}", name, ip, mask, gw);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                SaveString(PresetKey(slot, "Name"), dlg.PresetName);
                SaveString(PresetKey(slot, "Ip"), dlg.Ip);
                SaveString(PresetKey(slot, "Subnet"), dlg.Subnet);
                SaveString(PresetKey(slot, "Gateway"), dlg.Gateway);

                RefreshPresetButtonTexts();
                PresetsChanged?.Invoke();
            }
        }

        private static string PresetKey(int slot, string field)
            => $"Preset{slot}_{field}";

        // Small inlined modal to edit a preset (keeps things in one file)
        private sealed class PresetDialog : Form
        {
            public string PresetName => _txtName.Text.Trim();
            public string Ip => _txtIp.Text.Trim();
            public string Subnet => _txtMask.Text.Trim();
            public string Gateway => _txtGw.Text.Trim();

            private readonly TextBox _txtName = new() { Width = 240 };
            private readonly TextBox _txtIp = new() { Width = 240 };
            private readonly TextBox _txtMask = new() { Width = 240 };
            private readonly TextBox _txtGw = new() { Width = 240 };

            public PresetDialog(string title, string name, string ip, string mask, string gw)
            {
                Text = title;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = MinimizeBox = false;
                ClientSize = new Size(360, 220);

                var lblName = new Label { Text = "Preset Name", AutoSize = true, Top = 16, Left = 16 };
                var lblIp = new Label { Text = "IP Address", AutoSize = true, Top = 56, Left = 16 };
                var lblMask = new Label { Text = "Subnet Mask", AutoSize = true, Top = 96, Left = 16 };
                var lblGw = new Label { Text = "Gateway (optional)", AutoSize = true, Top = 136, Left = 16 };

                _txtName.Left = 140; _txtName.Top = 12; _txtName.Text = name;
                _txtIp.Left = 140; _txtIp.Top = 52; _txtIp.Text = ip;
                _txtMask.Left = 140; _txtMask.Top = 92; _txtMask.Text = mask;
                _txtGw.Left = 140; _txtGw.Top = 132; _txtGw.Text = gw;

                var btnOk = new Button { Text = "Save", DialogResult = DialogResult.OK, Left = 140, Top = 172, Width = 90 };
                var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 240, Top = 172, Width = 90 };

                btnOk.Click += (_, __) =>
                {
                    // quick validation (more thorough validation happens when applying static IP)
                    if (string.IsNullOrWhiteSpace(_txtName.Text)) { MessageBox.Show("Preset name is required."); this.DialogResult = DialogResult.None; return; }
                    // Allow empty fields; user can complete later.
                };

                Controls.AddRange(new Control[] { lblName, lblIp, lblMask, lblGw, _txtName, _txtIp, _txtMask, _txtGw, btnOk, btnCancel });
                AcceptButton = btnOk;
                CancelButton = btnCancel;
            }
        }

        #endregion

        #region Control helpers

        private T? Find<T>(string name) where T : Control
            => Controls.Find(name, true).FirstOrDefault() as T;

        private void SetCheck(string name, bool value)
        {
            var cb = Find<CheckBox>(name);
            if (cb != null) cb.Checked = value;
        }

        private void SetNumeric(string name, int value)
        {
            var nud = Find<NumericUpDown>(name);
            if (nud != null)
            {
                if (value < nud.Minimum) value = (int)nud.Minimum;
                if (value > nud.Maximum) value = (int)nud.Maximum;
                nud.Value = value;
            }
        }

        #endregion
    }

    // Tiny extension to keep code tidy
    internal static class ControlExtensions
    {
        public static T Apply<T>(this T control, Action<T> action) where T : Control
        {
            action?.Invoke(control);
            return control;
        }
    }
}
