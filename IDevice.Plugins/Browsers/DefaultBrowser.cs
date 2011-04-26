using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;

namespace IDevice.Plugins.Browsers
{
    public class DefaultBrowser : AbstractPlugin, IBrowsable
    {
        public override Form Open()
        {
            FileInfo info = FileManager.GetWorkingFile(SelectedBackup, SelectedFiles.FirstOrDefault());
            Process.Start(info.FullName).Start();
            return null;
        }

        public string[] Prefixes
        {
            get { return new string[] { "*" }; }
        }

        public override string Author
        {
            get { return "Isak Karlsson"; }
        }

        public override string Description
        {
            get { return "Default handler. Handle using Operating system."; }
        }

        public override string Name
        {
            get { return "DefaultBrowser"; }
        }

        public override Icon Icon
        {
            get { return null; }
        }
    }
}