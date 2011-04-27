using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDevice.IPhone
{
    public class IPhoneApp
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; }
        public string Container { get; set; }
        public List<IPhoneFile> Files { get; set; }
    };

}
