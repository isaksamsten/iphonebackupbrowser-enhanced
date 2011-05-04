using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IDevice.Plugins.Menus
{
    public class OpenWithPlugin : AbstractPlugin
    {
        public override string Author
        {
            get { return "Isak Karlsson"; }
        }

        public override string Description
        {
            get { return "Give the ability to choose browser"; }
        }

        public override string Name
        {
            get { return "Open with plugin"; }
        }

        public override System.Drawing.Icon Icon
        {
            get { return null; }
        }

        private ToolStripMenuItem openWith = new ToolStripMenuItem("Open with...");
        public OpenWithPlugin()
        {
            openWith.Enabled = false;
        }

        protected override void OnSelectionChanged(object sender, EventArgs e)
        {
            if (SelectedFiles != null && SelectedFiles.Count() > 0)
                openWith.Enabled = true;
        }

        protected override void OnPostRegister()
        {
            foreach (IBrowsable browser in Model.BrowserManagers.Distinct())
            {
                var itm = new ToolStripMenuItem(browser.Name);
                itm.Tag = browser;
                itm.Click += delegate(object sender, EventArgs e)
                {
                    IBrowsable b = (sender as ToolStripItem).Tag as IBrowsable;
                    try
                    {
                        Form form = b.Open();
                        if (form != null)
                        {
                            if (b.Modal)
                                form.ShowDialog(Model.Window);
                            else
                                form.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        //Logger.DebugException(ex.Message, ex);
                        MessageBox.Show(string.Format("'{0}' could not be opened by '{1}'"
                                                        + "\n\n'{2}'"
                                                        + "\nStacktrace\n{3}", SelectedFiles.FirstOrDefault().Path, b.Name,
                                                        ex.Message, ex.StackTrace), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                openWith.DropDownItems.Add(itm);
            }
        }

        protected override void OnRegisterMenu(Managers.MenuManager manager)
        {
            manager.Add(Managers.MenuContainer.FileContext, openWith);
        }

        protected override void OnUnregisterMenu(Managers.MenuManager manager)
        {
            manager.Remove(Managers.MenuContainer.FileContext, openWith);
        }
    }
}
