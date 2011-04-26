using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IDevice.Managers
{
    public enum MenuContainer
    {
        Base,
        File,
        Edit,
        View,
        Export,
        Analyzer,
        Plugin,

        Custom
    }

    public class MenuEvent : EventArgs
    {
        public MenuEvent(ToolStripItem itm, MenuContainer at)
        {
            Item = itm;
            At = at;
        }

        public string Name { get { return Item.Name; } }
        public ToolStripItem Item { get; private set; }
        public MenuContainer At { get; private set; }
    }

    public class MenuManager
    {
        public event EventHandler<MenuEvent> Added;
        public event EventHandler<MenuEvent> Removed;
        //public event EventHandler<MenuEvent> Changed;

        private Dictionary<MenuContainer, List<ToolStripItem>> _items;
        private MenuStrip _strip;

        public MenuManager(/*MenuStrip strip*/)
        {
            //_strip = strip;
            _items = new Dictionary<MenuContainer, List<ToolStripItem>>();
        }

        public void Add(MenuContainer container, ToolStripItem itm)
        {
            if (!_items.ContainsKey(container))
                _items.Add(container, new List<ToolStripItem>());

            _items[container].Add(itm);

            OnAdded(container, itm);
        }

        public void Remove(MenuContainer container, ToolStripItem itm)
        {
            if (!_items.ContainsKey(container))
                return;
            List<ToolStripItem> list = _items[container];
            list.Remove(itm);

            OnRemoved(container, itm);
        }

        public ToolStripItem Find(MenuContainer at, string name)
        {
            if (!_items.ContainsKey(at))
                return null;

            return _items[at].FirstOrDefault(x => x.Name == name);
        }

        protected virtual void OnAdded(MenuContainer container, ToolStripItem itm)
        {
            if (Added != null)
                Added(this, new MenuEvent(itm, container));
        }

        protected virtual void OnRemoved(MenuContainer container, ToolStripItem itm)
        {
            if (Removed != null)
                Removed(this, new MenuEvent(itm, container));
        }
    }
}
