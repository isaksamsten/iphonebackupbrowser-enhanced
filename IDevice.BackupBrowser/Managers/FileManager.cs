using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IDevice.IPhone;

namespace IDevice.Managers
{
    public class FileManager
    {
        /// <summary>
        /// Return a file copy from a temp folder
        /// </summary>
        /// <param name="backup"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public FileInfo GetWorkingFile(IPhoneBackup backup, IPhoneFile file)
        {
            string tempPath = Path.GetTempPath();
            string fileName = GetFileName(file);

            string src = Path.Combine(backup.Path, file.Key);
            string dest = Path.Combine(tempPath, fileName);
            if (src != dest)
                File.Copy(src, dest, true);

            return new FileInfo(dest);
        }

        public void Copy(IPhoneBackup backup, IPhoneFile file, string dest)
        {
            string src = Path.Combine(backup.Path, file.Key);
            dest = Path.Combine(dest, GetFileName(file));
            if (src != dest)
                File.Copy(src, dest, true);
        }

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
    }
}
