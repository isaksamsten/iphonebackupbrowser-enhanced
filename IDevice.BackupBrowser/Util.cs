using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Collections;

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

        public static void Sort(ToolStripItemCollection collection, IComparer comparer)
        {
            ArrayList items = new ArrayList(collection);
            items.Sort(comparer);

            collection.Clear();
            foreach (object itm in items)
                collection.Add(itm as ToolStripItem);
        }

        public static void Sort(ToolStripItemCollection collection)
        {
            Sort(collection, new ToolStripItemComparer());
        }


        private class ToolStripItemComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                ToolStripItem a = x as ToolStripItem;
                ToolStripItem b = y as ToolStripItem;

                return a.Name.CompareTo(b.Name);
            }
        }
        #endregion
    }
}
