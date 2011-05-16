using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDevice.Plugins;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using IDevice.IPhone;
using System.IO;

namespace IDevice.Plugins.Analyzers.SMS
{
    class SMSAnalyzerPlugin : AbstractPlugin
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ToolStripMenuItem start = new ToolStripMenuItem("SMS");
        public SMSAnalyzerPlugin()
        {
            start.Click += new EventHandler(start_Click);
        }

        void start_Click(object sender, EventArgs e)
        {
            IPhoneBackup backup = SelectedBackup;
            if (backup == null)
                return;

            IPhoneApp app = backup.GetApps().FirstOrDefault(t => t.Name == "System");
            IPhoneFile dbFile = app.Files.FirstOrDefault(t => t.Path.Contains("sms.db"));
            FileInfo fileInfo = FileManager.GetWorkingFile(backup, dbFile,true);

            SMSAnalyzer analyzer = new SMSAnalyzer(backup, fileInfo.FullName);
            analyzer.Show();
        }

        public override string Author
        {
            get { return "Karl Jansson"; }
        }

        public override string Description
        {
            get { return "Analyze the SMS content of this backup"; }
        }

        public override string Name
        {
            get { return "SMSAnalyzer"; }
        }

        public override Icon Icon
        {
            get { return null; }
        }

        protected override void OnRegisterMenu(Managers.MenuManager manager)
        {
            manager.Add(Managers.MenuContainer.Analyzer, start);
        }

        protected override void OnUnregisterMenu(Managers.MenuManager manager)
        {
            manager.Remove(Managers.MenuContainer.Analyzer, start);
        }
    }
}
