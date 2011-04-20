using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PList;
using System.IO;
using System.Xml;

namespace IDevice.Plugins.Browsers.PList
{
    public partial class PListBrowser : AbstractBrowsable
    {
        public PListBrowser()
            : base(".plist")
        {
            InitializeComponent();
        }

        public override Form Initialize(System.IO.FileInfo file)
        {
            return Initialize(file.FullName);
        }

        public override Form Initialize(string path)
        {
            plistList.Clear();
            PListRoot root = PListRoot.Load(path);
            PopulateRecurse(root.Root as PListDict, "");

            return this;
        }

        private void PopulateRecurse(PListDict dict, string space)
        {
            foreach (var p in dict)
            {
                ListViewItem itm = new ListViewItem();
                itm.Tag = p.Value;
                itm.Text = space + p.Key;
                itm.SubItems.Add(p.Value.ToString());
                plistList.Items.Add(itm);

                if (p.Value is PListDict)
                    PopulateRecurse(p.Value as PListDict, space + " ");
            }
        }

        private void PListBrowser_Load(object sender, EventArgs e)
        {
            plistList.Columns.Add("Key", 100);
            plistList.Columns.Add("Value", 300);
        }
    }
}
