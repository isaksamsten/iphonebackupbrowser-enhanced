using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using IDevice.Plugins;
using System.Collections.Specialized;
using NLog;

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

    public class PluginException : Exception
    {
        public PluginException(string msg, Exception e) : base(msg, e) { }
        public PluginException(string msg) : base(msg) { }
    }

    public class PluginManager : IEnumerable<IPlugin>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                Logger.ErrorException(e.Message, e);
                throw new PluginException("Could not load plugin", e);
            }

        }

        public void Load(string assembly)
        {
            try
            {
                Load(Assembly.Load(assembly));
            }
            catch (Exception e)
            {
                string msg = "Failed to load assembly " + assembly;
                Logger.ErrorException(msg, e);
                throw new PluginException(msg, e);
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
            try
            {
                Unload(Assembly.Load(assembly));
            }
            catch (Exception e)
            {
                string msg = "Failed to unload assembly " + assembly;
                Logger.ErrorException(msg, e);
                throw new PluginException(msg, e);
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
            if (!_plugins.Contains(plugin))
            {
                _plugins.Add(plugin);
                OnAdded(plugin);
            }
        }

        public void Remove(IPlugin plugin)
        {
            IPlugin removed = _plugins.FirstOrDefault(x => x.Name == plugin.Name);
            if (removed != null)
            {
                _plugins.Remove(plugin);
                OnRemoved(removed);
            }
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
                throw new PluginException("No such plugin to enable");
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
                throw new PluginException("No such plugin to disable");
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
