using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IDevice.IPhone;
using System.IO;
using System.ComponentModel;

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
        private ToolStripSeparator sep = new ToolStripSeparator();
        private ToolStripMenuItem showHash = new ToolStripMenuItem("Show hash");

        public HashAnalyzerPlugin()
        {
            hashes.Click += new EventHandler(hashes_Click);
            
            showHash.Click += new EventHandler(showHash_Click);
            
            // name is used for sorting
            hashes.Name = "A";
            sep.Name = "B";
            showHash.Name = "C";
        }

        void showHash_Click(object sender, EventArgs e)
        {
            Model.InvokeAsync(delegate(object s, DoWorkEventArgs arg)
            {
                var data = arg.Argument as dynamic;
                HashInfo info = new HashInfo(data.Backup, data.File);
                arg.Result = info;

            }, delegate(object s, RunWorkerCompletedEventArgs arg)
            {
                if (!arg.Cancelled)
                {
                    Form info = arg.Result as Form;
                    info.Show(Model.Window);
                }

            }, "Hashing...", true, new { Backup = SelectedBackup, File = SelectedFiles.FirstOrDefault()});

        }

        void hashes_Click(object sender, EventArgs e)
        {
            hashes.Enabled = false;
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
            Model.InvokeAsync(files, delegate(IPhoneFile file) // run
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

                hashes.Enabled = true;
            }, "Analyzing hashes");
        }

        protected override void OnSelectionChanged(object sender, EventArgs e)
        {
            if (SelectedFiles != null && SelectedFiles.Count() > 0)
                showHash.Enabled = true;
            else
                showHash.Enabled = false;
        }

        protected override void OnRegisterMenu(Managers.MenuManager manager)
        {
            manager.Add(Managers.MenuContainer.Analyzer, hashes);
            manager.Add(Managers.MenuContainer.FileContext, showHash);
            manager.Add(Managers.MenuContainer.FileContext, sep);
        }

        protected override void OnUnregisterMenu(Managers.MenuManager manager)
        {
            manager.Remove(Managers.MenuContainer.Analyzer, hashes);
            manager.Remove(Managers.MenuContainer.Analyzer, showHash);
            manager.Remove(Managers.MenuContainer.Analyzer, sep);
        }


    }
}
