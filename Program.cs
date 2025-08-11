using System;
using System.Threading;
using System.Windows.Forms;

namespace NetworkUtilityApp
{
    internal static class Program
    {
        // Flip to true if you want to enforce single-instance later.
        private const bool USE_SINGLE_INSTANCE = false;

        [STAThread]
        private static void Main()
        {
            // High DPI + classic WinForms look
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            // Global exception handling (UI + non-UI)
            Application.ThreadException += (sender, args) =>
            {
                ShowFatal("An unexpected error occurred (UI thread).", args.Exception);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                ShowFatal("An unexpected error occurred.", ex);
            };

            // Start the app
            Application.Run(new Form1());
        }

        private static void ShowFatal(string message, Exception? ex)
        {
            try
            {
                var details = ex == null ? "" : $"\n\n{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
                MessageBox.Show($"{message}{details}", "Network Utility - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // Last resort: swallow to avoid recursive crashes
            }
        }
    }
}
