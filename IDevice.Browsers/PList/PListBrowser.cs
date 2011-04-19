﻿using System;
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

namespace IDevice.Browsers.PList
{
    public partial class PListBrowser : Form, IBrowsable
    {
        public PListBrowser()
        {
            InitializeComponent();
        }

        public Form Initialize(System.IO.FileInfo file)
        {
            return Initialize(file.FullName);
        }

        public Form Initialize(string path)
        {
            PListRoot root = PListRoot.Load(path);
            foreach (var p in root.Root as PListDict)
            {
                ListViewItem itm = new ListViewItem();
                itm.Tag = p.Key;
                itm.Text = p.Key;
                itm.SubItems.Add(p.Key.ToString());
                itm.SubItems.Add(p.Value.ToString());

                plistList.Items.Add(itm);
            }

            return this;
        }

        private void PListBrowser_Load(object sender, EventArgs e)
        {
            plistList.Columns.Add("Key", 100);
            plistList.Columns.Add("Value", 300);
        }
    }
}