using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using IDevice.Plugins;

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
            Application.Run(browser);
        }
    }
}
