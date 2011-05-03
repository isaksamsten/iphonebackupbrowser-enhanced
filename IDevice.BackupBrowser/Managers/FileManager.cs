using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IDevice.IPhone;
using PList;
using mbdbdump;
using System.Diagnostics;
using System.Windows.Forms;
using NLog;

namespace IDevice.Managers
{
    public class FileManager
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public static readonly string BasePath = "$idb$$";
        private static FileManager _current;

        public static FileManager Current
        {
            get
            {
                if (_current == null)
                    _current = new FileManager();
                return _current;
            }
        }

        /// <summary>
        /// Use FileManager.Current instead
        /// 
        /// This is to allow for cleaning...
        /// </summary>
        private FileManager()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backup"></param>
        /// <param name="file"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public FileInfo GetWorkingFile(IPhoneBackup backup, IPhoneFile file, bool random)
        {
            return GetWorkingFile("", backup, file, random);
        }

        /// <summary>
        /// Get a working file in the directory Temp/${InvokingClassName}/filename
        /// 
        /// This to avoid confilcts but a will to retain the name
        /// </summary>
        /// <param name="backup"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public FileInfo GetWorkingFileCurrentClass(IPhoneBackup backup, IPhoneFile file)
        {
            StackTrace trace = new StackTrace();
            StackFrame frame = trace.GetFrame(1);
            Type klass = frame.GetMethod().ReflectedType;

            return GetWorkingFile(string.Format("${0}", klass.Name), backup, file, false);
        }

        /// <summary>
        /// Return a file copy from a temp folder
        /// </summary>
        /// <param name="backup"></param>
        /// <param name="file"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public FileInfo GetWorkingFile(string dir, IPhoneBackup backup, IPhoneFile file, bool random)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), BasePath, dir);
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

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
        /// This returns a random file in a temporary location with the file
        /// name as Tuple.
        /// 
        /// Item1 = correct fileName
        /// Item2 = the fileInfo about the random file created
        /// </summary>
        /// <param name="backup"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public Tuple<string, FileInfo> GetRandomFile(IPhoneBackup backup, IPhoneFile file)
        {
            FileInfo fileInfo = GetWorkingFile(backup, file, true);
            string name = GetFileName(file);
            return Tuple.Create(name, fileInfo);
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

        /// <summary>
        /// Remove all files created in subPath
        /// </summary>
        /// <param name="subPath"></param>
        public void Clean(string subPath)
        {
            try
            {
                Directory.Delete(Path.Combine(Path.GetTempPath(), BasePath, subPath));
            }
            catch(Exception e)
            {
                Logger.ErrorException(e.Message, e);   
            }
        }

        /// <summary>
        /// Clean all files created by current class
        /// (asuming the usage of GetWorkingFileCurrentClass())
        /// </summary>
        public void CleanCurrentClass()
        {
            StackTrace trace = new StackTrace();
            StackFrame frame = trace.GetFrame(1);
            Type klass = frame.GetMethod().ReflectedType;
            Clean(string.Format("${0}", klass.Name));
        }

        /// <summary>
        /// Clean all files created...
        /// </summary>
        public void Clean()
        {
            Clean("");
        }
    }
}
