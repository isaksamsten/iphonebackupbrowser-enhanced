using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace IDevice
{
    public static class Util
    {

        #region "HASH"

        public static string MD5File(FileInfo fileInfo)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                return FileHash(fileInfo, md5);
            }
        }

        public static string SHA1File(FileInfo fileInfo)
        {
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                return FileHash(fileInfo, sha1);
            }
        }

        public static string FileHash(FileInfo fileInfo, HashAlgorithm hash)
        {
            using (FileStream file = new FileStream(fileInfo.FullName, FileMode.Open))
            {
                byte[] retVal = hash.ComputeHash(file);
                return BitConverter.ToString(retVal).Replace("-", "");	// hex string
            }

        }

        #endregion

        #region "HELPERS"

        public static int Percent(int current, int length)
        {
            return (int)(((double)current / length) * 100);
        }

        #endregion
    }
}
