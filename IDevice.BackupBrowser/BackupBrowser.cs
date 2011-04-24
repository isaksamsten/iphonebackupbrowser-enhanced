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

using IDevice.Reader;
using IDevice.IPhone;
using IDevice.Plugins;
using IDevice.Managers;

namespace IDevice
{
    public partial class BackupBrowser : Form
    {
        public event EventHandler<IPhoneFileSelectedArgs> SelectedFiles;
        public event EventHandler<IPhoneBackupSelectedArgs> SelectedBackup;

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

        private List<IPhoneBackup> backups = new List<IPhoneBackup>();
        private IPhoneManifestData manifest;
        private mbdb.MBFileRecord[] files92;


        private ListViewColumnSorter lvwColumnSorter;
        private BrowserManager _browserManger;
        private PluginManager _pluginManager;
        private SelectionModel _selectionModel;

        public BackupBrowser()
        {
            InitializeComponent();

            _pluginManager = new PluginManager(Properties.Settings.Default.Plugins.ToArray());
            _pluginManager.Added += new EventHandler<PluginArgs>(_pluginManager_Added);
            _pluginManager.Removed += new EventHandler<PluginArgs>(_pluginManager_Removed);

            _browserManger = new BrowserManager(_pluginManager);
            _selectionModel = new SelectionModel(this);

            foreach (IPlugin p in _pluginManager)
            {
                p.SetModel(_selectionModel);
            }
        }

        void _pluginManager_Removed(object sender, PluginArgs e)
        {
            // REmove menu and clean up interface
        }

        void _pluginManager_Added(object sender, PluginArgs e)
        {
            // Load the plugin in the interface
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            folderList.Columns.Add("Display Name", 200);
            folderList.Columns.Add("Name", 200);
            folderList.Columns.Add("Files");
            folderList.Columns.Add("Identifier", 200);

            fileList.Columns.Add("Name", 400);
            fileList.Columns.Add("Size");
            fileList.Columns.Add("Date", 130);
            fileList.Columns.Add("Domain", 300);
            fileList.Columns.Add("Key", 250);

            // Créer une instance d'une méthode de trie de la colonne ListView et l'attribuer
            // au contrôle ListView.            
            lvwColumnSorter = new ListViewColumnSorter();
            fileList.ListViewItemSorter = lvwColumnSorter;


            string s = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            s = Path.Combine(s, "Apple Computer", "MobileSync", "Backup");

            DirectoryInfo d = new DirectoryInfo(s);
            if (d.Exists)
            {
                foreach (DirectoryInfo sd in d.EnumerateDirectories())
                {
                    try
                    {
                        string filename = Path.Combine(sd.FullName, "Info.plist");
                        PListRoot root = PListRoot.Load(filename);
                        PListDict dict = root.Root as PListDict;
                        if (dict != null)
                        {
                            IPhoneBackup backup = new IPhoneBackup();
                            backup.Path = sd.FullName;

                            foreach (var p in dict)
                            {
                                switch (p.Key)
                                {
                                    case "Device Name":
                                        backup.DeviceName = p.Value().ToString();
                                        break;
                                    case "Display Name":
                                        backup.DisplayName = p.Value().ToString();
                                        break;
                                    case "Last Backup Date":
                                        backup.LastBackupDate = p.Value().ToString();
                                        break;
                                }
                            }

                            backups.Add(backup);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        MessageBox.Show(ex.InnerException.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show("There exsist no backups in: " + d.ToString());
            }

            foreach (IPhoneBackup b in backups)
            {
                backupSelect.Items.Add(b);
            }
        }

        private List<IPhoneApp> ParseAll(PListDict di)
        {
            List<IPhoneApp> list = new List<IPhoneApp>();

            PListDict apps = null;
            if ((apps = di["Applications"] as PListDict) == null)
                return list;

            Dictionary<string, List<int>> filesByDomain = new Dictionary<string, List<int>>();
            for (int i = 0; i < files92.Length; ++i)
            {
                if ((files92[i].Mode & 0xF000) == 0x8000)
                {
                    string d = files92[i].Domain;
                    if (!filesByDomain.ContainsKey(d))
                        filesByDomain.Add(d, new List<int>());

                    filesByDomain[d].Add(i);
                }
            }

            foreach (var p in apps)
            {
                IPhoneApp app = new IPhoneApp();

                app.Key = p.Key;

                PListDict appd = p.Value as PListDict;

                KeyValuePair<string, IPListElement> name = appd.FirstOrDefault(x => x.Key == "CFBundleDisplayName");
                if (name.Value != null)
                    app.DisplayName = name.Value.Value().ToString();

                KeyValuePair<string, IPListElement> bname = appd.FirstOrDefault(x => x.Key == "CFBundleName");
                if (bname.Value != null)
                    app.Name = bname.Value.Value().ToString();

                KeyValuePair<string, IPListElement> ident = appd.FirstOrDefault(x => x.Key == "CFBundleIdentifier");
                if (ident.Value != null)
                    app.Identifier = ident.Value.Value().ToString();

                KeyValuePair<string, IPListElement> cont = appd.FirstOrDefault(x => x.Key == "Container");
                if (cont.Value != null)
                    app.Container = cont.Value.Value().ToString();

                if (app.Name == null) app.Name = app.Key;
                if (app.DisplayName == null) app.DisplayName = app.Name;

                if (filesByDomain.ContainsKey("AppDomain-" + app.Key))
                {
                    app.Files = new List<String>();

                    foreach (int i in filesByDomain["AppDomain-" + app.Key])
                    {
                        app.Files.Add(i.ToString());
                    }

                    filesByDomain.Remove("AppDomain-" + app.Key);
                }

                ListViewItem lvi = new ListViewItem();
                lvi.Tag = app;
                lvi.Text = app.DisplayName;
                lvi.SubItems.Add(app.Name);
                lvi.SubItems.Add(app.Files != null ? app.Files.Count.ToString() : "N/A");
                lvi.SubItems.Add(app.Identifier != null ? app.Identifier : "N/A");
                folderList.Items.Add(lvi);

                list.Add(app);
            }
            IPhoneApp system = new IPhoneApp();
            system.Name = "System";
            system.DisplayName = "---";
            system.Identifier = "---";
            system.Container = "---";
            system.Files = new List<String>();

            foreach (List<int> i in filesByDomain.Values)
            {
                foreach (int j in i)
                {
                    system.Files.Add(j.ToString());
                }
            }

            ListViewItem lvi2 = new ListViewItem();
            lvi2.Tag = system;
            lvi2.Text = system.DisplayName;
            lvi2.SubItems.Add(system.Name);
            lvi2.SubItems.Add(system.Files != null ? system.Files.Count.ToString() : "N/A");
            lvi2.SubItems.Add(system.Identifier != null ? system.Identifier : "N/A");
            folderList.Items.Add(lvi2);

            list.Add(system);

            return list;

        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            folderList.Items.Clear();
            fileList.Items.Clear();
            files92 = null;

            try
            {
                IPhoneBackup backup = (IPhoneBackup)backupSelect.SelectedItem;
                OnSelectedBackup(backup);

                // backup iTunes 9.2+
                if (File.Exists(Path.Combine(backup.Path, "Manifest.mbdb")))
                {
                    files92 = mbdbdump.mbdb.ReadMBDB(backup.Path, false, true);
                    PListRoot root = PListRoot.Load(Path.Combine(backup.Path, "Manifest.plist"));
                    if (root.Root != null)
                    {
                        manifest = new IPhoneManifestData();
                        List<IPhoneApp> apps = ParseAll(root.Root as PListDict);
                    }
                }
                else
                {
                    MessageBox.Show("We dont support iTunes v < 9.2");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void OnSelectedBackup(IPhoneBackup backup)
        {
            if (SelectedBackup != null)
            {
                SelectedBackup(backupSelect, new IPhoneBackupSelectedArgs(backup));
            }
        }


        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            IPhoneApp app = (IPhoneApp)folderList.FocusedItem.Tag;

            fileList.Items.Clear();

            if (app.Files == null)
                return;

            fileList.BeginUpdate();
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                ListViewItem[] lvic = new ListViewItem[app.Files.Count];
                int idx = 0;

                foreach (string f in app.Files)
                {
                    IPhoneFile ff;

                    if (manifest.Files == null)
                    {
                        ff = new IPhoneFile();

                        mbdb.MBFileRecord x = files92[Int32.Parse(f)];

                        ff.Key = x.key;
                        ff.Domain = x.Domain;
                        ff.Path = x.Path;
                        ff.ModificationTime = x.aTime.ToString();
                        ff.FileLength = x.FileLength;
                    }
                    else
                    {
                        //Debug.WriteLine("{0} {1}", f, "");
                        ff = manifest.Files[f];

                        IPhoneBackup backup = (IPhoneBackup)backupSelect.SelectedItem;

                        if (ff.Path == null)
                        {
                            PListRoot root = PListRoot.Load(Path.Combine(backup.Path, f + ".mdinfo"));
                            PListDict dict = root.Root as PListDict;
                            string domain;
                            if (dict.ContainsKey("Domain"))
                                domain = dict["Domain"].Value().ToString();
                            if (dict.ContainsKey("Path"))
                                ff.Path = dict["Path"].Value().ToString();

                            if (ff.Path == null)
                                ff.Path = "N/A";
                        }
                    }


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


        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            IPhoneBackup backup = (IPhoneBackup)backupSelect.SelectedItem;
            IPhoneFile file = (IPhoneFile)fileList.FocusedItem.Tag;

            string ext = "";
            if (files92 == null)
                ext = ".mddata";

            string argument = @"/select, """ + Path.Combine(backup.Path, file.Key + ext) + @"""";

            System.Diagnostics.Process.Start("explorer.exe", argument);
        }


        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
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
            // Déterminer si la colonne sélectionnée est déjà la colonne triée.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Inverser le sens de tri en cours pour cette colonne.
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
                // Définir le numéro de colonne à trier ; par défaut sur croissant.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Procéder au tri avec les nouvelles options.
            this.fileList.Sort();
        }

        private void listView2_ItemDrag(object sender, ItemDragEventArgs e)
        {
            IPhoneBackup backup = backupSelect.SelectedItem as IPhoneBackup;
            List<IPhoneFile> files = new List<IPhoneFile>();
            foreach (ListViewItem itm in fileList.SelectedItems)
                files.Add(itm.Tag as IPhoneFile);


            toolExportProgress.Visible = true;
            toolExportProgress.Maximum = files.Count;
            toolExportProgress.Step = 1;

            string path = Path.GetTempPath();
            List<string> filenames = new List<string>();
            foreach (var ifile in files)
            {
                string source = Path.Combine(backup.Path, ifile.Key);
                string dest = Path.Combine(path, ifile.Path.Replace("/", Path.DirectorySeparatorChar.ToString()));

                // If there is a folder structure
                // create it.
                int lastIndex = ifile.Path.LastIndexOf("/");
                if (lastIndex >= 0)
                {
                    string fileFolder = ifile.Path.Substring(0, lastIndex);
                    Directory.CreateDirectory(Path.Combine(path, fileFolder.Replace("/", Path.DirectorySeparatorChar.ToString())));
                }

                // Copy the file (overwrite)
                File.Copy(source, dest, true);
                filenames.Add(dest);
            }

            toolExportProgress.Value = 0;
            toolExportProgress.Visible = false;

            fileList.DoDragDrop(new DataObject(DataFormats.FileDrop, filenames.ToArray()), DragDropEffects.Copy);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            IPhoneBackup backup = (IPhoneBackup)backupSelect.SelectedItem;

            if (backup == null)
                return;

            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = backup.Path;
            prc.Start();
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
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

        private void OnSelectedFiles(IPhoneFile[] file)
        {
            if (SelectedFiles != null)
                SelectedFiles(fileList, new IPhoneFileSelectedArgs(file));
        }

        private void toolShowBtn_Click(object sender, EventArgs e)
        {
            IPhoneFile file = (IPhoneFile)fileList.FocusedItem.Tag;
            IPhoneBackup backup = (IPhoneBackup)backupSelect.SelectedItem;

            FileManager filemanager = new FileManager();
            FileInfo dest = filemanager.GetWorkingFile(backup, file);

            try
            {
                IBrowsable browser = _browserManger.Get(dest.Extension);
                if (browser != null)
                {
                    Form form = browser.Open();
                    if (browser.IsModal)
                        form.ShowDialog(this);
                    else
                        form.Show();
                }
            }
            catch
            {
                MessageBox.Show("No program can open this file. Try export.");
            }
        }

        private void toolExportBtn_Click(object sender, EventArgs e)
        {
            IPhoneBackup backup = backupSelect.SelectedItem as IPhoneBackup;
            List<IPhoneFile> files = new List<IPhoneFile>();
            foreach (ListViewItem itm in fileList.SelectedItems)
                files.Add(itm.Tag as IPhoneFile);

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                toolExportProgress.Visible = true;
                toolExportProgress.Maximum = files.Count;
                toolExportProgress.Step = 1;

                Cursor.Current = Cursors.WaitCursor;
                String path = dialog.SelectedPath;

                foreach (var ifile in files)
                {
                    string source = Path.Combine(backup.Path, ifile.Key);
                    string dest = Path.Combine(path, ifile.Path.Replace("/", Path.DirectorySeparatorChar.ToString()));

                    // If there is a folder structur
                    // create it.
                    int lastIndex = ifile.Path.LastIndexOf("/");
                    if (lastIndex >= 0)
                    {
                        string fileFolder = ifile.Path.Substring(0, lastIndex);
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
                        catch { }
                    }

                    toolExportProgress.Value = toolExportProgress.Value + 1;
                }
                Cursor.Current = Cursors.Default;
                toolExportProgress.Value = 0;
                toolExportProgress.Visible = false;
            }

        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            string content = searchBox.Text.ToLower();
            listView1_DoubleClick(sender, e); // Fill.

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

        /// <summary>
        /// Search for apps
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void appSearchTxt_TextChanged(object sender, EventArgs e)
        {
            string content = appSearchTxt.Text.ToLower();
            comboBox1_SelectedIndexChanged(sender, e); //fill

            List<ListViewItem> collection = new List<ListViewItem>();
            foreach (ListViewItem itm in folderList.Items)
                collection.Add(itm);

            folderList.Items.Clear();
            folderList.BeginUpdate();
            foreach (ListViewItem itm in collection)
            {
                IPhoneApp file = itm.Tag as IPhoneApp;
                if (file.DisplayName.ToLower().Contains(content)
                    || file.Identifier.ToLower().Contains(content)
                    || string.IsNullOrWhiteSpace(content))
                {
                    folderList.Items.Add(itm);
                }
            }
            folderList.EndUpdate();
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
    }

    public class IPhoneFileSelectedArgs : EventArgs
    {
        public IPhoneFile[] Selected { get; private set; }

        public IPhoneFileSelectedArgs(IPhoneFile[] selected)
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
