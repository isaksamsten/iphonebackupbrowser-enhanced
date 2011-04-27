using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDevice.IPhone;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace IDevice.Plugins.Browsers.SQL
{
    public class SQLiteBrowserPlugin : AbstractPlugin, IBrowsable
    {

        public bool Modal
        {
            get
            {
                return false;
            }
        }

        public override string Author
        {
            get
            {
                return "Magnus Wahlgren";
            }
        }

        public override string Description
        {
            get
            {
                return "Simple db browser";
            }
        }

        public override string Name
        {
            get
            {
                return "SQLiteBrowser";
            }
        }

        public override Icon Icon
        {
            get { return null; }
        }

        public Form Open()
        {
            IPhoneFile file = SelectedFiles.FirstOrDefault();
            if (file != null)
            {
                FileInfo path = FileManager.GetWorkingFile(SelectedBackup, file, true);
                return new SQLiteBrowser(path.FullName);
            }

            return null;
        }

        public string[] Prefixes
        {
            get { return new string[] { ".db", ".sqlite", ".sqlitedb" }; }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
