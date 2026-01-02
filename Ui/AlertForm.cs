using System.Drawing;
using System.Windows.Forms;

namespace NetworkUtilityApp.Ui
{
    internal sealed class AlertForm : Form
    {
        public static void ShowError(IWin32Window? owner, string message, string title, bool darkMode)
        {
            using var dlg = new AlertForm(message, title, darkMode);
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ShowInTaskbar = false;
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false;
            dlg.MinimizeBox = false;
            dlg.AcceptButton = dlg._ok;
            if (owner != null) dlg.ShowDialog(owner); else dlg.ShowDialog();
        }

        private readonly Label _label;
        internal readonly Button _ok;

        private AlertForm(string message, string title, bool darkMode)
        {
            Text = title;
            Width = 420;
            Height = 180;

            var back = darkMode ? Color.FromArgb(30,30,30) : SystemColors.Window;
            var fore = darkMode ? Color.FromArgb(230,230,230) : SystemColors.WindowText;
            BackColor = back;
            ForeColor = fore;

            _label = new Label
            {
                AutoSize = true,
                Text = message,
                Left = 12,
                Top = 12,
                MaximumSize = new Size(380, 0),
                ForeColor = fore,
                BackColor = Color.Transparent
            };

            _ok = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Width = 80,
                Height = 28,
                Left = 320,
                Top = _label.Bottom + 20,
                BackColor = darkMode ? Color.FromArgb(20,111,20) : Color.FromArgb(20,111,20),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Standard
            };

            Height = _ok.Bottom + 60;

            Controls.Add(_label);
            Controls.Add(_ok);
        }
    }
}
