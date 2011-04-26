using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IDevice.IPhone;

namespace IDevice.Plugins.Analyzers.Image
{
    public partial class ImageAnalyzer : Form
    {
        private IPhoneBackup _backup;
        public ImageAnalyzer(IPhoneBackup backup)
        {
            _backup = backup;
            InitializeComponent();


            this.Text += " [" + _backup.DisplayName + "]";
        }
    }
}
