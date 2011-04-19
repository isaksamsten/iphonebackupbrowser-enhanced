using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace IDevice.Browsers
{
    public interface IBrowsable
    {
        Form Initialize(FileInfo file);
        Form Initialize(string path);
    }
}
