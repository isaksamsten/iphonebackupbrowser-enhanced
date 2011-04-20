using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IDevice.Plugins
{
    public interface IPlugin
    {
        /// <summary>
        /// Get the menu to be added to the menu
        /// </summary>
        /// <returns></returns>
        ToolStripMenuItem GetMenu();

        // TO BE ADDED
        //string Name { get; }
        //string Author { get; }
        //string Description { get; }
        //Icon Icon { get; }



    }
}
