using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


namespace iphonebackupbrowser
{

    class DLL
    {
        [DllImport("bplist.dll", EntryPoint = "bplist2xml_buffer", CallingConvention = CallingConvention.StdCall)]
        private static extern int bplist2xml_(byte[] ptr, int len, out IntPtr xml_ptr, bool useOpenStepEpoch);

        [DllImport("bplist.dll", EntryPoint = "bplist2xml_file", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private static extern int bplist2xml_(string filename, out IntPtr xml_ptr, bool useOpenStepEpoch);

        [DllImport("bplist.dll", EntryPoint = "mdinfo", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int mdinfo(string filename, out string Domain, out string Path);

        // en C#, string et byte[] ne représentent pas la même chose: il y a la notion d'encoding dans une string,
        // (c'est forcément de l'Unicode), même marshalée depuis const char* d'une DLL non managée, qui est forcément
        // de l'ANSI
        // et ce n'est pas plus mal comme ainsi !
        public static int bplist2xml(byte[] ptr, int len, out byte[] xml, bool useOpenStepEpoch)
        {
            int xml_size;
            IntPtr xml_ptr;

            xml_size = bplist2xml_(ptr, len, out xml_ptr, useOpenStepEpoch);

            if (xml_size != 0)
            {
                xml = new byte[xml_size];
                Marshal.Copy(xml_ptr, xml, 0, xml_size);
                Marshal.FreeCoTaskMem(xml_ptr);
            }
            else
            {
                xml = null;
            }

            return xml_size;
        }


        public static int bplist2xml(string filename, out byte[] xml, bool useOpenStepEpoch)
        {
            int xml_size;
            IntPtr xml_ptr;

            xml_size = bplist2xml_(filename, out xml_ptr, useOpenStepEpoch);

            if (xml_size != 0)
            {
                xml = new byte[xml_size];
                Marshal.Copy(xml_ptr, xml, 0, xml_size);
                Marshal.FreeCoTaskMem(xml_ptr);
            }
            else
            {
                xml = null;
            }

            return xml_size;
        }
    }


    class iPhoneBackup
    {
        public string DeviceName;
        public string DisplayName;
        public string LastBackupDate;

        public string path;                 // chemin du backup

        public override string ToString()
        {
            return DisplayName + " (" + LastBackupDate + ")";
        }
    };


    class iPhoneApp
    {
        public string Key;
        public string DisplayName;          // CFBundleDisplayName
        public string Name;                 // CFBundleName
        public string Identifier;           // CFBundleIdentifier
        public string Container;            // le chemin d'install sur l'iPhone
        public List <String> Files;
    };


    class iPhoneFile
    {
        public string Key;                  
        public string Domain;
        public long FileLength;
        public string ModificationTime;
        public string Path;                 // information issue de .mdinfo
    };


    class iPhoneManifestData
    {
        //public List<iPhoneApp> Applications;
        public Dictionary<string, iPhoneFile> Files;
    };
}
