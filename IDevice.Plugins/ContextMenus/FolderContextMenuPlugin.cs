using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IDevice.IPhone;

namespace IDevice.Plugins.ContextMenus
{
    public class FolderContextMenuPlugin : AbstractPlugin
    {
        private ToolStripMenuItem open = new ToolStripMenuItem("Open");

        public FolderContextMenuPlugin()
        {
            open.Click += new EventHandler(open_Click);
        }

        void open_Click(object sender, EventArgs e)
        {
            IPhoneApp app = SelectedApp;
            if (app != null)
            {
                SelectionModel.Select(app);
            }
        }

        public override void RegisterMenu(Managers.MenuManager manager)
        {
            manager.Add(Managers.MenuContainer.FolderContext, open);
        }

        public override void UnregisterMenu(Managers.MenuManager manager)
        {
            manager.Remove(Managers.MenuContainer.FolderContext, open);
        }

        public override string Author
        {
            get { return "Isak Karlsson"; }
        }

        public override string Description
        {
            get { return "Give menu items to the folder context menu"; }
        }

        public override string Name
        {
            get { return "FolderContextMenuPlugin"; }
        }

        public override System.Drawing.Icon Icon
        {
            get { return null; }
        }

        public override void Dispose()
        {
            base.Dispose();
            open.Click -= new EventHandler(open_Click);
        }
    }
}
