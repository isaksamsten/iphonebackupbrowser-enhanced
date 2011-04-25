using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using IDevice.Plugins;

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
        private List<string> _blacklist = new List<string>();

        public PluginManager(string[] assmblies, string[] blacklist)
        {
            try
            {
                _blacklist.AddRange(blacklist);
                foreach (string a in assmblies)
                    Load(a);
            }
            catch
            {
                throw new Exception("Pluginerror");
            }

        }

        public void Load(string assembly)
        {
            Load(Assembly.Load(assembly));
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

        public IEnumerator<IPlugin> GetEnumerator()
        {
            return _plugins.Where(t => !_blacklist.Contains(t.PluginName)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
