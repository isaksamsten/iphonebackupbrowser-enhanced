using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IDevice.Plugins.Browsers
{
    public class DefaultBrowser : AbstractPlugin, IBrowsable
    {
        public string Prefix
        {
            get { return "*"; }
        }

        public override Form Open()
        {
            //implement os open
            return null;
        }

        public override string PluginAuthor
        {
            get { return "Isak Karlsson"; }
        }

        public override string PluginDescription
        {
            get { return "Default handler. Handle using Operating system."; }
        }

        public override string PluginName
        {
            get { return "DefaultBrowser"; }
        }
    }
}