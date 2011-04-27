using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDevice.Managers;

namespace IDevice.Plugins
{
    public class RegisterArgs : IRegisterArgs
    {
        private MenuManager _manager;
        private IDevice.BrowserModel _selection;
        public RegisterArgs(MenuManager manager, BrowserModel selection)
        {
            _manager = manager;
            _selection = selection;
        }
        #region IRegister Members

        public Managers.MenuManager MenuManager
        {
            get { return _manager; }
        }

        public BrowserModel SelectionModel
        {
            get { return _selection; }
        }

        #endregion
    }
}
