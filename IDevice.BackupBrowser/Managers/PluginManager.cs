using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using IDevice.Plugins;
using System.Collections.Specialized;

namespace IDevice.Managers
{

    public static class TypeExtensions
    {
        public static bool HasInterface(this Type t, Type t2)
        {
            return t.GetInterfaces().Any(p => p == (t2));
        }
    }
    public class PluginArgs : EventArgs
    {
        public PluginArgs(IPlugin plugin)
        {
            Plugin = plugin;
        }

        public IPlugin Plugin { get; private set; }
    }

    public class PluginManager : IEnumerable<IPlugin>
    {
        public event EventHandler<PluginArgs> Added;
        public event EventHandler<PluginArgs> Removed;

        private List<IPlugin> _plugins = new List<IPlugin>();
        public PluginManager()
        {
            try
            {
                foreach (string a in Properties.Settings.Default.EnabledPlugins)
                    Load(a);
            }
            catch (Exception e)
            {
                throw new Exception("Pluginerror", e);
            }

        }

        public void Load(string assembly)
        {
            if (!Properties.Settings.Default.EnabledPlugins.Contains(assembly))
                Properties.Settings.Default.EnabledPlugins.Add(assembly);
            try
            {
                Load(Assembly.Load(assembly));
                Properties.Settings.Default.Save();
            }
            catch
            {

            }

        }

        public void Load(Assembly assembly)
        {
            IEnumerable<Type> types = assembly
                .GetTypes()
                .Where(t => t.HasInterface(typeof(IPlugin)) && !t.IsAbstract && !t.IsInterface);

            foreach (Type t in types)
            {
                IPlugin plugin = (IPlugin)Activator.CreateInstance(t);
                Add(plugin);
            }

        }

        public void Unload(string assembly)
        {
            if (Properties.Settings.Default.EnabledPlugins.Contains(assembly))
                Properties.Settings.Default.EnabledPlugins.Remove(assembly);
            try
            {
                Unload(Assembly.Load(assembly));
                Properties.Settings.Default.Save();
            }
            catch
            {
            }
        }

        public void Unload(Assembly assembly)
        {
            IEnumerable<Type> types = assembly
                .GetTypes()
                .Where(t => t.HasInterface(typeof(IPlugin)) && !t.IsAbstract && !t.IsInterface);

            foreach (Type t in types)
            {
                IPlugin plugin = (IPlugin)Activator.CreateInstance(t);
                Remove(plugin);
            }
        }

        public void Add(IPlugin plugin)
        {
            _plugins.Add(plugin);
            OnAdded(plugin);
        }

        public void Remove(IPlugin plugin)
        {
            _plugins.Remove(plugin);
            OnRemoved(plugin);
        }

        protected virtual void OnRemoved(IPlugin p)
        {
            if (Removed != null)
                Removed(this, new PluginArgs(p));
        }

        protected virtual void OnAdded(IPlugin p)
        {
            if (Removed != null)
                Added(this, new PluginArgs(p));
        }

        public IPlugin[] Plugins
        {
            get
            {
                return _plugins.ToArray();
            }
        }

        public void Enable(string name)
        {
            IPlugin plugin = _plugins.FirstOrDefault(p => p.Name == name);
            if (plugin != null)
            {
                if (Properties.Settings.Default.BlacklistedPlugins.Contains(name))
                {
                    Properties.Settings.Default.BlacklistedPlugins.Remove(name);
                    OnAdded(plugin);
                }
            }
            else
            {
                throw new Exception("No such plugin to enable");
            }
        }


        public void Disable(string name)
        {
            IPlugin plugin = _plugins.FirstOrDefault(p => p.Name == name);
            if (plugin != null)
            {
                Properties.Settings.Default.BlacklistedPlugins.Add(name);
                OnRemoved(plugin);
            }
            else
            {
                throw new Exception("No such plugin to disable");
            }
        }

        public bool Enabled(string name)
        {
            return !Properties.Settings.Default.BlacklistedPlugins.Contains(name);
        }

        /// <summary>
        /// Enumerate enabled plugins
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IPlugin> GetEnumerator()
        {
            return _plugins.Where(t => Enabled(t.Name)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
