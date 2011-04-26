using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IDevice.Managers;
using IDevice.Plugins;

namespace IDevice.Settings
{
    public partial class PluginManagerWindow : Form
    {
        private PluginManager _manager;
        public PluginManagerWindow(PluginManager manager)
        {
            _manager = manager;
            InitializeComponent();
        }

        private void PluginManagerWindow_Load(object sender, EventArgs e)
        {
            IEnumerable<PluginInfo> data = _manager.Plugins.Select(x => new PluginInfo
            {
                Name = x.Name,
                Enabled = _manager.Enabled(x.Name)
            });

            iPluginBindingSource.DataSource = data.ToArray();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            foreach (PluginInfo info in iPluginBindingSource)
            {
                if (!info.Enabled)
                {
                    _manager.Disable(info.Name);
                }
                else
                {
                    _manager.Enable(info.Name);
                }
            }

            Properties.Settings.Default.Save();
            Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

    public class PluginInfo
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}
