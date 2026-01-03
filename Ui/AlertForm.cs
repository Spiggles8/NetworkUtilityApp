namespace NetworkUtilityApp.Ui
{
    /// <summary>
    /// Small, themed alert dialog used for errors and important messages.
    ///
    /// The form is created on demand with optional dark mode styling and a
    /// single OK button. It avoids designer usage to keep dependencies low.
    /// </summary>
    internal sealed class AlertForm : Form
    {
        /// <summary>
        /// Show a simple modal error/alert dialog.
        /// </summary>
        /// <param name="owner">Optional owner window for centering.</param>
        /// <param name="message">Body text to display.</param>
        /// <param name="title">Caption/title for the window.</param>
        /// <param name="darkMode">Whether to use dark theme colors.</param>
        public static void ShowError(IWin32Window? owner, string message, string title, bool darkMode)
        {
            using var dlg = new AlertForm(message, title, darkMode);
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ShowInTaskbar = false;
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false;
            dlg.MinimizeBox = false;
            dlg.AcceptButton = dlg._ok; // Enter key activates OK

            if (owner != null)
                dlg.ShowDialog(owner);
            else
                dlg.ShowDialog();
        }

        // Message label and OK button kept as fields so their properties
        // can be used when sizing / configuring the dialog.
        private readonly Label _label;
        internal readonly Button _ok;

        private AlertForm(string message, string title, bool darkMode)
        {
            Text = title;
            Width = 420;
            Height = 180; // temporary; adjusted after controls are placed

            // Pick base colors according to theme
            var back = darkMode ? Color.FromArgb(30, 30, 30) : SystemColors.Window;
            var fore = darkMode ? Color.FromArgb(230, 230, 230) : SystemColors.WindowText;
            BackColor = back;
            ForeColor = fore;

            // Auto-sized label constrained to a max width so long messages wrap
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

            // OK button styled as a subtle primary action
            _ok = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Width = 80,
                Height = 28,
                Left = 320,
                Top = _label.Bottom + 20,
                BackColor = darkMode ? Color.FromArgb(20, 111, 20) : Color.FromArgb(20, 111, 20),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Standard
            };

            // Adjust overall height so there is padding below the button
            Height = _ok.Bottom + 60;

            Controls.Add(_label);
            Controls.Add(_ok);
        }
    }
}
