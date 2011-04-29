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
            _manager.Added += new EventHandler<PluginArgs>(_manager_Added);
            _manager.Removed += new EventHandler<PluginArgs>(_manager_Removed);
            Properties.Settings.Default.SettingsSaving += new System.Configuration.SettingsSavingEventHandler(Default_SettingsSaving);
            InitializeComponent();
        }

        void Default_SettingsSaving(object sender, CancelEventArgs e)
        {
            assemblies.Items.Clear();
            foreach (string str in Properties.Settings.Default.EnabledPlugins)
            {
                assemblies.Items.Add(str);
            }
        }

        void _manager_Removed(object sender, PluginArgs e)
        {
            //PluginManagerWindow_Load(sender, e);
        }

        void _manager_Added(object sender, PluginArgs e)
        {
            //PluginManagerWindow_Load(sender, e);
        }

        private void PluginManagerWindow_Load(object sender, EventArgs e)
        {
            iPluginBindingSource.Clear();
            IEnumerable<PluginInfo> data = _manager.Plugins.Select(x => new PluginInfo
            {
                Name = x.Name,
                Enabled = _manager.Enabled(x.Name)
            });

            iPluginBindingSource.DataSource = data.ToArray();

            assemblies.Items.Clear();
            foreach (string str in Properties.Settings.Default.EnabledPlugins)
            {
                assemblies.Items.Add(str);
            }
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
                _manager.Load(name);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            string assembly = assemblies.SelectedItem as string;
            _manager.Unload(assembly);

            // TODO remove .dll
        }
    }

    public class PluginInfo
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}
