using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IDevice.IPhone;

namespace IDevice
{
    public partial class SelectBackupForm : Form
    {
        public SelectBackupForm(IPhoneBackup[] backups)
        {
            InitializeComponent();

            foreach (IPhoneBackup b in backups)
            {
                listBackups.Items.Add(b);
            }
        }

        /// <summary>
        /// Null if cancel
        /// </summary>
        public IPhoneBackup Selected { get; protected set; }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Selected = listBackups.SelectedItem as IPhoneBackup;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void listBackups_SelectedValueChanged(object sender, EventArgs e)
        {
            IPhoneBackup backup = listBackups.SelectedItem as IPhoneBackup;
            if (backup != null)
            {
                lblDate.Text = backup.LastBackupDate;
                lblDevice.Text = backup.DisplayName;
            }
        }

    }
}
