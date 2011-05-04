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
    /// <summary>
    /// Question: Is it required to have this thread safe?
    /// 
    /// Long copy operations will block for ever...
    /// 
    /// 
    /// </summary>
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

        private object _lock = new object();

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
        /// 
        /// The path will be /Temp/$$idb$/{dir}/{fileName} (or random i.e. Path.GetTempFileName())
        /// 
        /// if verify == true the file from source and the destination hashes will be compared
        /// if the comparsion fail. An IOExcetion is raised.
        /// </summary>
        /// <param name="backup"></param>
        /// <param name="file"></param>
        /// <param name="random"></param>
        /// <param name="dir"></param>
        /// <param name="verify">Verify the files after copy (using md5)</param>
        /// <exception cref="IOException">
        /// Thrown if file copy failed or that the copy cannot be verified
        /// </exception>
        /// <returns></returns>
        public FileInfo GetWorkingFile(string dir, IPhoneBackup backup, IPhoneFile file, bool random, bool verify = true)
        {
            lock (_lock)
            {
                string tempPath = Path.Combine(Path.GetTempPath(), BasePath, dir);
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                FileInfo dest = new FileInfo(Path.GetTempFileName());
                if (!random)
                    dest = new FileInfo(Path.Combine(tempPath, GetFileName(file)));

                FileInfo src = GetOriginalFile(backup, file);
                string srcHash = "", destHash = "";
                if (verify)
                    srcHash = Util.MD5File(src);

                if (src != dest)
                    File.Copy(src.FullName, dest.FullName, true);

                if (verify)
                {
                    destHash = Util.MD5File(dest);
                    if (srcHash != destHash)
                    {
                        Clean(dest.FullName);
                        throw new IOException("File copy failed. Reason: Hashes do not match!!");
                    }
                }
                return dest;
            }
        }

        public FileInfo GetOriginalFile(IPhoneBackup backup, IPhoneFile file)
        {
            string src = Path.Combine(backup.Path, file.Key);
            return new FileInfo(src);
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
            lock (_lock)
            {
                string src = Path.Combine(backup.Path, file.Key);
                dest = Path.Combine(dest, GetFileName(file));
                if (src != dest)
                    File.Copy(src, dest, true);
            }
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
            lock (_lock)
            {
                try
                {
                    Directory.Delete(Path.Combine(Path.GetTempPath(), BasePath, subPath), true);
                }
                catch (Exception e)
                {
                    Logger.ErrorException(e.Message, e);
                }
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
