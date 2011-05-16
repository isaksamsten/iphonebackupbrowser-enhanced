using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Google.KML;
using System.Drawing;
using System.Drawing.Imaging;

namespace IDevice.Plugins.Analyzers.Location
{
    /// <summary>
    /// Simple class to represent a location
    /// </summary>
    public class Location
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Name { get; set; }
        public object Tag { get; set; }
        public DateTime Time { get; set; }

        public static geKML ToKML(IEnumerable<Location> locations)
        {
            geDocument doc = new geDocument();
            doc.Name = "Locations exported";
            foreach (var loc in locations)
            {
                gePlacemark pm = new gePlacemark();

                //Create some coordinates for the point at which
                //this placemark will sit. (Lat / Lon)
                geCoordinates coords = new geCoordinates(new geAngle90(loc.Latitude), new geAngle180(loc.Longitude));

                //Create a point with these new coordinates
                gePoint point = new gePoint(coords);

                //Assign the point to the Geometry property of the
                //placemark.
                pm.Geometry = point;

                //Now lets add some other properties to our placemark
                pm.Name = loc.Name;
                pm.Description = "Visit at: " + loc.Time.ToString();
                //Finally, add the placemark to the document
                doc.Features.Add(pm);
            }
            //Now that we have our document, lets create our KML
            geKML kml = new geKML(doc);
            return kml;
        }
    }
}
