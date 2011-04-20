using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace IDevice.Plugins.Browsers
{
    public class AbstractBrowsable : Form, IBrowsable
    {
        private string prefix;
        private SelectionModel _selectionModel;

        protected AbstractBrowsable(string prefix)
        {
            this.prefix = prefix;
        }

        public AbstractBrowsable() : this("*******************") { }

        protected SelectionModel SelectionModel
        {
            get { return _selectionModel; }
        }

        protected virtual void PreOpen()
        {

        }

        public virtual Form Open()
        {
            PreOpen();
            return this;
        }

        public virtual ToolStripMenuItem GetMenu()
        {
            throw new NotImplementedException();
        }


        public virtual string Prefix
        {
            get { return this.prefix; }
        }


        public void SetModel(SelectionModel model)
        {
            _selectionModel = model;
        }


        public virtual Form Open(string path)
        {
            throw new NotImplementedException();
        }
    }
}
