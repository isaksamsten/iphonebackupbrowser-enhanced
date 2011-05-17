using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IDevice.IPhone;

namespace IDevice.Plugins.Analyzers.MMS
{
    public class MMSAnalyzerPlugin :AbstractPlugin
    {
        private ToolStripMenuItem show = new ToolStripMenuItem("MMS");
        public MMSAnalyzerPlugin()
        {
            show.Click += new EventHandler(show_Click);
        }

        void show_Click(object sender, EventArgs e)
        {
            IPhoneBackup backup = SelectedBackup;
            IPhoneApp app = backup.GetApps().FirstOrDefault(t => t.Name == "System");
            IPhoneFile dbFile = app.Files.FirstOrDefault(t => t.Path.Contains("sms.db"));
            MMSAnalyzer mmsAnalyzer = new MMSAnalyzer(backup, dbFile);
            mmsAnalyzer.Show();
        }

        public override string Author
        {
            get { return "Isak Karlsson"; }
        }

        public override string Description
        {
            get { return "Show MMS:es"; }
        }

        public override string Name
        {
            get { return "MMSAnalyzerPlugin"; }
        }

        public override System.Drawing.Icon Icon
        {
            get { return null; }
        }

        protected override void OnRegisterMenu(Managers.MenuManager manager)
        {
            manager.Add(Managers.MenuContainer.Analyzer, show);
        }

        protected override void OnUnregisterMenu(Managers.MenuManager manager)
        {
            manager.Remove(Managers.MenuContainer.Analyzer, show);
        }
    }
}
