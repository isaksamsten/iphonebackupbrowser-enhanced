// MBDB/MBDX decoder
// René DEVICHI 2010

using System;
using System.Text;
using System.IO;
using System.Diagnostics;


namespace mbdbdump
{

    #region BigEndianBitConverter class
    class BigEndianBitConverter
    {
        private static byte[] ReverseBytes(byte[] inArray, int offset, int count)
        {
            int j = count;
            byte[] ret = new byte[count];

            for (int i = offset; i < offset + count; ++i)
                ret[--j] = inArray[i];
            return ret;
        }

        public static short ToInt16(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt16(ReverseBytes(value, startIndex, 2), 0);
            }
            else
            {
                return BitConverter.ToInt16(value, startIndex);
            }
        }

        public static ushort ToUInt16(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt16(ReverseBytes(value, startIndex, 2), 0);
            }
            else
            {
                return BitConverter.ToUInt16(value, startIndex);
            }
        }

        public static int ToInt32(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt32(ReverseBytes(value, startIndex, 4), 0);
            }
            else
            {
                return BitConverter.ToInt32(value, startIndex);
            }
        }

        public static uint ToUInt32(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt32(ReverseBytes(value, startIndex, 4), 0);
            }
            else
            {
                return BitConverter.ToUInt32(value, startIndex);
            }
        }

        public static long ToInt64(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt64(ReverseBytes(value, startIndex, 8), 0);
            }
            return BitConverter.ToInt64(value, startIndex);

        }
    }
    #endregion


    class mbdb
    {
        private static string getS(Stream fs)
        {
            int b0 = fs.ReadByte();
            int b1 = fs.ReadByte();

            if (b0 == 255 && b1 == 255)
                return "n/a";

            int length = b0 * 256 + b1;

            byte[] buf = new byte[length];
            fs.Read(buf, 0, length);

            // We need to do a "Unicode normalization form C" (see Unicode 4.0 TR#15)
            // since some applications don't like the canonical decomposition (NormalizationD)...

            // More information: http://msdn.microsoft.com/en-us/library/dd319093(VS.85).aspx
            // or http://msdn.microsoft.com/en-us/library/8eaxk1x2.aspx

            string s = Encoding.UTF8.GetString(buf, 0, length);

            return s.Normalize(NormalizationForm.FormC);
        }


        private static char toHex(int value)
        {
            value &= 0xF;
            if (value >= 0 && value <= 9) return (char)('0' + value);
            else return (char)('A' + (value - 10));
        }


        private static char toHexLow(int value)
        {
            value &= 0xF;
            if (value >= 0 && value <= 9) return (char)('0' + value);
            else return (char)('a' + (value - 10));
        }


        private static string toHex(byte[] data, params int[] spaces)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);

            int n = 0;
            int p = 0;

            for (int i = 0; i < data.Length; ++i)
            {
                if (n < spaces.Length && i == p + spaces[n])
                {
                    sb.Append(' ');
                    p += spaces[n];
                    n++;
                }
                sb.Append(toHex(data[i] >> 4));
                sb.Append(toHex(data[i] & 15));
            }

            return sb.ToString();
        }


        private static int fromHex(char c)
        {
            if (c >= '0' && c <= '9')
                return (int)(c - '0');

            if (c >= 'A' && c <= 'F')
                return (int)(c - 'A' + 10);

            if (c >= 'a' && c <= 'f')
                return (int)(c - 'a' + 10);

            return 0;
        }


        private static string getD(Stream fs)
        {
            int b0 = fs.ReadByte();
            int b1 = fs.ReadByte();

            if (b0 == 255 && b1 == 255)
                return "n/a";

            int length = b0 * 256 + b1;

            byte[] buf = new byte[length];
            fs.Read(buf, 0, length);

            // if we have only ASCII printable characters, we return the string
            int i;
            for (i = 0; i < length; ++i)
            {
                if (buf[i] < 32 || buf[i] >= 128)
                    break;
            }
            if (i == length)
                return Encoding.ASCII.GetString(buf, 0, length);

            // otherwise the hexadecimal dump
            StringBuilder sb = new StringBuilder(length * 2);

            for (i = 0; i < length; ++i)
            {
                sb.Append(toHex(buf[i] >> 4));
                sb.Append(toHex(buf[i] & 15));
            }

            return sb.ToString();
        }


        public struct MBFileRecord
        {
            // from .mbdx
            public string key;              // filename if the directory
            public int offset;              // offset of record in the .mbdb file
            public ushort Mode;             // 4xxx=dir, 8xxx=file, Axxx=symlink

            // from .mbdb
            public string Domain;
            public string Path;
            public string LinkTarget;
            public string DataHash;         // SHA.1 for 'important' files
            public string alwaysNA;

            public string data;             // the 40-byte block (some fields still need to be explained)

            //public ushort ModeBis;        // same as .mbdx field
            public int alwaysZero;
            public int unknown;
            public int UserId;
            public int GroupId;
            public DateTime aTime;          // aTime or bTime is the former ModificationTime
            public DateTime bTime;
            public DateTime cTime;
            public long FileLength;         // always 0 for link or directory
            public byte flag;               // 0 if special (link, directory), otherwise values unknown
            public byte PropertyCount;

            public struct Property
            {
                public string Name;
                public string Value;
            };
            public Property[] Properties;
        }


        public static MBFileRecord[] ReadMBDB(string BackupPath, bool dump, bool checks)
        {
            try
            {
                MBFileRecord[] files;
                MBFileRecord rec = new MBFileRecord();
                byte[] signature = new byte[6];                     // buffer signature
                byte[] buf = new byte[26];                          // buffer for .mbdx record
                StringBuilder sb = new StringBuilder(40);           // stringbuilder for the Key
                byte[] data = new byte[40];                         // buffer for the fixed part of .mbdb record

                System.DateTime unixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);


                // open the index
                FileStream mbdx = new FileStream(Path.Combine(BackupPath, "Manifest.mbdx"), FileMode.Open, FileAccess.Read);

                // skip signature
                mbdx.Read(signature, 0, 6);
                if (BitConverter.ToString(signature, 0) != "6D-62-64-78-02-00")     // "mbdx\2\0"
                {
                    throw new Exception("bad .mbdx file");
                }


                // open the database
                FileStream mbdb = new FileStream(Path.Combine(BackupPath, "Manifest.mbdb"), FileMode.Open, FileAccess.Read);

                // skip signature
                mbdb.Read(signature, 0, 6);
                if (BitConverter.ToString(signature, 0) != "6D-62-64-62-05-00")     // "mbdb\5\0"
                {
                    throw new Exception("bad .mbdb file");
                }

                // number of records in .mbdx
                if (mbdx.Read(buf, 0, 4) != 4)
                {
                    throw new Exception("altered .mbdx file");
                }
                int records = BigEndianBitConverter.ToInt32(buf, 0);
                files = new MBFileRecord[records];

                // loop through the records
                for (int i = 0; i < records; ++i)
                {
                    // get the fixed size .mbdx record
                    if (mbdx.Read(buf, 0, 26) != 26)
                        break;

                    // convert key to text, it's the filename in the backup directory
                    // in previous versions of iTunes, it was the file part of .mddata/.mdinfo
                    sb.Clear();
                    for (int j = 0; j < 20; ++j)
                    {
                        byte b = buf[j];
                        sb.Append(toHexLow(b >> 4));
                        sb.Append(toHexLow(b & 15));
                    }

                    rec.key = sb.ToString();
                    rec.offset = BigEndianBitConverter.ToInt32(buf, 20);
                    rec.Mode = BigEndianBitConverter.ToUInt16(buf, 24);


                    // read the record in the .mbdb
                    mbdb.Seek(6 + rec.offset, SeekOrigin.Begin);

                    rec.Domain = getS(mbdb);
                    rec.Path = getS(mbdb);
                    rec.LinkTarget = getS(mbdb);
                    rec.DataHash = getD(mbdb);
                    rec.alwaysNA = getD(mbdb);

                    mbdb.Read(data, 0, 40);

                    rec.data = toHex(data, 2, 4, 4, 4, 4, 4, 4, 4, 8, 1, 1);

                    //rec.ModeBis = BigEndianBitConverter.ToUInt16(data, 0);
                    rec.alwaysZero = BigEndianBitConverter.ToInt32(data, 2);
                    rec.unknown = BigEndianBitConverter.ToInt32(data, 6);
                    rec.UserId = BigEndianBitConverter.ToInt32(data, 10);       // or maybe GroupId (don't care...)
                    rec.GroupId = BigEndianBitConverter.ToInt32(data, 14);      // or maybe UserId

                    rec.aTime = unixEpoch.AddSeconds(BigEndianBitConverter.ToUInt32(data, 18));
                    rec.bTime = unixEpoch.AddSeconds(BigEndianBitConverter.ToUInt32(data, 22));
                    rec.cTime = unixEpoch.AddSeconds(BigEndianBitConverter.ToUInt32(data, 26));

                    rec.FileLength = BigEndianBitConverter.ToInt64(data, 30);

                    rec.flag = data[38];
                    rec.PropertyCount = data[39];

                    rec.Properties = new MBFileRecord.Property[rec.PropertyCount];
                    for (int j = 0; j < rec.PropertyCount; ++j)
                    {
                        rec.Properties[j].Name = getS(mbdb);
                        rec.Properties[j].Value = getD(mbdb);
                    }

                    files[i] = rec;


                    // debug print
                    if (dump)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("record {0} (mbdb offset {1})", i, rec.offset + 6);

                        Console.WriteLine("  key    {0}", rec.key);
                        Console.WriteLine("  domain {0}", rec.Domain);
                        Console.WriteLine("  path   {0}", rec.Path);
                        if (rec.LinkTarget != "n/a") Console.WriteLine("  target {0}", rec.LinkTarget);
                        if (rec.DataHash != "n/a") Console.WriteLine("  hash   {0}", rec.DataHash);
                        if (rec.alwaysNA != "n/a") Console.WriteLine("  unk3   {0}", rec.alwaysNA);

                        string l = "?";
                        switch ((rec.Mode & 0xF000) >> 12)
                        {
                            case 0xA: l = "link"; break;
                            case 0x4: l = "dir"; break;
                            case 0x8: l = "file"; break;
                        }
                        Console.WriteLine("  mode   {1} ({0})", rec.Mode & 0xFFF, l);

                        Console.WriteLine("  time   {0}", rec.aTime);

                        // length is unsignificant if link or dir
                        if ((rec.Mode & 0xF000) == 0x8000) Console.WriteLine("  length {0}", rec.FileLength);

                        Console.WriteLine("  data   {0}", rec.data);
                        for (int j = 0; j < rec.PropertyCount; ++j)
                        {
                            Console.WriteLine("  pn[{0}]  {1}", j, rec.Properties[j].Name);
                            Console.WriteLine("  pv[{0}]  {1}", j, rec.Properties[j].Value);
                        }
                    }


                    // some assertions...
                    if (checks)
                    {
                        //Debug.Assert(rec.Mode == rec.ModeBis);
                        Debug.Assert(rec.alwaysZero == 0);
                        if (rec.LinkTarget != "n/a") Debug.Assert((rec.Mode & 0xF000) == 0xA000);
                        if (rec.DataHash != "n/a") Debug.Assert(rec.DataHash.Length == 40);
                        Debug.Assert(rec.alwaysNA == "n/a");
                        if (rec.Domain.StartsWith("AppDomain-")) Debug.Assert(rec.GroupId == 501 && rec.UserId == 501);
                        if (rec.FileLength != 0) Debug.Assert((rec.Mode & 0xF000) == 0x8000);
                        if ((rec.Mode & 0xF000) == 0x8000) Debug.Assert(rec.flag != 0);
                        if ((rec.Mode & 0xF000) == 0xA000) Debug.Assert(rec.flag == 0 && rec.FileLength == 0);
                        if ((rec.Mode & 0xF000) == 0x4000) Debug.Assert(rec.flag == 0 && rec.FileLength == 0);
                    }

                }

                return files;
            }
            catch (Exception e)
            {
                Console.WriteLine("exception: {0}", e.Message);
            }

            return null;
        }

    }
}
