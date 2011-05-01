using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using Google.KML;
using System.IO;
using IDevice.IPhone;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using LevDan.Exif;
using System.Text.RegularExpressions;

namespace IDevice.Plugins.Analyzers.Location
{
    public class LocationAnalyzerPlugin : AbstractPlugin
    {
        private ToolStripMenuItem _analyze = new ToolStripMenuItem("Locations");

        public LocationAnalyzerPlugin()
        {
            _analyze.Click += new EventHandler(_analyze_Click);
        }

        void _analyze_Click(object sender, EventArgs e)
        {
            IPhoneBackup backup = SelectedBackup;
            IPhoneApp app = backup.GetApps().FirstOrDefault(t => t.Name == "System");
            IPhoneFile dbFile = app.Files.FirstOrDefault(t => t.Path.Contains("consolidated"));
            IEnumerable<IPhoneFile> imgs = app.Files.Where(t => t.Path.Contains("DCIM/100APPLE") && t.Domain == "MediaDomain");
            FileInfo fileInfo = FileManager.GetWorkingFile(backup, dbFile);
            Model.InvokeAsync(delegate(object w, DoWorkEventArgs a)
            {
                BackgroundWorker worker = w as BackgroundWorker;
                if (!worker.CancellationPending)
                {
                    List<Location> locations = new List<Location>();
                    using (SQLiteConnection con = new SQLiteConnection(@"Data Source=" + fileInfo.FullName))
                    {
                        con.Open();
                        var cmd = new SQLiteCommand();
                        cmd.Connection = con;
                        cmd.CommandText = "SELECT * FROM WifiLocation;";

                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            locations.Add(new Location
                            {
                                Name = "[Wifi] MAC: " + reader.GetString(0),
                                Time = new DateTime(2001, 1, 1, 0, 0, 0).AddSeconds(reader.GetDouble(1)),
                                Latitude = reader.GetDouble(2),
                                Longitude = reader.GetDouble(3)
                            });
                        }
                        reader.Close();

                        worker.ReportProgress(33);

                        cmd.CommandText = "SELECT * FROM CellLocation";
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            locations.Add(new Location
                            {
                                Name = "[Cell] MCC: " + reader.GetInt32(0) + " MNC: " +
                                                        reader.GetInt32(1) + " LAC: " +
                                                        reader.GetInt32(2) + " CELL ID:" +
                                                        reader.GetInt32(3),
                                Time = new DateTime(2001, 1, 1, 0, 0, 0).AddSeconds(reader.GetDouble(4)),
                                Latitude = reader.GetDouble(5),
                                Longitude = reader.GetDouble(6)
                            });
                        }

                        worker.ReportProgress(33);
                    }

                    int count = imgs.Count() + 66, current = 66;
                    foreach (IPhoneFile file in imgs)
                    {
                        FileInfo info = FileManager.GetWorkingFile(backup, file);
                        ExifTagCollection er = new ExifTagCollection(info.FullName);

                        //not working...
                        Location location = new Location();
                        double lat = 0, lon = 0;
                        foreach (ExifTag tag in er)
                        {
                            if (tag.FieldName == "GPSLatitude")
                            {
                                string dec = Regex.Match(tag.Value, @"\d+\.\d+").ToString();
                                double.TryParse(dec, out lat);
                            }
                            else if (tag.FieldName == "GPSLongitude")
                            {
                                string dec = Regex.Match(tag.Value, @"\d+\.\d+").ToString();
                                double.TryParse(dec, out lon);
                            }
                        }
                        location.Latitude = lat;
                        location.Longitude = lon;
                        location.Name = "[Image] " + info.Name;
                        locations.Add(location);

                        worker.ReportProgress(BrowserModel.Percent(current++, count));
                    }

                    geKML kml = Location.ToKML(locations.OrderBy(x => x.Time));
                    a.Result = kml;
                }
                else
                {
                    a.Cancel = true;
                    return;
                }
            },
            delegate(object w, RunWorkerCompletedEventArgs a)
            {
                geKML kml = a.Result as geKML;

                FileDialog dialog = new SaveFileDialog();
                dialog.DefaultExt = "kmz";
                dialog.FileName = "locations.kmz";
                dialog.Filter = "Google earth|*.kmz";
                if (dialog.ShowDialog(Model.Window) == DialogResult.OK)
                {
                    using (FileStream s = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        using (BinaryWriter wr = new BinaryWriter(s))
                        {
                            wr.Write(kml.ToKMZ());
                        }
                    }
                }
            }, "Analyzing location", false);
        }

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
            get { return "LocationAnalyzer"; }
        }

        public override System.Drawing.Icon Icon
        {
            get { return null; }
        }

        protected override void OnRegisterMenu(Managers.MenuManager manager)
        {
            manager.Add(Managers.MenuContainer.Analyzer, _analyze);
        }

        protected override void OnUnregisterMenu(Managers.MenuManager manager)
        {
            manager.Remove(Managers.MenuContainer.Analyzer, _analyze);
        }
    }
}
