using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using IDevice.Managers;

namespace IDevice.Plugins
{
    public interface IPlugin : IComparable<IPlugin>, IDisposable
    {
        /// <summary>
        /// Get the menu to be added to the menu
        /// </summary>
        /// <returns></returns>
        void RegisterMenu(MenuManager manager);

        /// <summary>
        /// Unregister my menu
        /// </summary>
        /// <param name="manager"></param>
        void UnregisterMenu(MenuManager manager);

        /// <summary>
        /// Get an instance of a the selection model
        /// </summary>
        /// <param name="model"></param>
        void RegisterModel(SelectionModel model);

        /// <summary>
        /// is this a modal window
        /// </summary>
        bool IsModal { get; }

        /// <summary>
        /// Open this plugin
        /// </summary>
        /// <returns></returns>
        Form Open();

        // TO BE ADDED
        /// <summary>
        /// Name of the plugin
        /// 
        /// It is supposed to be unique
        /// </summary>
        string PluginName { get; }

        /// <summary>
        /// Name of the author
        /// </summary>
        string PluginAuthor { get; }
        
        /// <summary>
        /// Description
        /// </summary>
        string PluginDescription { get; }


        //Icon Icon { get; }



    }
}
