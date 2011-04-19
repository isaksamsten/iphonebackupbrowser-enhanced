using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace IDevice.Browsers
{
    public class AbstractBrowsable : Form, IBrowsable
    {
        private string prefix;
        protected AbstractBrowsable(string prefix)
        {
            this.prefix = prefix;
        }

        public AbstractBrowsable() : this("*******************") { }

        public virtual Form Initialize(FileInfo file)
        {
            throw new NotImplementedException("Sub-class responsibility");
        }

        public virtual Form Initialize(string path)
        {
            return Initialize(new FileInfo(path));
        }

        public virtual ToolStripMenuItem GetMenu()
        {
            throw new NotImplementedException();
        }


        public virtual string Prefix
        {
            get { return this.prefix; }
        }
    }
}
