using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using IDevice.Plugins.Browsers;
using IDevice.Plugins;

namespace IDevice
{
    public class RegisterEventArgs : EventArgs
    {
        public RegisterEventArgs(IBrowsable browser)
        {
            Browser = browser;
        }

        public IBrowsable Browser { get; private set; }
    }

    /// <summary>
    /// This one need a boot strapper to load Browsers at boot time
    /// </summary>
    public class BrowserManager : IEnumerable<KeyValuePair<string, IBrowsable>>
    {
        public event EventHandler<RegisterEventArgs> Registered;
        private Dictionary<string, IBrowsable> _browsers;
        private PluginManager _manager;
        public  BrowserManager(PluginManager manager)
        {
            _browsers = new Dictionary<string, IBrowsable>();
            foreach (IPlugin p in manager)
            {
                if (p is IBrowsable)
                {
                    IBrowsable b = p as IBrowsable;
                    Add(b.Prefix, b);
                }
            }

            _manager = manager;

            _manager.Added += new EventHandler<PluginArgs>(_manager_Added);
            _manager.Removed += new EventHandler<PluginArgs>(_manager_Removed);
        }

        void _manager_Removed(object sender, PluginArgs e)
        {
            IBrowsable p = e.Plugin as IBrowsable;
            if (p != null)
            {
                Remove(p.Prefix);   
            }
        }

        void _manager_Added(object sender, PluginArgs e)
        {
            IBrowsable p = e.Plugin as IBrowsable;
            if (p != null)
            {
                Add(p.Prefix, p);
            }
        }

        public void Add(string prefix, IBrowsable browser)
        {
            _browsers[prefix] = browser;
        }

        public void Add(string prefix, Type browser)
        {
            Add(prefix, (IBrowsable)Activator.CreateInstance(browser));
        }

        public void Remove(string prefix)
        {
            _browsers.Remove(prefix);
        }

        public IBrowsable Get(string prefix)
        {
            IBrowsable browsable = null;
            _browsers.TryGetValue(prefix, out browsable);

            return browsable != null ? browsable : _browsers["*"];
        }

        public bool CanHandle(string prefix)
        {
            return _browsers.ContainsKey(prefix) && _browsers[prefix] != null;
        }


        public IEnumerator<KeyValuePair<string, IBrowsable>> GetEnumerator()
        {
            return _browsers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _browsers.GetEnumerator();
        }

        public void OnRegistered(IBrowsable b)
        {
            if (Registered != null)
                Registered(this, new RegisterEventArgs(b));
        }
    }
}
