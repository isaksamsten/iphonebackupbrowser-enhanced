using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IDevice.IPhone;
using PList;
using mbdbdump;

namespace IDevice.Managers
{
    public class FileManager
    {
        /// <summary>
        /// Return a file copy from a temp folder
        /// </summary>
        /// <param name="backup"></param>
        /// <param name="file"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public FileInfo GetWorkingFile(IPhoneBackup backup, IPhoneFile file, bool random)
        {
            string tempPath = Path.Combine(Path.GetTempPath());
            string dest = Path.GetTempFileName();
            if (!random)
                dest = Path.Combine(tempPath, GetFileName(file));

            string src = Path.Combine(backup.Path, file.Key);
            if (src != dest)
                File.Copy(src, dest, true);

            return new FileInfo(dest);
        }

        /// <summary>
        /// Returns a non random file
        /// </summary>
        /// <param name="backup"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public FileInfo GetWorkingFile(IPhoneBackup backup, IPhoneFile file)
        {
            return GetWorkingFile(backup, file, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backup"></param>
        /// <param name="file"></param>
        /// <param name="dest"></param>
        public void Copy(IPhoneBackup backup, IPhoneFile file, string dest)
        {
            string src = Path.Combine(backup.Path, file.Key);
            dest = Path.Combine(dest, GetFileName(file));
            if (src != dest)
                File.Copy(src, dest, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backup"></param>
        /// <param name="file"></param>
        /// <param name="dest"></param>
        public void Copy(IPhoneBackup backup, IPhoneFile file, FileInfo dest)
        {
            Copy(backup, file, dest.FullName);
        }

        /// <summary>
        /// Get the filename of an iphone file
        /// 
        /// excluding the directory hirarcy
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string GetFileName(IPhoneFile file)
        {
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

            return fileName;
        }

        //public IPhoneBackup GetBackup(DirectoryInfo path)
        //{
        //    try
        //    {
        //        string filename = Path.Combine(path.FullName, "Info.plist");
        //        PListRoot root = PListRoot.Load(filename);
        //        PListDict dict = root.Root as PListDict;

        //        IPhoneBackup backup = new IPhoneBackup();
        //        backup.Path = path.FullName;

        //        foreach (var p in dict)
        //        {
        //            switch (p.Key)
        //            {
        //                case "Device Name":
        //                    backup.DeviceName = p.Value().ToString();
        //                    break;
        //                case "Display Name":
        //                    backup.DisplayName = p.Value().ToString();
        //                    break;
        //                case "Last Backup Date":
        //                    backup.LastBackupDate = p.Value().ToString();
        //                    break;
        //            }
        //        }
        //        return backup;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new FileLoadException("No backup at " + path, e);
        //    }
        //}

        //public List<IPhoneApp> GetApps(IPhoneBackup backup)
        //{
        //    if (File.Exists(Path.Combine(backup.Path, "Manifest.mbdb")))
        //    {
        //        List<IPhoneApp> list = new List<IPhoneApp>();
        //        mbdb.MBFileRecord[] files = mbdb.ReadMBDB(backup.Path, false, true);
        //        PListRoot root = PListRoot.Load(Path.Combine(backup.Path, "Manifest.plist"));
        //        PListDict di = root.Root as PListDict;

        //        PListDict apps = null;
        //        if ((apps = di["Applications"] as PListDict) == null)
        //            return list;

        //        Dictionary<string, List<int>> filesByDomain = new Dictionary<string, List<int>>();
        //        for (int i = 0; i < files.Length; ++i)
        //        {
        //            if ((files[i].Mode & 0xF000) == 0x8000)
        //            {
        //                string d = files[i].Domain;
        //                if (!filesByDomain.ContainsKey(d))
        //                    filesByDomain.Add(d, new List<int>());

        //                filesByDomain[d].Add(i);
        //            }
        //        }

        //        foreach (var p in apps)
        //        {
        //            IPhoneApp app = new IPhoneApp();

        //            app.Key = p.Key;

        //            PListDict appd = p.Value as PListDict;

        //            KeyValuePair<string, IPListElement> name = appd.FirstOrDefault(x => x.Key == "CFBundleDisplayName");
        //            if (name.Value != null)
        //                app.DisplayName = name.Value.Value().ToString();

        //            KeyValuePair<string, IPListElement> bname = appd.FirstOrDefault(x => x.Key == "CFBundleName");
        //            if (bname.Value != null)
        //                app.Name = bname.Value.Value().ToString();

        //            KeyValuePair<string, IPListElement> ident = appd.FirstOrDefault(x => x.Key == "CFBundleIdentifier");
        //            if (ident.Value != null)
        //                app.Identifier = ident.Value.Value().ToString();

        //            KeyValuePair<string, IPListElement> cont = appd.FirstOrDefault(x => x.Key == "Container");
        //            if (cont.Value != null)
        //                app.Container = cont.Value.Value().ToString();

        //            if (app.Name == null) app.Name = app.Key;
        //            if (app.DisplayName == null) app.DisplayName = app.Name;

        //            if (filesByDomain.ContainsKey("AppDomain-" + app.Key))
        //            {
        //                app.Files = new List<IPhoneFile>();

        //                foreach (int i in filesByDomain["AppDomain-" + app.Key])
        //                {
        //                    IPhoneFile ff = new IPhoneFile();
        //                    mbdb.MBFileRecord x = files[i];
        //                    ff.Key = x.key;
        //                    ff.Domain = x.Domain;
        //                    ff.Path = x.Path;
        //                    ff.ModificationTime = x.aTime.ToString();
        //                    ff.FileLength = x.FileLength;
        //                    app.Files.Add(ff);
        //                }

        //                filesByDomain.Remove("AppDomain-" + app.Key);
        //            }
        //            list.Add(app);
        //        }

        //        IPhoneApp system = new IPhoneApp();
        //        system.Name = "System";
        //        system.DisplayName = "---";
        //        system.Identifier = "---";
        //        system.Container = "---";
        //        system.Files = new List<IPhoneFile>();

        //        foreach (List<int> i in filesByDomain.Values)
        //        {
        //            foreach (int j in i)
        //            {
        //                IPhoneFile ff = new IPhoneFile();
        //                mbdb.MBFileRecord x = files[j];
        //                ff.Key = x.key;
        //                ff.Domain = x.Domain;
        //                ff.Path = x.Path;
        //                ff.ModificationTime = x.aTime.ToString();
        //                ff.FileLength = x.FileLength;
        //                system.Files.Add(ff);
        //            }
        //        }
        //        list.Add(system);
        //        return list;
        //    }
        //    else
        //    {
        //        throw new FileLoadException("Can only handle iTunes <= v9.2");
        //    }
        //}
    }
}
