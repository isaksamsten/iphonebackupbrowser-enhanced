using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace IDevice
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            BackupBrowser browser = new BackupBrowser();
            // Run bootstrapper logic
            foreach (string s in Properties.Settings.Default.Browsers)
            {
                BrowseHandler.Current.Register(s);
            }

            Application.Run(browser);
        }
    }
}
