using NetworkUtilityApp.Services;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NetworkUtilityApp
{
    /// <summary>
    /// Main application window hosting all tabs and a single global output log.
    /// Subscribes to <see cref="AppLog"/> to mirror log entries into the UI.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Initializes the form, wires global log button handlers, and subscribes to AppLog updates.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            AppLog.EntryAdded += OnAppLogEntryAdded;

            // Wire clear / save buttons (if present in designer)
            if (btnGlobalLogClear is not null)
                btnGlobalLogClear.Click += (_, __) => OnClearLog();
            if (btnGlobalLogSave is not null)
                btnGlobalLogSave.Click += (_, __) => OnSaveLog();
        }

        /// <summary>
        /// Form load handler. Seeds the global log textbox from the existing log snapshot
        /// and runs any required per-tab initialization.
        /// </summary>
        private void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
                // Render any existing AppLog entries to the global log UI on startup.
                foreach (var e1 in AppLog.Snapshot())
                    AppendToGlobalLog(e1.ToString());

                // Perform per-tab on-load work (adapters grid refresh, etc.)
                tabNetworkAdapters?.Initialize();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize form.\n\n" + ex.Message,
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// AppLog event callback. Marshals to UI thread if required and appends
        /// the new entry to the global log textbox.
        /// </summary>
        private void OnAppLogEntryAdded(object? sender, AppLog.LogEntry e)
        {
            if (IsDisposed) return;

            // Ensure we update the textbox on the UI thread.
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object?, AppLog.LogEntry>(OnAppLogEntryAdded), sender, e);
                return;
            }
            AppendToGlobalLog(e.ToString());
        }

        /// <summary>
        /// Appends a single line to the global log textbox and scrolls to the end.
        /// </summary>
        private void AppendToGlobalLog(string line)
        {
            if (txtGlobalLog is null) return;

            if (txtGlobalLog.TextLength == 0) txtGlobalLog.Text = line;
            else txtGlobalLog.AppendText(Environment.NewLine + line);

            txtGlobalLog.SelectionStart = txtGlobalLog.TextLength;
            txtGlobalLog.ScrollToCaret(); // keep newest entries in view
        }

        /// <summary>
        /// Clears the AppLog and the UI textbox, then shows the synthetic clear entry.
        /// </summary>
        private void OnClearLog()
        {
            AppLog.Clear();        // clears underlying list + emits "(log cleared)"
            txtGlobalLog?.Clear(); // clear UI textbox

            // Re-add the synthetic clear entry from snapshot (optional – already added by event)
            var snapshot = AppLog.Snapshot();
            var last = snapshot.Count > 0 ? snapshot[snapshot.Count - 1] : null;
            if (last is not null) AppendToGlobalLog(last.ToString());
        }

        /// <summary>
        /// Saves the current AppLog snapshot to a text file using a Save File dialog.
        /// Also logs the destination path on success.
        /// </summary>
        private void OnSaveLog()
        {
            try
            {
                using var dlg = new SaveFileDialog
                {
                    Title = "Save Log As",
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FileName = $"NetworkUtilityLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    OverwritePrompt = true
                };

                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                var snapshot = AppLog.Snapshot();
                var lines = new string[snapshot.Count];
                for (int i = 0; i < snapshot.Count; i++)
                    lines[i] = snapshot[i].ToString();

                File.WriteAllLines(dlg.FileName, lines);

                AppLog.Info($"Log saved to: {dlg.FileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save log.\n\n" + ex.Message, "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}