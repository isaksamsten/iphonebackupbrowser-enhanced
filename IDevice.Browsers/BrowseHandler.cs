using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace IDevice.Browsers
{
    /// <summary>
    /// This one need a boot strapper to load Browsers at boot time
    /// </summary>
    public class BrowseHandler : IEnumerable<KeyValuePair<string, IBrowsable>>
    {
        public event EventHandler<EventArgs> Register;

        private static BrowseHandler _handler;
        public static BrowseHandler Current
        {
            get
            {
                if (_handler == null)
                    _handler = new BrowseHandler();
                return _handler;
            }
        }
        private Dictionary<string, IBrowsable> _browsers;
        private BrowseHandler()
        {
            _browsers = new Dictionary<string, IBrowsable>();
        }

        public void Add(string prefix, IBrowsable browser)
        {
            _browsers.Add(prefix, browser);
        }

        public void Add(string prefix, Type browser)
        {
            _browsers.Add(prefix, (IBrowsable)Activator.CreateInstance(browser));
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

        private bool _hasInit = false;
        public void Initialize()
        {
            if (!_hasInit)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                IEnumerable<Type> types = assembly
                    .GetTypes()
                    .Where(t => t.GetInterfaces().Any(p => p == (typeof(IBrowsable))) && !t.IsAbstract);
                foreach(Type t in types) 
                {
                    ((IBrowsable)Activator.CreateInstance(t)).Register();
                }

            }
        }

        public bool IsInitialized
        {
            get { return _hasInit; }
        }

        protected virtual void OnRegister()
        {
            if (Register != null)
                Register(this, EventArgs.Empty);
        }
    }
}
