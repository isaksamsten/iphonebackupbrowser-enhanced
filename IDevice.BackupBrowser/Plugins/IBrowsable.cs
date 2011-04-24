using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using IDevice.Plugins;

namespace IDevice.Plugins
{
    public interface IBrowsable : IPlugin
    {
        /// <summary>
        /// The prefix this browser handels
        /// </summary>
        string Prefix { get; }
    }
}
