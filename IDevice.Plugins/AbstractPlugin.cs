using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using IDevice.IPhone;
using IDevice.Managers;
using System.Drawing;

namespace IDevice.Plugins
{
    public abstract class AbstractPlugin : IPlugin
    {
        private BrowserModel _selectionModel;
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
        protected virtual BrowserModel SelectionModel
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
        /// 
        /// </summary>
        protected virtual IPhoneApp SelectedApp
        {
            get { return _selectionModel.App; }
        }

        /// <summary>
        /// Get a file manager helper
        /// </summary>
        protected virtual FileManager FileManager
        {
            get { return _fileManager; }
        }

        /// <summary>
        /// Register the menu
        /// </summary>
        /// <param name="manager"></param>
        protected virtual void OnRegisterMenu(MenuManager manager)
        {

        }

        /// <summary>
        /// Un register when not needed
        /// </summary>
        /// <param name="manager"></param>
        protected virtual void OnUnregisterMenu(MenuManager manager)
        {

        }

        /// <summary>
        /// After we are registerd
        /// </summary>
        protected virtual void OnPostRegister()
        {

        }

        /// <summary>
        /// after we are un registered
        /// </summary>
        protected virtual void OnPostUnregister()
        {

        } 

        /// <summary>
        /// Standard to register a model and a selection changed
        /// 
        /// </summary>
        /// <param name="model"></param>
        protected virtual void OnRegisterModel(BrowserModel model)
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
        /// Set a setting value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetSetting(string key, string value)
        {
            IDevice.Properties.Settings.Default.Plugin.Add(key, value);
        }

        /// <summary>
        /// Retrive a setting value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetSetting(string key)
        {
            return IDevice.Properties.Settings.Default.Plugin.Get(key);
        }

        /// <summary>
        /// Save the settings to the next run
        /// </summary>
        public void PersistSetting()
        {
            IDevice.Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Dispose me!
        /// 
        /// Subclass: remember to base.Dispose()!
        /// </summary>
        public virtual void Dispose()
        {
            _selectionModel.Changed -= new EventHandler(OnSelectionChanged);
        }

        public void Register(IRegisterArgs args)
        {
            OnRegisterModel(args.SelectionModel);
            OnRegisterMenu(args.MenuManager);

            OnPostRegister();
        }

        public void Unregister(IRegisterArgs args)
        {
            OnUnregisterMenu(args.MenuManager);
            OnPostUnregister();
        }
    }
}
