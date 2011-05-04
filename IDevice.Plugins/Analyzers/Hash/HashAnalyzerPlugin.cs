using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IDevice.IPhone;
using System.IO;

namespace IDevice.Plugins.Analyzers.Hash
{
    public class HashAnalyzerPlugin : AbstractPlugin
    {
        public override string Author
        {
            get { return "Isak Karlsson"; }
        }

        public override string Description
        {
            get
            {
                return @"List all files and their corresponding md5
                         and sha1 hash and export this to a cvs file";
            }
        }

        public override string Name
        {
            get { return "HashAnalyzerPlugin"; }
        }

        public override System.Drawing.Icon Icon
        {
            get { return null; }
        }

        private ToolStripMenuItem hashes = new ToolStripMenuItem("Hashes");

        public HashAnalyzerPlugin()
        {
            hashes.Click += new EventHandler(hashes_Click);
        }

        void hashes_Click(object sender, EventArgs e)
        {
            IPhoneBackup backup = SelectedBackup;
            if (backup == null)
                return;
            IEnumerable<IPhoneApp> apps = backup.GetApps();
            if (apps == null)
                return;

            IEnumerable<IPhoneFile> files = apps.SelectMany(x => x.Files != null ? x.Files : new List<IPhoneFile>());

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Row,DateTime,MD5,SHA1,DisplayName,FileName");
            int row = 0;
            Model.InvokeAsync(files.Take(10), delegate(IPhoneFile file) // run
            {
                FileInfo fileInfo = FileManager.GetOriginalFile(backup, file);
                string md5 = Util.MD5File(fileInfo);
                string sha1 = Util.SHA1File(fileInfo);

                builder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", 
                    row++, 
                    DateTime.Now.ToUniversalTime(), 
                    md5, 
                    sha1, 
                    file.Path, 
                    fileInfo.FullName));
            }, delegate() // Called when completed
            {
                SaveFileDialog fileSaver = new SaveFileDialog();
                fileSaver.AddExtension = true;
                fileSaver.DefaultExt = ".csv";
                fileSaver.Filter = "Comma separated file|*.csv";

                if (fileSaver.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(fileSaver.FileName, builder.ToString());
                }

            }, "Analyzing hashes");
        }

        protected override void OnRegisterMenu(Managers.MenuManager manager)
        {
            manager.Add(Managers.MenuContainer.Analyzer, hashes);
        }

        protected override void OnUnregisterMenu(Managers.MenuManager manager)
        {
            manager.Remove(Managers.MenuContainer.Analyzer, hashes);
        }


    }
}
