using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDevice.IPhone;

namespace IDevice
{
    public class SelectionModel
    {
        private BackupBrowser _browser;
        public SelectionModel(BackupBrowser browser)
        {
            browser.SelectedFiles += new EventHandler<IPhoneFileSelectedArgs>(browser_SelectedApp);
            browser.SelectedBackup += new EventHandler<IPhoneBackupSelectedArgs>(browser_SelectedBackup);
        }

        void browser_SelectedBackup(object sender, IPhoneBackupSelectedArgs e)
        {
            Backup = e.Selected;
        }

        void browser_SelectedApp(object sender, IPhoneFileSelectedArgs e)
        {
            Files = e.Selected;
        }

        public IPhoneFile[] Files
        {
            get;
            private set;
        }
        
        public IPhoneBackup Backup { get; private set; }
    }
}
