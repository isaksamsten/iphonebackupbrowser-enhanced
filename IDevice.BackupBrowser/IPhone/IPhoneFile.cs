using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDevice.IPhone
{
    public class IPhoneFile
    {
        public string Key { get; set; }
        public string Domain { get; set; }
        public long FileLength { get; set; }
        public string ModificationTime { get; set; }
        public string Path { get; set; }
    };
}
