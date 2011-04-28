// convertir DTD -> XSD : http://www.hitsw.com/xml_utilites/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

using mbdbdump;
using PList;

using IDevice;
using IDevice.IPhone;
using IDevice.Plugins;
using IDevice.Managers;
using NLog;
using IDevice.Settings;

namespace IDevice
{
    public partial class BackupBrowser : Form
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public event EventHandler<IPhoneFileSelectedArgs> SelectedFiles;
        public event EventHandler<IPhoneBackupSelectedArgs> SelectedBackup;
        public event EventHandler<IPhoneAppSelectedArgs> SelectedApps;

        public BrowserModel Model { get { return _model; } }

        /// <summary>
        /// Cette classe est une implémentation de l'interface 'IComparer'.
        /// </summary>
        public class ListViewColumnSorter : IComparer
        {
            /// <summary>
            /// Spécifie la colonne à trier
            /// </summary>
            private int ColumnToSort;
            /// <summary>
            /// Spécifie l'ordre de tri (en d'autres termes 'Croissant').
            /// </summary>
            private SortOrder OrderOfSort;
            /// <summary>
            /// Objet de comparaison ne respectant pas les majuscules et minuscules
            /// </summary>
            private CaseInsensitiveComparer ObjectCompare;

            /// <summary>
            /// Constructeur de classe.  Initializes various elements
            /// </summary>
            public ListViewColumnSorter()
            {
                // Initialise la colonne sur '0'
                ColumnToSort = 0;

                // Initialise l'ordre de tri sur 'aucun'
                OrderOfSort = SortOrder.None;

                // Initialise l'objet CaseInsensitiveComparer
                ObjectCompare = new CaseInsensitiveComparer();
            }

            /// <summary>
            /// Cette méthode est héritée de l'interface IComparer.  Il compare les deux objets passés en effectuant une comparaison 
            ///qui ne tient pas compte des majuscules et des minuscules.
            /// </summary>
            /// <param name="x">Premier objet à comparer</param>
            /// <param name="x">Deuxième objet à comparer</param>
            /// <returns>Le résultat de la comparaison. "0" si équivalent, négatif si 'x' est inférieur à 'y' 
            ///et positif si 'x' est supérieur à 'y'</returns>
            public int Compare(object x, object y)
            {
                int compareResult;
                ListViewItem listviewX, listviewY;

                // Envoit les objets à comparer aux objets ListViewItem
                listviewX = (ListViewItem)x;
                listviewY = (ListViewItem)y;

                // Compare les deux éléments
                compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

                // Calcule la valeur correcte d'après la comparaison d'objets
                if (OrderOfSort == SortOrder.Ascending)
                {
                    // Le tri croissant est sélectionné, renvoie des résultats normaux de comparaison
                    return compareResult;
                }
                else if (OrderOfSort == SortOrder.Descending)
                {
                    // Le tri décroissant est sélectionné, renvoie des résultats négatifs de comparaison
                    return (-compareResult);
                }
                else
                {
                    // Renvoie '0' pour indiquer qu'ils sont égaux
                    return 0;
                }
            }

            /// <summary>
            /// Obtient ou définit le numéro de la colonne à laquelle appliquer l'opération de tri (par défaut sur '0').
            /// </summary>
            public int SortColumn
            {
                set
                {
                    ColumnToSort = value;
                }
                get
                {
                    return ColumnToSort;
                }
            }

            /// <summary>
            /// Obtient ou définit l'ordre de tri à appliquer (par exemple, 'croissant' ou 'décroissant').
            /// </summary>
            public SortOrder Order
            {
                set
                {
                    OrderOfSort = value;
                }
                get
                {
                    return OrderOfSort;
                }
            }

        }
        private ListViewColumnSorter lvwColumnSorter;
        private BrowserManager _browserManger;
        private PluginManager _pluginManager;
        private BrowserModel _model;
        private MenuManager _menuManager;

        public BackupBrowser()
        {
            InitializeComponent();

            _pluginManager = new PluginManager();

            _pluginManager.Added += new EventHandler<PluginArgs>(_pluginManager_Added);
            _pluginManager.Removed += new EventHandler<PluginArgs>(_pluginManager_Removed);

            _browserManger = new BrowserManager(_pluginManager);

            _menuManager = new MenuManager();
            _menuManager.Added += new EventHandler<MenuEvent>(_menuManager_Added);
            _menuManager.Removed += new EventHandler<MenuEvent>(_menuManager_Removed);

            _model = new BrowserModel(this);

            //init and load all plugins that is not blacklisted
            foreach (IPlugin p in _pluginManager)
            {
                Register(p);
            }
        }

        void _menuManager_Removed(object sender, MenuEvent e)
        {
            ToolStripItemCollection collection = GetMenuCollection(e.At);
            if (collection == null)
            {
                ToolStripItem[] coll = mainMenu.Items.Find(e.Name, true);
                if (coll.Length == 1)
                    (coll.FirstOrDefault() as ToolStripMenuItem).DropDownItems.Remove(e.Item);
            }
            else
            {
                collection.Remove(e.Item);
            }
        }

        void _menuManager_Added(object sender, MenuEvent e)
        {
            ToolStripItemCollection collection = GetMenuCollection(e.At);
            if (collection == null)
            {
                ToolStripItem[] coll = mainMenu.Items.Find(e.Name, true);
                if (coll.Length == 1)
                    (coll.FirstOrDefault() as ToolStripMenuItem).DropDownItems.Add(e.Item);
            }
            else
            {
                collection.Add(e.Item);
            }
        }

        protected virtual ToolStripItemCollection GetMenuCollection(MenuContainer at)
        {
            switch (at)
            {
                case MenuContainer.Base:
                    return mainMenu.Items;
                case MenuContainer.File:
                    return fileToolStripMenuItem.DropDownItems;
                case MenuContainer.Edit:
                    return editToolStripMenuItem.DropDownItems;
                case MenuContainer.View:
                    return viewToolStripMenuItem.DropDownItems;
                case MenuContainer.Analyzer:
                    return analyzeToolStripMenuItem.DropDownItems;
                case MenuContainer.FolderContext:
                    return folderContextMenu.Items;
                case MenuContainer.FileContext:
                    return fileContextMenu.Items;
                default:
                    return null;
            }
        }

        protected virtual void Register(IPlugin p)
        {
            Logger.Debug("Register '{0}'", p.Name);
            try
            {
                IRegisterArgs args = new RegisterArgs(_menuManager, Model);
                p.Register(args);
            }
            catch (Exception ex)
            {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        protected virtual void Unregister(IPlugin p)
        {
            Logger.Debug("Unregister '{0}'", p.Name);
            try
            {
                IRegisterArgs args = new RegisterArgs(_menuManager, Model);
                p.Unregister(args);


                p.Dispose(); // clean!
            }
            catch (Exception ex)
            {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        void _pluginManager_Removed(object sender, PluginArgs e)
        {
            Unregister(e.Plugin);
        }

        void _pluginManager_Added(object sender, PluginArgs e)
        {
            Register(e.Plugin);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            folderList.Columns.Add("Display Name", 200);
            folderList.Columns.Add("Files");

            fileList.Columns.Add("Name", 300);
            fileList.Columns.Add("Size");
            fileList.Columns.Add("Date", 130);
            fileList.Columns.Add("Domain", 300);
            fileList.Columns.Add("Key", 250);

            lvwColumnSorter = new ListViewColumnSorter();
            fileList.ListViewItemSorter = lvwColumnSorter;
        }

        private void folderList_DoubleClick(object sender, EventArgs e)
        {
            IPhoneApp app = (IPhoneApp)folderList.FocusedItem.Tag;
            SelectApp(app);
        }

        private void folderList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            switch (folderList.Sorting)
            {
                case SortOrder.None: folderList.Sorting = SortOrder.Ascending; break;
                case SortOrder.Ascending: folderList.Sorting = SortOrder.Descending; break;
                case SortOrder.Descending: folderList.Sorting = SortOrder.None; break;
            }
        }


        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }
            this.fileList.Sort();
        }

        private void fileList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            IPhoneBackup backup = Model.Backup;
            List<IPhoneFile> files = new List<IPhoneFile>();
            foreach (ListViewItem itm in fileList.SelectedItems)
                files.Add(itm.Tag as IPhoneFile);

            string path = Path.GetTempPath();
            List<string> filenames = new List<string>();
            Model.InvokeAsync(files, delegate(IPhoneFile file)
            {
                string source = Path.Combine(backup.Path, file.Key);
                string dest = Path.Combine(path, file.Path.Replace("/", Path.DirectorySeparatorChar.ToString()));

                // If there is a folder structure
                // create it.
                int lastIndex = file.Path.LastIndexOf("/");
                if (lastIndex >= 0)
                {
                    string fileFolder = file.Path.Substring(0, lastIndex);
                    Directory.CreateDirectory(Path.Combine(path, fileFolder.Replace("/", Path.DirectorySeparatorChar.ToString())));
                }

                // Copy the file (overwrite)
                File.Copy(source, dest, true);
                filenames.Add(dest);
            }, "Export...");
            fileList.DoDragDrop(new DataObject(DataFormats.FileDrop, filenames.ToArray()), DragDropEffects.Copy);
        }

        private void fileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (fileList.SelectedItems.Count == 1)
            {
                toolShowBtn.Enabled = true;
                toolExportBtn.Enabled = true;

                exportMenu.Enabled = true;
                showMenu.Enabled = true;
            }
            else if (fileList.SelectedItems.Count > 1)
            {
                toolShowBtn.Enabled = false;
                showMenu.Enabled = false;

                toolExportBtn.Enabled = true;
                exportMenu.Enabled = true;
            }
            else
            {
                toolShowBtn.Enabled = false;
                toolExportBtn.Enabled = false;
                showMenu.Enabled = false;
                exportMenu.Enabled = false;
            }

            List<IPhoneFile> list = new List<IPhoneFile>();
            foreach (ListViewItem itm in fileList.SelectedItems)
            {
                list.Add(itm.Tag as IPhoneFile);
            }

            OnSelectedFiles(list.ToArray());

        }

        private void toolShowBtn_Click(object sender, EventArgs e)
        {
            IPhoneFile file = (IPhoneFile)fileList.FocusedItem.Tag;
            IPhoneBackup backup = Model.Backup;

            FileManager filemanager = new FileManager();
            FileInfo dest = filemanager.GetWorkingFile(backup, file);

            IBrowsable browser = _browserManger.Get(dest.Extension);
            try
            {
                if (browser != null)
                {
                    Form form = browser.Open();
                    if (form != null)
                    {
                        if (browser.Modal)
                            form.ShowDialog(this);
                        else
                            form.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("'{0}' could not be opened by '{1}'"
                                                + "\n\n'{2}'"
                                                + "\nStacktrace\n{3}", dest.Name, browser.Name,
                                                ex.Message, ex.StackTrace), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolExportBtn_Click(object sender, EventArgs e)
        {
            IPhoneBackup backup = Model.Backup;
            List<IPhoneFile> files = new List<IPhoneFile>();
            foreach (ListViewItem itm in fileList.SelectedItems)
                files.Add(itm.Tag as IPhoneFile);

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                string path = dialog.SelectedPath;
                Model.InvokeAsync(files, delegate(IPhoneFile file)
                {
                    string source = Path.Combine(backup.Path, file.Key);
                    string dest = Path.Combine(path, file.Path.Replace("/", Path.DirectorySeparatorChar.ToString()));

                    // If there is a folder structur
                    // create it.
                    int lastIndex = file.Path.LastIndexOf("/");
                    if (lastIndex >= 0)
                    {
                        string fileFolder = file.Path.Substring(0, lastIndex);
                        Directory.CreateDirectory(Path.Combine(path, fileFolder.Replace("/", Path.DirectorySeparatorChar.ToString())));
                    }

                    // Copy the file (overwrite)
                    File.Copy(source, dest, true);

                    FileInfo info = new FileInfo(dest);
                    if (info.Extension == ".plist")
                    {
                        try
                        {
                            PListRoot root = PListRoot.Load(info.FullName);
                            if (root.Format == PListFormat.Binary)
                            {
                                using (XmlWriter writer = XmlWriter.Create(Path.Combine(info.Directory.FullName, "converted_" + info.Name)))
                                {
                                    root.WriteXml(writer);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    }
                }, "Copying..", Cursors.WaitCursor);
            }

        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            string content = searchBox.Text.ToLower();
            folderList_DoubleClick(sender, e); // Fill.

            List<ListViewItem> collection = new List<ListViewItem>();
            foreach (ListViewItem itm in fileList.Items)
                collection.Add(itm);

            fileList.Items.Clear();
            fileList.BeginUpdate();
            foreach (ListViewItem itm in collection)
            {
                IPhoneFile file = itm.Tag as IPhoneFile;
                if (file.Path.ToLower().Contains(content) || string.IsNullOrWhiteSpace(content))
                {
                    fileList.Items.Add(itm);
                }
            }
            fileList.EndUpdate();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("iDevice Backup Browser\n"
                + "\n Features: \n"
                + "  * Browser iTunes > 9.2 backups\n"
                + "  * View and export files\n"
                + "  * Convert *.plist from binary to xml\n"
                + "  * ...and more\n"
                + "\n"
                + "\n Licence:\n"
                + "  * New BSD License \n"
                + "\n Credits: \n"
                + "  * http://code.google.com/p/iphonebackupbrowser/"
                + "\n\n"
                + " Enhanced by: Isak & Magnus");
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginManagerWindow win = new PluginManagerWindow(_pluginManager);
            win.ShowDialog(this);
        }

        private void folderList_SelectedIndexChanged(object sender, EventArgs e)
        {
            IPhoneApp app = folderList.FocusedItem.Tag as IPhoneApp;
            OnSelectedApps(app);
        }

        private void OnSelectedApps(IPhoneApp app)
        {
            if (SelectedApps != null)
                SelectedApps(this, new IPhoneAppSelectedArgs(app));
        }

        private void OnSelectedBackup(IPhoneBackup backup)
        {
            if (SelectedBackup != null)
            {
                SelectedBackup(this, new IPhoneBackupSelectedArgs(backup));
            }
        }

        private void OnSelectedFiles(IPhoneFile[] file)
        {
            if (SelectedFiles != null)
                SelectedFiles(fileList, new IPhoneFileSelectedArgs(file));
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog from = new FolderBrowserDialog();
            from.Description = "Select a iDevice backup to inspect";

            string s = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            s = Path.Combine(s, "Apple Computer", "MobileSync", "Backup");
            from.SelectedPath = s;

            DialogResult result = from.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                DirectoryInfo d = new DirectoryInfo(from.SelectedPath);
                List<IPhoneBackup> backups = new List<IPhoneBackup>();
                foreach (DirectoryInfo sd in d.EnumerateDirectories())
                {
                    try
                    {
                        IPhoneBackup backup = IPhoneBackup.New(sd);
                        backups.Add(backup);
                    }
                    catch (FileLoadException ex)
                    {
                        Logger.DebugException(ex.Message, ex);
                    }
                }

                if (backups.Count < 1)
                {
                    MessageBox.Show("No backups found!");
                }
                else
                {
                    changeBackupToolStripMenuItem.DropDownItems.Clear();
                    foreach (IPhoneBackup b in backups)
                    {
                        ToolStripMenuItem itm = new ToolStripMenuItem(b.ToString());
                        itm.Tag = b;
                        itm.Click += delegate(object se, EventArgs arg)
                        {
                            ToolStripMenuItem me = se as ToolStripMenuItem;
                            if (me != null)
                                SelectBackup(me.Tag as IPhoneBackup);
                        };
                        changeBackupToolStripMenuItem.DropDownItems.Add(itm);
                    }

                    SelectBackupForm form = new SelectBackupForm(backups.ToArray());
                    form.ShowDialog(this);
                    if (form.Selected != null)
                        SelectBackup(form.Selected);
                }
            }
        }

        public void SelectBackup(IPhoneBackup backup)
        {
            folderList.Items.Clear();
            fileList.Items.Clear();

            OnSelectedBackup(backup);

            UpdateTitle(backup.DisplayName);

            FileManager fm = new FileManager();
            List<IPhoneApp> apps = backup.GetApps();
            foreach (var app in apps)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Tag = app;
                lvi.Text = app.Name;
                lvi.SubItems.Add(app.Files != null ? app.Files.Count.ToString() : "N/A");
                folderList.Items.Add(lvi);
            }
        }

        public void SelectApp(IPhoneApp app)
        {
            fileList.Items.Clear();

            if (app.Files == null)
                return;

            fileList.BeginUpdate();
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                ListViewItem[] lvic = new ListViewItem[app.Files.Count];
                int idx = 0;

                foreach (IPhoneFile ff in app.Files)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = ff;
                    lvi.Text = ff.Path;
                    lvi.SubItems.Add(ff.FileLength.ToString());
                    lvi.SubItems.Add(ff.ModificationTime);
                    lvi.SubItems.Add(ff.Domain);
                    lvi.SubItems.Add(ff.Key);

                    lvic[idx++] = lvi;
                }

                fileList.Items.AddRange(lvic);
            }
            finally
            {
                fileList.EndUpdate();
                Cursor.Current = Cursors.Default;
            }
        }

        public void UpdateTitle(string p)
        {
            Text = "iDevice Backup Browser [" + p + "]";
        }

        public ProgressArgs PushProgress(string name)
        {
            ProgressArgs arg = new ProgressArgs(name);
            progressPanel.Controls.Add(arg.Panel);
            return arg;
        }

        public void PopProgress(ProgressArgs arg)
        {
            progressPanel.Controls.RemoveByKey(arg.Key);
        }
    }

    public class ProgressArgs
    {
        private string _name;
        public ProgressBar ProgressBar { get; private set; }
        public Label Label { get; private set; }
        public Button Button { get; private set; }
        public string Key { get; private set; }

        public TableLayoutPanel Panel
        {
            get
            {
                TableLayoutPanel panel = new TableLayoutPanel();
                panel.AutoSize = true;
                panel.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
                panel.Name = Key;
                panel.Controls.Add(Label, 0, 0);
                panel.Controls.Add(ProgressBar, 1, 0);
                panel.Controls.Add(Button, 2, 0);

                return panel;
            }
        }

        public ProgressArgs(string name)
        {
            _name = name;
            ProgressBar = new ProgressBar { Maximum = 100, Anchor = AnchorStyles.Right | AnchorStyles.Left };
            Label = new Label { Text = name, Anchor = AnchorStyles.Left };
            Button = new Button { Text = "Cancel", Anchor = AnchorStyles.Right };
            Key = Guid.NewGuid().ToString();
        }
    }

    public class IPhoneFileSelectedArgs : EventArgs
    {
        public IPhoneFile[] Selected { get; private set; }

        public IPhoneFileSelectedArgs(IPhoneFile[] selected)
        {
            Selected = selected;
        }
    }

    public class IPhoneAppSelectedArgs : EventArgs
    {
        public IPhoneApp Selected { get; private set; }

        public IPhoneAppSelectedArgs(IPhoneApp selected)
        {
            Selected = selected;
        }
    }

    public class IPhoneBackupSelectedArgs : EventArgs
    {
        public IPhoneBackup Selected { get; private set; }

        public IPhoneBackupSelectedArgs(IPhoneBackup selected)
        {
            Selected = selected;
        }
    }

}
