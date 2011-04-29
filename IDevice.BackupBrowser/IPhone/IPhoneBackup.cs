using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PList;
using mbdbdump;

using IDevice.Extensions;
using NLog;

namespace IDevice.IPhone
{
    public class IPhoneBackup
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Use IPhoneBackup.New
        /// </summary>
        private IPhoneBackup() { }

        public static string DefaultPath
        {
            get
            {
                string s = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                s = System.IO.Path.Combine(s, "Apple Computer", "MobileSync", "Backup");
                return s;
            }
        }

        public static IPhoneBackup New(DirectoryInfo path)
        {
            try
            {
                string filename = System.IO.Path.Combine(path.FullName, "Info.plist");
                PListRoot root = PListRoot.Load(filename);
                PListDict dict = root.Root as PListDict;

                IPhoneBackup backup = new IPhoneBackup();
                backup.Path = path.FullName;

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
                return backup;
            }
            catch (Exception e)
            {
                throw new FileLoadException("No backup at " + path, e);
            }
        }

        public static IPhoneBackup[] Load(string path)
        {
            DirectoryInfo d = new DirectoryInfo(path);
            List<IPhoneBackup> backups = new List<IPhoneBackup>();
            foreach (DirectoryInfo sd in d.EnumerateDirectories())
            {
                try
                {
                    IPhoneBackup backup = New(sd);
                    backups.Add(backup);
                }
                catch (FileLoadException ex)
                {
                    Logger.DebugException(ex.Message, ex);
                }
            }

            return backups.ToArray();
        }

        public string DeviceName { get; set; }
        public string DisplayName { get; set; }
        public string LastBackupDate { get; set; }

        public string Path { get; set; }

        public List<IPhoneApp> GetApps()
        {
            if (File.Exists(System.IO.Path.Combine(Path, "Manifest.mbdb")))
            {
                List<IPhoneApp> list = new List<IPhoneApp>();
                mbdb.MBFileRecord[] files = mbdb.ReadMBDB(Path, false, true);
                PListRoot root = PListRoot.Load(System.IO.Path.Combine(Path, "Manifest.plist"));
                PListDict di = root.Root as PListDict;

                PListDict apps = null;
                if ((apps = di["Applications"] as PListDict) == null)
                    return list;

                Dictionary<string, List<int>> filesByDomain = new Dictionary<string, List<int>>();
                for (int i = 0; i < files.Length; ++i)
                {
                    if ((files[i].Mode & 0xF000) == 0x8000)
                    {
                        string d = files[i].Domain;
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
                        app.Files = new List<IPhoneFile>();

                        foreach (int i in filesByDomain["AppDomain-" + app.Key])
                        {
                            IPhoneFile ff = new IPhoneFile();
                            mbdb.MBFileRecord x = files[i];
                            ff.Key = x.key;
                            ff.Domain = x.Domain;
                            ff.Path = x.Path;
                            ff.ModificationTime = x.aTime.ToString();
                            ff.FileLength = x.FileLength;
                            app.Files.Add(ff);
                        }

                        filesByDomain.Remove("AppDomain-" + app.Key);
                    }
                    list.Add(app);
                }

                IPhoneApp system = new IPhoneApp();
                system.Name = "System";
                system.DisplayName = "---";
                system.Identifier = "---";
                system.Container = "---";
                system.Files = new List<IPhoneFile>();

                foreach (List<int> i in filesByDomain.Values)
                {
                    foreach (int j in i)
                    {
                        IPhoneFile ff = new IPhoneFile();
                        mbdb.MBFileRecord x = files[j];
                        ff.Key = x.key;
                        ff.Domain = x.Domain;
                        ff.Path = x.Path;
                        ff.ModificationTime = x.aTime.ToString();
                        ff.FileLength = x.FileLength;
                        system.Files.Add(ff);
                    }
                }
                list.Add(system);
                return list;
            }
            else
            {
                throw new FileLoadException("Can only handle iTunes <= v9.2");
            }
        }

        public override string ToString()
        {
            return DisplayName + " (" + LastBackupDate + ")";
        }
    };
}
