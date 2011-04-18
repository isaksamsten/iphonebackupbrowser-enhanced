using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IDevice.Browsers
{
    public abstract class AbstractBrowsable : IBrowsable
    {
        public abstract void Open(FileInfo file);

        public virtual void Open(string path)
        {
            Open(new FileInfo(path));
        }
    }
}
