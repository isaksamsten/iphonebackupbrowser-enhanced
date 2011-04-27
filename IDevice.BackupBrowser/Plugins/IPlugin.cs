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
        /// 
        /// </summary>
        /// <param name="args"></param>
        void Register(IRegisterArgs args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        void Unregister(IRegisterArgs args);


        // TO BE ADDED
        /// <summary>
        /// Name of the plugin
        /// 
        /// It is supposed to be unique
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Name of the author
        /// </summary>
        string Author { get; }
        
        /// <summary>
        /// Description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Icon
        /// </summary>
        Icon Icon { get; }
    }
}
