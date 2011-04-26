using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace IDevice.Plugins.Browsers
{
    public class DefaultBrowser : AbstractPlugin, IBrowsable
    {
        public string[] Prefixes
        {
            get { return new string[] { "*" }; }
        }

        public override Form Open()
        {
            FileInfo info = FileManager.GetWorkingFile(SelectedBackup, SelectedFiles.FirstOrDefault());
            Process.Start(info.FullName).Start();
            return null;
        }

        public override string PluginAuthor
        {
            get { return "Isak Karlsson"; }
        }

        public override string PluginDescription
        {
            get { return "Default handler. Handle using Operating system."; }
        }

        public override string PluginName
        {
            get { return "DefaultBrowser"; }
        }
    }
}