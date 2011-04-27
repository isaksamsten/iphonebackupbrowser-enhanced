using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDevice.IPhone;

namespace IDevice
{
    public class SelectionModel
    {
        public event EventHandler Changed;
        private BackupBrowser _browser;
        public SelectionModel(BackupBrowser browser)
        {
            browser.SelectedFiles += new EventHandler<IPhoneFileSelectedArgs>(browser_SelectedApp);
            browser.SelectedBackup += new EventHandler<IPhoneBackupSelectedArgs>(browser_SelectedBackup);
            browser.SelectedApps += new EventHandler<IPhoneAppSelectedArgs>(browser_SelectedApps);

            _browser = browser;
        }

        void browser_SelectedApps(object sender, IPhoneAppSelectedArgs e)
        {
            App = e.Selected;
            OnChanged();
        }

        void browser_SelectedBackup(object sender, IPhoneBackupSelectedArgs e)
        {
            Backup = e.Selected;
            OnChanged();
        }

        void browser_SelectedApp(object sender, IPhoneFileSelectedArgs e)
        {
            Files = e.Selected;
            OnChanged();
        }

        public void Select(IPhoneApp app)
        {
            _browser.SelectApp(app);
        }

        public void Select(IPhoneBackup backup)
        {
            _browser.SelectBackup(backup);
        }


        protected virtual void OnChanged()
        {
            if (Changed != null)
                Changed(this, EventArgs.Empty);
        }

        public IPhoneFile[] Files { get; private set; }

        public IPhoneBackup Backup { get; private set; }

        public IPhoneApp App { get; private set; }
    }
}
