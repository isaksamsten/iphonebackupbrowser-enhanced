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
using System.IO;

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
            Reload();
        }

        private void Reload()
        {
            assemblies.Items.Clear();
            foreach (string str in Properties.Settings.Default.EnabledPlugins)
            {
                assemblies.Items.Add(str);
            }

            iPluginBindingSource.Clear();
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

        private void btn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".dll";
            dialog.Filter = "Plugin|*.dll";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                FileInfo info = new FileInfo(dialog.FileName);
                try
                {
                    File.Copy(info.FullName, Path.Combine(".", info.Name));
                }
                catch
                {

                }
                string name = info.Name.Replace(".dll", "");
                try
                {
                    _manager.Load(name);
                    if (!Properties.Settings.Default.EnabledPlugins.Contains(name))
                    {
                        Properties.Settings.Default.EnabledPlugins.Add(name);
                        Properties.Settings.Default.Save();
                    }

                    Reload();
                }
                catch (PluginException pe)
                {
                    MessageBox.Show("Could not load plugin.", "Could not load", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            string assembly = assemblies.SelectedItem as string;
            Properties.Settings.Default.EnabledPlugins.Remove(assembly);
            Properties.Settings.Default.Save();
            try
            {
                _manager.Unload(assembly);
                Reload();
            }
            catch { }
        }
    }

    public class PluginInfo
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}
