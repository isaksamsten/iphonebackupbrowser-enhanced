using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IDevice.IPhone;
using System.IO;
using IDevice.Managers;

namespace IDevice.Plugins.Analyzers.Hash
{
    public partial class HashInfo : Form
    {
        public HashInfo(IPhoneBackup backup, IPhoneFile file)
        {
            InitializeComponent();

            FileInfo fileInfo = FileManager.Current.GetOriginalFile(backup, file);
            Text = "Hash information [" + file.Path + "]";
            lblFileName.Text = file.Path;
            lblMd5.Text = Util.MD5File(fileInfo);
            lblSha1.Text = Util.SHA1File(fileInfo);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void label_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                Label label = sender as Label;
                Clipboard.SetText(lblMd5.Text);
            }
        }
    }
}
