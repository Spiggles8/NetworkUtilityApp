using System;
using System.Windows;

namespace NetworkUtilityApp
{
    internal static class Startup
    {
        [STAThread]
        private static void Main()
        {
            var app = new NetworkUtilityApp.App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
