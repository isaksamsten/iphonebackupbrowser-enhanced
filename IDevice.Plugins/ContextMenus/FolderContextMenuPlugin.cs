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
        private ToolStripMenuItem explorer = new ToolStripMenuItem("Open folder in explorer");
        private ToolStripSeparator sep1 = new ToolStripSeparator();
        private ToolStripMenuItem info = new ToolStripMenuItem("Properties");

        public FolderContextMenuPlugin()
        {
            open.Click += new EventHandler(open_Click);
            open.Enabled = false;

            explorer.Enabled = false;

            info.Enabled = false;
        }

        protected override void OnSelectionChanged(object sender, EventArgs e)
        {
            open.Enabled = SelectedApp != null;
        }

        void open_Click(object sender, EventArgs e)
        {
            IPhoneApp app = SelectedApp;
            if (app != null)
            {
                SelectionModel.Select(app);
            }
        }

        protected override void OnRegisterMenu(Managers.MenuManager manager)
        {
            manager.Add(Managers.MenuContainer.FolderContext, open);
            manager.Add(Managers.MenuContainer.FolderContext, explorer);
            manager.Add(Managers.MenuContainer.FolderContext, sep1);
            manager.Add(Managers.MenuContainer.FolderContext, info);
        }

        protected override void OnUnregisterMenu(Managers.MenuManager manager)
        {
            manager.Remove(Managers.MenuContainer.FolderContext, open);
            manager.Remove(Managers.MenuContainer.FolderContext, explorer);
            manager.Remove(Managers.MenuContainer.FolderContext, sep1);
            manager.Remove(Managers.MenuContainer.FolderContext, info);
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
