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
using System.Data.SQLite;
using NLog;

namespace IDevice.Plugins.Analyzers.MMS
{
    public partial class MMSAnalyzer : Form
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IPhoneBackup _backup;
        private IPhoneFile _file;
        public MMSAnalyzer(IPhoneBackup backup, IPhoneFile file)
        {
            _backup = backup;
            _file = file;

            InitializeComponent();
        }

        private void MMSAnalyzer_Load(object sender, EventArgs e)
        {
            lstMms.Columns.Add("Address");
            lstMms.Columns.Add("Text");
            lstMms.Columns.Add("Content Type");
            lstMms.Columns.Add("Location");

            FileInfo info = FileManager.Current.GetWorkingFileCurrentClass(_backup, _file);
            using (SQLiteConnection con = new SQLiteConnection(@"Data Source=" + info.FullName))
            {
                try
                {
                    con.Open();
                    var cmd = new SQLiteCommand();
                    cmd.Connection = con;  //      0                1                2           3       4        5        
                    cmd.CommandText = "SELECT mp.message_id, mp.content_type, mp.content_loc, m.text, m.flags, m.address FROM message AS m INNER JOIN msg_pieces AS mp ON mp.message_id = m.ROWID;";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string pre = "FROM: ";
                        if (reader.GetInt64(4) == 3)
                            pre = "TO:   ";
                        ListViewItem itm = new ListViewItem(pre + reader.GetValue(5).ToString());
                        itm.SubItems.Add(reader.GetValue(3).ToString());
                        itm.SubItems.Add(reader.GetValue(1).ToString());
                        itm.SubItems.Add(new ListViewItem.ListViewSubItem
                        {
                            Text = reader.GetValue(2).ToString(),
                            Tag = reader.GetValue(0).ToString()
                        });
                        lstMms.Items.Add(itm);
                    }
                }
                catch (Exception ex) { Logger.ErrorException(ex.Message, ex); }
            }
        }

        private void lstMms_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ListViewItem.ListViewSubItem itm = e.Item.SubItems[3];
            if (itm != null)
            {
                if (!String.IsNullOrWhiteSpace(itm.Text))
                {
                    string rowId = itm.Tag.ToString();
                    string file = itm.Text;
                    IPhoneFile iFile = _backup.GetApps()
                        .FirstOrDefault(x => x.Name == "System")
                        .Files
                        .FirstOrDefault(y => y.Path.Contains(file) || y.Path.Contains(rowId));
                    Tuple<string, FileInfo> tuple = null;
                    if (iFile != null)
                    {
                        tuple = FileManager.Current.GetRandomFile(_backup, iFile);
                    }

                    if (tuple != null && (tuple.Item1.ToLower().Contains(".jpg") || tuple.Item1.ToLower().Contains(".jpeg")))
                    {
                        FileInfo info = tuple.Item2;
                        Bitmap image = new Bitmap(info.FullName);
                        picContainer.Tag = info;
                        picContainer.Image = image;
                    }
                    else
                    {
                        picContainer.Image = Properties.Resources.help;
                        picContainer.Tag = null;
                    }
                }
                else
                {
                    picContainer.Image = Properties.Resources.help;
                    picContainer.Tag = null;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (picContainer.Tag != null)
            {
                FileInfo info = picContainer.Tag as FileInfo;
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.FileName = lstMms.SelectedItems.Count > 0 ? lstMms.SelectedItems[0].SubItems[3].Text + ".jpg" : "mms.jpg";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.Copy(info.FullName, dialog.FileName);
                }
            }
        }
    }
}
