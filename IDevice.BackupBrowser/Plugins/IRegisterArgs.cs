using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDevice.Plugins
{
    public interface IRegisterArgs
    {
        Managers.MenuManager MenuManager { get; }
        BrowserModel SelectionModel { get; }
    }
}
