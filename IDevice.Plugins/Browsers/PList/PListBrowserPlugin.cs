using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDevice.IPhone;
using System.IO;
using PList;
using System.Windows.Forms;
using System.Drawing;

namespace IDevice.Plugins.Browsers.PList
{
    public class PListBrowserPlugin : AbstractPlugin, IBrowsable
    {
        public Form Open()
        {
            IPhoneFile file = SelectedFiles.FirstOrDefault();
            if (file != null)
            {
                FileInfo info = FileManager.GetWorkingFile(SelectedBackup, file, true);
                PListRoot root = PListRoot.Load(info.FullName);
                return new PListBrowser(root);
            }

            return null;
        }

        public override string Author
        {
            get
            {
                return "Isak Karlsson";
            }
        }

        public override string Name
        {
            get
            {
                return "PListBrowser";
            }
        }

        public override string Description
        {
            get
            {
                return "Browse plist files";
            }
        }

        public override Icon Icon
        {
            get { return null; }
        }

        public string[] Prefixes
        {
            get { return new string[] { ".plist" }; }
        }
    }
}
