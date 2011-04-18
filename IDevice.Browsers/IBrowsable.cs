using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IDevice.Browsers
{
    public interface IBrowsable
    {
        void Open(FileInfo file);
        void Open(string path);
    }
}
