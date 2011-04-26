using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDevice.Plugins;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using IDevice.IPhone;

namespace IDevice.Plugins.Analyzers.Image
{
    public class ImageAnalyzerPlugin : AbstractPlugin
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ToolStripMenuItem start = new ToolStripMenuItem("Image analyzer");
        public ImageAnalyzerPlugin()
        {
            start.Click += new EventHandler(start_Click);
        }

        void start_Click(object sender, EventArgs e)
        {
            IPhoneBackup backup = SelectedBackup;
            ImageAnalyzer analyzer = new ImageAnalyzer(backup);
            analyzer.Show();
        }

        public override string Author
        {
            get { return "Isak Karlsson"; }
        }

        public override string Description
        {
            get { return "Analyze the image content of this backup"; }
        }

        public override string Name
        {
            get { return "ImageAnalyser"; }
        }

        public override Icon Icon
        {
            get { return null; }
        }

        public override void RegisterMenu(Managers.MenuManager manager)
        {
            manager.Add(Managers.MenuContainer.Analyzer, start);
        }

        public override void UnregisterMenu(Managers.MenuManager manager)
        {
            manager.Remove(Managers.MenuContainer.Analyzer, start);
        }
    }
}
