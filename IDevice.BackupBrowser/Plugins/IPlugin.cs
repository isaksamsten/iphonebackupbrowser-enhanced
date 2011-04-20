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

        /// <summary>
        /// Get an instance of a the selection model
        /// </summary>
        /// <param name="model"></param>
        void SetModel(SelectionModel model);

        Form Open();

        // TO BE ADDED
        //string Name { get; }
        //string Author { get; }
        //string Description { get; }
        //Icon Icon { get; }



    }
}
