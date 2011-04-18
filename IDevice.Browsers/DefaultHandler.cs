using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDevice.Browsers
{
    public class DefaultHandler : AbstractBrowsable
    {
        public override void Open(System.IO.FileInfo file)
        {
            System.Diagnostics.Process.Start(file.FullName);
        }
    }
}
