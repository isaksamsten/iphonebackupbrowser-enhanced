using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IDevice.IPhone;
using IDevice.Managers;
using System.IO;
using LevDan.Exif;

namespace IDevice.Plugins.Analyzers.Image
{
    public partial class ImageAnalyzer : Form
    {
        private IPhoneBackup _backup;
        private BackgroundWorker _worker;
        public ImageAnalyzer(IPhoneBackup backup)
        {
            _backup = backup;
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
            _worker.ProgressChanged += new ProgressChangedEventHandler(_worker_ProgressChanged);
            _worker.WorkerReportsProgress = true;

            InitializeComponent();

            if (_backup != null)
                this.Text += " [" + _backup.DisplayName + "]";
        }

        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var x = e.Result as dynamic;
            imageView.BeginInvoke(new MethodInvoker(delegate()
            {
                imageView.LargeImageList = x.ImageList;
                imageView.Items.AddRange(x.ListViewItems);
            }));
            progress.Visible = false;
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            IPhoneBackup backup = e.Argument as IPhoneBackup;

            IPhoneApp system = backup.GetApps().FirstOrDefault(app => app.Name == "System");
            IEnumerable<IPhoneFile> images = system.Files.Where(file => file.Domain == "MediaDomain" && file.Path.Contains("DCIM/100APPLE"));
            FileManager fm = new FileManager();

            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(64, 64);
            imgList.ColorDepth = ColorDepth.Depth32Bit;

            List<ListViewItem> listViewItems = new List<ListViewItem>();
            int length = images.Count(), current = 0;
            foreach (IPhoneFile file in images)
            {
                FileInfo info = fm.GetWorkingFile(_backup, file);
                ListViewItem itm = new ListViewItem();
                itm.Tag = new Bitmap(info.FullName);
                itm.Text = info.Name;
                itm.ImageKey = info.Name;

                imgList.Images.Add(info.Name, (Bitmap)itm.Tag);
                listViewItems.Add(itm);

                worker.ReportProgress(BrowserModel.Percent(current++, length));
            }

            e.Result = new { ImageList = imgList, ListViewItems = listViewItems.ToArray() };
        }

        private void ImageAnalyzer_Load(object sender, EventArgs e)
        {
            infoPanel.Columns.Add("Key", 100);
            infoPanel.Columns.Add("Value", 300);

            progress.Visible = true;
            _worker.RunWorkerAsync(_backup);
        }

        private void imageView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                infoPanel.Items.Clear();
                ListViewItem selected = e.Item;
                Bitmap image = selected.Tag as Bitmap;

                ExifTagCollection tags = new ExifTagCollection(image);
                foreach (ExifTag tag in tags)
                {
                    var item = new ListViewItem(tag.FieldName);
                    item.SubItems.Add(tag.Value);
                    infoPanel.Items.Add(item);
                }
            }
        }
    }
}
