using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDevice.IPhone
{
    public class IPhoneApp
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }          // CFBundleDisplayName
        public string Name { get; set; }                 // CFBundleName
        public string Identifier { get; set; }          // CFBundleIdentifier
        public string Container { get; set; }            // le chemin d'install sur l'iPhone
        public List<String> Files { get; set; }
    };

}
