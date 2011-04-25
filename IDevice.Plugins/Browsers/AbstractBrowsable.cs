using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using IDevice.IPhone;
using IDevice.Managers;

namespace IDevice.Plugins.Browsers
{
    public abstract class AbstractPlugin : Form
    {
        private SelectionModel _selectionModel;
        private FileManager _fileManager;

        protected AbstractPlugin()
        {
            _fileManager = new FileManager();
        }

        protected SelectionModel SelectionModel
        {
            get { return _selectionModel; }
        }

        protected virtual IPhoneFile[] SelectedFiles
        {
            get { return _selectionModel.Files; }
        }

        protected virtual IPhoneBackup SelectedBackup
        {
            get { return _selectionModel.Backup; }
        }

        protected virtual FileManager FileManager
        {
            get { return _fileManager; }
        }

        /// <summary>
        /// Default to a modal window
        /// </summary>
        public virtual bool IsModal { get { return true; } }

        /// <summary>
        /// Invoked before the form is opened.
        /// 
        /// Conviniet to use to initizlize here
        /// </summary>
        protected virtual void PreOpen()
        {

        }

        public virtual Form Open()
        {
            PreOpen();
            return this;
        }


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
        public void RegisterModel(SelectionModel model)
        {
            _selectionModel = model;
            _selectionModel.Changed += new EventHandler(OnSelectionChanged);
        }

        /// <summary>
        /// Invoked when the selection modelse selection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnSelectionChanged(object sender, EventArgs e)
        {

        }

        #region plugin info

        public abstract string PluginAuthor { get; }

        //public virtual string PluginAuthor
        //{
        //    get { return "none"; }
        //}

        public abstract string PluginDescription { get; }

        public abstract string PluginName { get; }

        #endregion

        #region Equality

        public override int GetHashCode()
        {
            return PluginName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IPlugin))
                return false;

            return CompareTo(obj as IPlugin) == 0;
        }

        public int CompareTo(IPlugin other)
        {
            return PluginName.CompareTo(other.PluginName);
        }

        #endregion
    }
}
