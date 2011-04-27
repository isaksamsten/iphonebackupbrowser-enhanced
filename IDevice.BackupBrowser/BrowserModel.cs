using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDevice.IPhone;

namespace IDevice
{
    public class BrowserModel
    {
        public event EventHandler Changed;
        private BackupBrowser _browser;

        public IPhoneFile[] Files { get; private set; }

        public IPhoneBackup Backup { get; private set; }

        public IPhoneApp App { get; private set; }

        public BrowserModel(BackupBrowser browser)
        {
            browser.SelectedFiles += new EventHandler<IPhoneFileSelectedArgs>(browser_SelectedApp);
            browser.SelectedBackup += new EventHandler<IPhoneBackupSelectedArgs>(browser_SelectedBackup);
            browser.SelectedApps += new EventHandler<IPhoneAppSelectedArgs>(browser_SelectedApps);

            _browser = browser;
        }

        /// <summary>
        /// Select an iphone app in the gui
        /// </summary>
        /// <param name="app"></param>
        public void Select(IPhoneApp app)
        {
            _browser.SelectApp(app);
        }

        /// <summary>
        /// Select an iphone backup in the gui
        /// </summary>
        /// <param name="backup"></param>
        public void Select(IPhoneBackup backup)
        {
            _browser.SelectBackup(backup);
        }

        /// <summary>
        /// update the title of the main gui
        /// </summary>
        /// <param name="title"></param>
        public void UpdateTitle(string title)
        {
            _browser.UpdateTitle(title);
        }

        protected virtual void OnChanged()
        {
            if (Changed != null)
                Changed(this, EventArgs.Empty);
        }

        #region EVENTS

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

        #endregion
    }
}
