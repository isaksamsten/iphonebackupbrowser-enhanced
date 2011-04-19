using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace IDevice.Browsers
{
    public abstract class AbstractBrowsable : Form, IBrowsable
    {
        public abstract Form Initialize(FileInfo file);

        public virtual Form Initialize(string path)
        {
            return Initialize(new FileInfo(path));
        }
    }
}
