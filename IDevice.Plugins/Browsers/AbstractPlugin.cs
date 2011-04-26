using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using IDevice.IPhone;
using IDevice.Managers;
using System.Drawing;

namespace IDevice.Plugins.Browsers
{
    public abstract class AbstractPlugin : IPlugin
    {
        private SelectionModel _selectionModel;
        private FileManager _fileManager;

        /// <summary>
        /// Initializer
        /// </summary>
        protected AbstractPlugin()
        {
            _fileManager = new FileManager();
        }

        /// <summary>
        /// Get the selection model
        /// </summary>
        protected SelectionModel SelectionModel
        {
            get { return _selectionModel; }
        }

        /// <summary>
        /// Get the currently selected files using the SelectionModel
        /// </summary>
        protected virtual IPhoneFile[] SelectedFiles
        {
            get { return _selectionModel.Files; }
        }

        /// <summary>
        /// GEt the currently selected backup
        /// </summary>
        protected virtual IPhoneBackup SelectedBackup
        {
            get { return _selectionModel.Backup; }
        }

        /// <summary>
        /// Get a file manager helper
        /// </summary>
        protected virtual FileManager FileManager
        {
            get { return _fileManager; }
        }

        /// <summary>
        /// Default to a modal window
        /// </summary>
        public virtual bool Modal { get { return true; } }


        /// <summary>
        /// Register the menu
        /// </summary>
        /// <param name="manager"></param>
        public virtual void RegisterMenu(MenuManager manager)
        {

        }

        /// <summary>
        /// Un register when not needed
        /// </summary>
        /// <param name="manager"></param>
        public virtual void UnregisterMenu(MenuManager manager)
        {

        }

        /// <summary>
        /// Standard to register a model and a selection changed
        /// 
        /// </summary>
        /// <param name="model"></param>
        public virtual void RegisterModel(SelectionModel model)
        {
            _selectionModel = model;
            _selectionModel.Changed += new EventHandler(OnSelectionChanged);
        }

        /// <summary>
        /// Invoked when the selection modelse selection changes
        /// 
        /// Override to know when a selection change in the main
        /// window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnSelectionChanged(object sender, EventArgs e)
        {

        }

        #region plugin info

        /// <summary>
        /// The name of this plugin author
        /// </summary>
        public abstract string Author { get; }

        /// <summary>
        /// The description
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// The name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The icon
        /// </summary>
        public abstract Icon Icon { get; }

        #endregion

        #region Equality

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IPlugin))
                return false;

            return CompareTo(obj as IPlugin) == 0;
        }

        public int CompareTo(IPlugin other)
        {
            return Name.CompareTo(other.Name);
        }

        #endregion

        /// <summary>
        /// Dispose me!
        /// 
        /// Subclass: remember to base.Dispose()!
        /// </summary>
        public virtual void Dispose()
        {
            _selectionModel.Changed -= new EventHandler(OnSelectionChanged);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Form Open();
    }
}
