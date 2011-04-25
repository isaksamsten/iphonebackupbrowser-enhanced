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
using IDevice.IPhone;

using IDevice;

namespace IDevice.Plugins.Browsers.PList
{
    public partial class PListBrowser : AbstractBrowsable
    {
        public PListBrowser()
            : base(".plist")
        {
            InitializeComponent();
        }

        protected override void PreOpen()
        {
            plistList.Clear();
            IPhoneFile file = SelectedFiles.FirstOrDefault();
            if (file != null)
            {
                FileInfo info = FileManager.GetWorkingFile(SelectedBackup, file);
                PListRoot root = PListRoot.Load(info.FullName);
                PopulateRecurse(root.Root as PListDict, "");
            }
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
                else if (p.Value is PListArray)
                    PopulateArray(p.Value as PListArray, space + " ");
            }
        }

        private void PopulateArray(PListArray pListArray, string space)
        {
            int lvl = 0;
            foreach (var p in pListArray)
            {
                ListViewItem itm = new ListViewItem();
                itm.Tag = p;
                itm.Text = space + lvl++;
                itm.SubItems.Add(p.ToString());
                plistList.Items.Add(itm);

                if (p is PListDict)
                    PopulateRecurse(p as PListDict, space + " ");
                else if (p is PListArray)
                    PopulateArray(p as PListArray, space + " ");
            }
        }

        private void PListBrowser_Load(object sender, EventArgs e)
        {
            plistList.Columns.Add("Key", 100);
            plistList.Columns.Add("Value", 300);
        }

        public override string PluginAuthor
        {
            get
            {
                return "Isak Karlsson";
            }
        }

        public override string PluginName
        {
            get
            {
                return "PListBrowser";
            }
        }

        public override string PluginDescription
        {
            get
            {
                return "Browse plist files";
            }
        }

        private void plistList_SelectedIndexChanged(object sender, EventArgs e)
        {
            IPListElement element = plistList.FocusedItem.Tag as IPListElement;

            lblHead.Text = element.TypeName();

        }
    }
}
