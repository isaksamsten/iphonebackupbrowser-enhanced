using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDevice.IPhone;
using System.Windows.Forms;
using NLog;

namespace IDevice
{
    public delegate bool TaskDispatcher<T>(T itm);

    public class BrowserModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public event EventHandler Changed;
        private BackupBrowser _browser;

        /// <summary>
        /// Same as App.Files in most cases
        /// </summary>
        public IPhoneFile[] Files { get; private set; }

        /// <summary>
        /// Get the backup
        /// </summary>
        public IPhoneBackup Backup { get; private set; }

        /// <summary>
        /// same as Backup.GetApps
        /// </summary>
        public IPhoneApp App { get; private set; }

        public IWin32Window Window { get { return _browser; } }

        public BrowserModel(BackupBrowser browser)
        {
            browser.SelectedFiles += new EventHandler<IPhoneFileSelectedArgs>(browser_SelectedApp);
            browser.SelectedBackup += new EventHandler<IPhoneBackupSelectedArgs>(browser_SelectedBackup);
            browser.SelectedApps += new EventHandler<IPhoneAppSelectedArgs>(browser_SelectedApps);

            _browser = browser;
        }

        /// <summary>
        /// Run long running list like this. It will 
        /// show an indicator to the user that the program 
        /// is working
        /// 
        /// <code>
        ///     Model.Dispatch(list, delegate(T itm) {
        ///         //DoThingsHere
        ///         
        ///         return true; // false will break the task
        ///     });
        /// </code>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="task"></param>
        public void Dispatch<T>(IEnumerable<T> list, TaskDispatcher<T> task, Cursor cursor = null)
        {
            _browser.ProgressBar.Visible = true;
            _browser.ProgressBar.Maximum = list.Count();
            _browser.ProgressBar.Step = 1;
            if (cursor != null)
                Cursor.Current = cursor;
            try
            {
                foreach (T itm in list)
                {
                    if (task(itm))
                    {
                        _browser.ProgressBar.Value++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);
            }
            finally
            {
                _browser.ProgressBar.Visible = false;
                _browser.ProgressBar.Value = 0;
                Cursor.Current = Cursors.Default;
            }

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
