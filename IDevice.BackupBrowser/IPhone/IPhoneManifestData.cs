using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDevice.IPhone
{
    public class IPhoneManifestData
    {
        //public List<iPhoneApp> Applications;
        public Dictionary<string, IPhoneFile> Files { get; set; }
    };
}
