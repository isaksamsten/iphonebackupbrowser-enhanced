using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IDevice.Plugins.Browsers
{
    public class DefaultHandler : AbstractBrowsable
    {
        public DefaultHandler() : base("*") { }
    }
}
