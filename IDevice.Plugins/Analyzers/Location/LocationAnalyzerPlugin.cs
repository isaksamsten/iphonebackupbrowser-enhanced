using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDevice.Plugins.Analyzers.Location
{
    public class LocationAnalyzerPlugin : AbstractPlugin
    {
        public override string Author
        {
            get { return "Isak Karlsson"; }
        }

        public override string Description
        {
            get { return "An plugin that analyzes the location data availible in this backup"; }
        }

        public override string Name
        {
            get { return "LocationAnalyzerPlugin"; }
        }

        public override System.Drawing.Icon Icon
        {
            get { return null; }
        }
    }
}
