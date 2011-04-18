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
using IDevice.Browsers.Browsers;

namespace IDevice
{
    public partial class BackupBrowser : Form
    {
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

        private List<iPhoneBackup> backups = new List<iPhoneBackup>();
        private iPhoneManifestData manifest;
        private mbdb.MBFileRecord[] files92;


        private ListViewColumnSorter lvwColumnSorter;


        public BackupBrowser()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.Columns.Add("Display Name", 200);
            listView1.Columns.Add("Name", 200);
            listView1.Columns.Add("Files");
            listView1.Columns.Add("Identifier", 200);

            listView2.Columns.Add("Name", 400);
            listView2.Columns.Add("Size");
            listView2.Columns.Add("Date", 130);
            listView2.Columns.Add("Domain", 300);
            listView2.Columns.Add("Key", 250);

            // Créer une instance d'une méthode de trie de la colonne ListView et l'attribuer
            // au contrôle ListView.            
            lvwColumnSorter = new ListViewColumnSorter();
            listView2.ListViewItemSorter = lvwColumnSorter;


            string s = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            s = Path.Combine(s, "Apple Computer", "MobileSync", "Backup");

            DirectoryInfo d = new DirectoryInfo(s);

            foreach (DirectoryInfo sd in d.EnumerateDirectories())
            {
                try
                {
                    string filename = Path.Combine(sd.FullName, "Info.plist");
                    xdict dd = xdict.open(filename);

                    if (dd != null)
                    {
                        iPhoneBackup backup = new iPhoneBackup();

                        backup.path = sd.FullName;

                        foreach (xdictpair p in dd)
                        {
                            if (p.item.GetType() == typeof(string))
                            {
                                switch (p.key)
                                {
                                    case "Device Name": backup.DeviceName = (string)p.item; break;
                                    case "Display Name": backup.DisplayName = (string)p.item; break;
                                    case "Last Backup Date": backup.LastBackupDate = (string)p.item; break;
                                }
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

            foreach (iPhoneBackup b in backups)
            {
                comboBox1.Items.Add(b);
            }

            //comboBox1.SelectedIndex = 0;
        }



        private void ParseApplications(xdict di, HashSet<string> files)
        {
            dict sd;

            if (!di.findKey("Applications", out sd))
                return;

            //manifest.Applications = new List<iPhoneApp>();

            foreach (xdictpair p in new xdict(sd))
            {
                iPhoneApp app = new iPhoneApp();

                app.Key = p.key;

                foreach (xdictpair q in new xdict(p.item))
                {
                    if (q.key == "AppInfo")
                    {
                        xdict zz = new xdict(q.item);

                        zz.findKey("CFBundleDisplayName", out app.DisplayName);
                        zz.findKey("CFBundleName", out app.Name);
                        zz.findKey("CFBundleIdentifier", out app.Identifier);
                        zz.findKey("Container", out app.Container);
                    }
                    else if (q.key == "Files" && q.item.GetType() == typeof(array))
                    {
                        array ar = (array)q.item;

                        app.Files = new List<String>();

                        for (int k = 0; k < ar.Items.Length; ++k)
                        {
                            string name = (string)ar.Items[k];
                            app.Files.Add(name);
                            files.Add(name);
                        }
                    }
                }

                // il y a des applis mal paramétrées...
                if (app.Name == null) app.Name = app.Key;
                if (app.DisplayName == null) app.DisplayName = app.Name;

                //manifest.Applications.Add(app);

                ListViewItem lvi = new ListViewItem();
                lvi.Tag = app;
                lvi.Text = app.DisplayName;
                lvi.SubItems.Add(app.Name);
                lvi.SubItems.Add(app.Files != null ? app.Files.Count.ToString() : "N/A");
                lvi.SubItems.Add(app.Identifier != null ? app.Identifier : "N/A");
                listView1.Items.Add(lvi);
            }
        }


        private void ParseFiles(xdict di, HashSet<string> files)
        {
            dict sd;

            if (!di.findKey("Files", out sd))
                return;

            manifest.Files = new Dictionary<string, iPhoneFile>();

            iPhoneApp system = new iPhoneApp();
            system.Name = "System";
            system.DisplayName = "---";
            system.Identifier = "---";
            system.Container = "---";
            system.Files = new List<String>();

            foreach (xdictpair p in new xdict(sd))
            {
                //Debug.WriteLine("{0} {1}", p.key, p.item);

                iPhoneFile f = new iPhoneFile();

                f.Key = p.key;
                f.Path = null;

                foreach (xdictpair q in new xdict(p.item))
                {
                    //Debug.WriteLine("{0} {1}", q.key, q.item);

                    if (q.key == "Domain")
                        f.Domain = (string)q.item;
                    else if (q.key == "ModificationTime")
                        f.ModificationTime = (string)q.item;
                    else if (q.key == "FileLength")
                        f.FileLength = Convert.ToInt64((string)q.item);
                }

                manifest.Files.Add(p.key, f);

                if (!files.Contains(p.key))
                {
                    system.Files.Add(p.key);
                }
            }


            if (system.Files.Count != 0)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Tag = system;
                lvi.Text = system.DisplayName;
                lvi.SubItems.Add(system.Name);
                lvi.SubItems.Add(system.Files != null ? system.Files.Count.ToString() : "N/A");
                lvi.SubItems.Add(system.Identifier != null ? system.Identifier : "N/A");
                listView1.Items.Add(lvi);
            }
        }


        private void ParseAll92(xdict di, HashSet<string> files)
        {
            dict sd;

            if (!di.findKey("Applications", out sd))
                return;

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



            foreach (xdictpair p in new xdict(sd))
            {
                iPhoneApp app = new iPhoneApp();

                app.Key = p.key;

                xdict zz = new xdict(p.item);
                zz.findKey("CFBundleDisplayName", out app.DisplayName);
                zz.findKey("CFBundleName", out app.Name);
                zz.findKey("CFBundleIdentifier", out app.Identifier);
                zz.findKey("Container", out app.Container);

                // il y a des applis mal paramétrées...
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
                listView1.Items.Add(lvi);
            }


            {
                iPhoneApp system = new iPhoneApp();
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


                ListViewItem lvi = new ListViewItem();
                lvi.Tag = system;
                lvi.Text = system.DisplayName;
                lvi.SubItems.Add(system.Name);
                lvi.SubItems.Add(system.Files != null ? system.Files.Count.ToString() : "N/A");
                lvi.SubItems.Add(system.Identifier != null ? system.Identifier : "N/A");
                listView1.Items.Add(lvi);
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            listView2.Items.Clear();
            manifest = null;
            files92 = null;


            try
            {
                iPhoneBackup backup = (iPhoneBackup)comboBox1.SelectedItem;

                // backup iTunes 9.2+
                if (File.Exists(Path.Combine(backup.path, "Manifest.mbdb")))
                {
                    files92 = mbdbdump.mbdb.ReadMBDB(backup.path, false, true);

                    byte[] xml;
                    DLL.bplist2xml(Path.Combine(backup.path, "Manifest.plist"), out xml, false);

                    if (xml != null)
                    {
                        using (StreamReader sr = new StreamReader(new MemoryStream(xml)))
                        {
                            xdict dd = xdict.open(sr);

                            if (dd != null)
                            {
                                manifest = new iPhoneManifestData();

                                HashSet<string> files = new HashSet<string>();

                                ParseAll92(dd, files);
                            }
                        }

                        return;
                    }
                }


                // backup iTunes 8.2+ et <= 9.1.1
                xdict d = xdict.open(Path.Combine(backup.path, "Manifest.plist"));

                string data;

                if (d != null && d.findKey("Data", out data))
                {
                    byte[] bdata = Convert.FromBase64String(data);
                    byte[] xml;

                    DLL.bplist2xml(bdata, bdata.Length, out xml, false);

                    if (xml != null)
                    {
                        using (StreamReader sr = new StreamReader(new MemoryStream(xml)))
                        {
                            xdict dd = xdict.open(sr);

                            if (dd != null)
                            {
                                manifest = new iPhoneManifestData();

                                HashSet<string> files = new HashSet<string>();

                                ParseApplications(dd, files);

                                ParseFiles(dd, files);
                            }
                        }
                    }

                    return;
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


        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            iPhoneApp app = (iPhoneApp)listView1.FocusedItem.Tag;

            listView2.Items.Clear();

            if (app.Files == null)
                return;

            listView2.BeginUpdate();
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                ListViewItem[] lvic = new ListViewItem[app.Files.Count];
                int idx = 0;

                foreach (string f in app.Files)
                {
                    iPhoneFile ff;

                    if (manifest.Files == null)
                    {
                        ff = new iPhoneFile();

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

                        iPhoneBackup backup = (iPhoneBackup)comboBox1.SelectedItem;

                        if (ff.Path == null)
                        {
                            string domain;
                            DLL.mdinfo(Path.Combine(backup.path, f + ".mdinfo"), out domain, out ff.Path);
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

                listView2.Items.AddRange(lvic); 
            }
            finally
            {
                listView2.EndUpdate();
                Cursor.Current = Cursors.Default;
            }
        }


        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            iPhoneBackup backup = (iPhoneBackup)comboBox1.SelectedItem;
            iPhoneFile file = (iPhoneFile)listView2.FocusedItem.Tag;

            string ext = "";
            if (files92 == null)
                ext = ".mddata";

            string argument = @"/select, """ + Path.Combine(backup.path, file.Key + ext) + @"""";

            System.Diagnostics.Process.Start("explorer.exe", argument);
        }


        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            switch (listView1.Sorting)
            {
                case SortOrder.None: listView1.Sorting = SortOrder.Ascending; break;
                case SortOrder.Ascending: listView1.Sorting = SortOrder.Descending; break;
                case SortOrder.Descending: listView1.Sorting = SortOrder.None; break;
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
            this.listView2.Sort();
        }

        private void listView2_ItemDrag(object sender, ItemDragEventArgs e)
        {
            iPhoneBackup backup = comboBox1.SelectedItem as iPhoneBackup;
            List<iPhoneFile> files = new List<iPhoneFile>();
            foreach(ListViewItem itm in listView2.SelectedItems)
                files.Add(itm.Tag as iPhoneFile);


            toolExportProgress.Visible = true;
            toolExportProgress.Maximum = files.Count;
            toolExportProgress.Step = 1;

            string path = Path.GetTempPath();
            List<string> filenames = new List<string>();
            foreach (var ifile in files)
            {
                string source = Path.Combine(backup.path, ifile.Key);
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
                        byte[] bytes = null;
                        DLL.bplist2xml(info.FullName, out bytes, true);

                        WriteByteArrayToFile(bytes, info.FullName + ".new");
                        filenames.Add(info.FullName + ".new");
                    }
                    catch { }
                }

                filenames.Add(dest);
            }

            toolExportProgress.Value = 0;
            toolExportProgress.Visible = false;
         
            listView2.DoDragDrop(new DataObject(DataFormats.FileDrop, filenames.ToArray()), DragDropEffects.Copy);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            iPhoneBackup backup = (iPhoneBackup)comboBox1.SelectedItem;

            if (backup == null)
                return;

            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = backup.path;
            prc.Start();
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 1)
            {
                toolShowBtn.Enabled = true;
                toolExportBtn.Enabled = true;

                exportMenu.Enabled = true;
                showMenu.Enabled = true;
            }
            else if (listView2.SelectedItems.Count > 1)
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

        }

        private void toolShowBtn_Click(object sender, EventArgs e)
        {
            iPhoneFile file = (iPhoneFile)listView2.FocusedItem.Tag;
            iPhoneBackup backup = (iPhoneBackup)comboBox1.SelectedItem;

            string tempPath = Path.GetTempPath();
            string fileName = "";

            int lastIndex = file.Path.LastIndexOf("/");
            if (lastIndex >= 0)
            {
                fileName = file.Path.Substring(lastIndex + 1, file.Path.Length - lastIndex - 1);
            }
            else
            {
                fileName = file.Path;
            }

            string src = Path.Combine(backup.path, file.Key);
            string dest = Path.Combine(tempPath, fileName);
            File.Copy(src, dest, true);

            try
            {
                BrowseHandler.Current.Get(Path.GetExtension(dest)).Open(dest);
            }
            catch
            {
                MessageBox.Show("No program can open this file. Try export.");
            }
        }

        private void toolExportBtn_Click(object sender, EventArgs e)
        {
            iPhoneBackup backup = comboBox1.SelectedItem as iPhoneBackup;
            List<iPhoneFile> files = new List<iPhoneFile>();
            foreach(ListViewItem itm in listView2.SelectedItems)
                files.Add(itm.Tag as iPhoneFile);

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
                    string source = Path.Combine(backup.path, ifile.Key);            
                    string dest = Path.Combine(path, ifile.Path.Replace("/", Path.DirectorySeparatorChar.ToString()));

                    // If there is a folder structur
                    // create it.
                    int lastIndex = ifile.Path.LastIndexOf("/");
                    if(lastIndex >= 0)
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
                            byte[] bytes = null;
                            DLL.bplist2xml(info.FullName, out bytes, true);

                            WriteByteArrayToFile(bytes, info.FullName + ".new");
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

        private bool WriteByteArrayToFile(byte[] buff, string fileName)
        {
            bool response = false;

            using(FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite)) {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(buff);
                    response = true;
                }
            }

            return response;
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            string content = searchBox.Text.ToLower();
            listView1_DoubleClick(sender, e); // Fill.

            List<ListViewItem> collection = new List<ListViewItem>();
            foreach (ListViewItem itm in listView2.Items)
                collection.Add(itm);

            listView2.Items.Clear();
            listView2.BeginUpdate();
            foreach (ListViewItem itm in collection)
            {
                iPhoneFile file = itm.Tag as iPhoneFile;
                if(file.Path.ToLower().Contains(content) || string.IsNullOrWhiteSpace(content)) 
                {
                    listView2.Items.Add(itm);
                }
            }
            listView2.EndUpdate();
        }

        private void appSearchTxt_TextChanged(object sender, EventArgs e)
        {
            string content = appSearchTxt.Text.ToLower();
            comboBox1_SelectedIndexChanged(sender, e); //fill

            List<ListViewItem> collection = new List<ListViewItem>();
            foreach (ListViewItem itm in listView1.Items)
                collection.Add(itm);

            listView1.Items.Clear();
            listView1.BeginUpdate();
            foreach (ListViewItem itm in collection)
            {
                iPhoneApp file = itm.Tag as iPhoneApp;
                if (file.DisplayName.ToLower().Contains(content)
                    || file.Identifier.ToLower().Contains(content)
                    || string.IsNullOrWhiteSpace(content))
                {
                    listView1.Items.Add(itm);
                }
            }
            listView1.EndUpdate();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Original app: http://code.google.com/p/iphonebackupbrowser/" + "\n"
                + "Enhanced by: Isak & Magnus");
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /*
        private void listView2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Peter.ShellContextMenu ctxMnu = new Peter.ShellContextMenu();
                FileInfo[] arrFI = new FileInfo[1];
                arrFI[0] = new FileInfo(@"c:\temp\a.xml");
                ctxMnu.ShowContextMenu(arrFI, this.PointToScreen(new Point(e.X, e.Y)));
            }
        }
        */
    }

}
