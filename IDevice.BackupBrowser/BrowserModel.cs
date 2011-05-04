using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDevice.IPhone;
using System.Windows.Forms;
using NLog;
using System.Threading;
using System.ComponentModel;

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
        /// Should run a background worker and update 
        /// the progress bar in the main gui with the progress
        /// of the current operation
        /// 
        /// Remember to be good and check for the cancelation!
        /// </summary>
        /// <param name="doWork"></param>
        /// <param name="completed"></param>
        /// <param name="arg"></param>
        /// <exception cref="Exception">Thrown if two operations at the same time...</exception>
        public void InvokeAsync(DoWorkEventHandler doWork, RunWorkerCompletedEventHandler completed, string name, bool canCancel = true, object arg = null)
        {
            Logger.Debug("Starting async work '{0}' and pushing progressbar", name);
            ProgressArgs args = _browser.PushProgress(name);
            ProgressBar bar = args.ProgressBar;
            Button cancel = args.Button;

            Logger.Debug("Work '{0}' was assigned key '{1}'", name, args.Key);
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = canCancel;

            cancel.Click += delegate(object sender, EventArgs e)
            {
                worker.CancelAsync();
                cancel.Enabled = false;
            };
            cancel.Enabled = canCancel;

            worker.DoWork += doWork;
            worker.RunWorkerCompleted += completed;
            worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                Logger.Debug("Work with key '{0}' is completed (Cancelled? {1})", args.Key, e.Cancelled);
                _browser.PopProgress(args);
            };

            worker.ProgressChanged += delegate(object sender, ProgressChangedEventArgs e)
            {
                bar.Value = e.ProgressPercentage;
            };

            worker.RunWorkerAsync(arg);
        }


        public void InvokeAsync<T>(IEnumerable<T> payload, Action<T> action, Action completed, string name, Cursor cursor = null)
        {
            Logger.Debug("Starting an async work on an Enumerable<{0}> with name '{1}'", typeof(T).Name, name);
            DoWorkEventHandler work = delegate(object sender, DoWorkEventArgs evt)
            {
                IEnumerable<T> items = evt.Argument as IEnumerable<T>;
                BackgroundWorker worker = sender as BackgroundWorker;
                int length = items.Count(), start = 0;
                foreach (T item in items)
                {
                    if (!worker.CancellationPending)
                    {
                        action(item);
                        worker.ReportProgress(Util.Percent(start++, length));
                    }
                    else
                    {
                        evt.Cancel = true;
                        break;
                    }
                }
            };

            RunWorkerCompletedEventHandler complete = delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                completed();
            };

            InvokeAsync(work, complete, name, true, payload);
        }

        public void InvokeAsync<T>(IEnumerable<T> payload, Action<T> action,string name, Cursor cursor = null)
        {
            InvokeAsync(payload, action, delegate() { }, name, cursor);
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
