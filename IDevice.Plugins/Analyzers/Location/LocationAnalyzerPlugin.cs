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
            IPhoneFile file = app.Files.FirstOrDefault(t => t.Path.Contains("consolidated"));

            FileInfo fileInfo = FileManager.GetWorkingFile(backup, file);
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

                        worker.ReportProgress(50);

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
            get { return "LocationAnalyzerPlugin"; }
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
