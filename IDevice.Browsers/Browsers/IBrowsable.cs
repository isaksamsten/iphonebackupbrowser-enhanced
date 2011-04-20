using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using IDevice.Plugins;

namespace IDevice.Plugins.Browsers
{
    public interface IBrowsable : IPlugin
    {
        /// <summary>
        /// The prefix this browser handels
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// Initialize this browsable with file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Form Initialize(FileInfo file);

        /// <summary>
        /// Init with path to file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Form Initialize(string path);
    }
}
