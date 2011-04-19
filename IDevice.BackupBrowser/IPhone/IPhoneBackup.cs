using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDevice.IPhone
{
    public class IPhoneBackup
    {
        public string DeviceName { get; set; }
        public string DisplayName { get; set; }
        public string LastBackupDate { get; set; }

        public string Path;                 // chemin du backup

        public override string ToString()
        {
            return DisplayName + " (" + LastBackupDate + ")";
        }
    };
}
