using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IDevice.Managers
{
    public enum MenuContainer
    {
        /// <summary>
        /// At the base menu strip
        /// </summary>
        Base,

        /// <summary>
        /// Under the file menu
        /// </summary>
        File,

        /// <summary>
        /// Under the edit menu
        /// </summary>
        Edit,

        /// <summary>
        /// Under the view menu
        /// </summary>
        View,

        /// <summary>
        /// Under the Export menu
        /// </summary>
        Export,

        /// <summary>
        /// Under the analyzer menu
        /// </summary>
        Analyzer,

        /// <summary>
        /// Under the plugin menu (unimplemented)
        /// </summary>
        Plugin,

        /// <summary>
        /// Context menu for the folders
        /// </summary>
        FolderContext,

        /// <summary>
        /// Context menu for the files
        /// </summary>
        FileContext,

        /// <summary>
        /// Use it under an other item.
        /// </summary>
        Custom
    }

    public class MenuEvent : EventArgs
    {
        public MenuEvent(ToolStripItem itm, int index, MenuContainer at)
        {
            Item = itm;
            At = at;
            Index = index;
        }

        public string Name { get { return Item.Name; } }
        public ToolStripItem Item { get; private set; }
        public MenuContainer At { get; private set; }
        public int Index { get; private set; }

        public bool Insert { get { return Index > -1; } }
    }

    public class MenuManager
    {
        public event EventHandler<MenuEvent> Added;
        public event EventHandler<MenuEvent> Removed;

        private Dictionary<MenuContainer, List<ToolStripItem>> _items;
        private MenuStrip _strip;

        public MenuManager()
        {
            _items = new Dictionary<MenuContainer, List<ToolStripItem>>();
        }

        public void Add(MenuContainer container, ToolStripItem itm)
        {
            Insert(container, itm, -1);
        }

        public void Insert(MenuContainer container, ToolStripItem itm, int at)
        {
            if (!_items.ContainsKey(container))
                _items.Add(container, new List<ToolStripItem>());

            _items[container].Add(itm);

            OnAdded(container, itm, at);
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

        protected virtual void OnAdded(MenuContainer container, ToolStripItem itm, int index)
        {
            if (Added != null)
                Added(this, new MenuEvent(itm, index, container));
        }

        protected virtual void OnRemoved(MenuContainer container, ToolStripItem itm)
        {
            if (Removed != null)
                Removed(this, new MenuEvent(itm, -1, container));
        }
    }
}
