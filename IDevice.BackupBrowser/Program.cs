using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using IDevice.Plugins;
using IDevice.Managers;
using NLog;

namespace IDevice
{
    static class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                BackupBrowser browser = new BackupBrowser();
                Application.Run(browser);
            }
            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                FileManager.Current.Clean();
            }
        }
    }
}
