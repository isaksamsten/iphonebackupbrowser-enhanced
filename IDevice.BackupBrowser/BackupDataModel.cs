using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDevice.IPhone;

namespace IDevice
{
    public class BackupDataModel
    {
        private static BackupDataModel _model;

        public static BackupDataModel Current
        {
            get
            {
                if (_model == null)
                    _model = new BackupDataModel();

                return _model;
            }
        }

        private object _lock = new object();

        private BackupDataModel() { }

        private List<IPhoneApp> _apps = new List<IPhoneApp>();
        public IPhoneApp[] AppData
        {
            get
            {
                lock (_apps)
                {
                    return _apps.ToArray();
                }
            }
        }

        public void Clear()
        {
            lock (_apps)
            {
                _apps.Clear();
            }
        }

        public void Add(IPhoneApp app)
        {
            lock (_apps)
            {
                _apps.Add(app);
            }
        }

        public void Add(IEnumerable<IPhoneApp> apps)
        {
            lock (_apps)
            {
                _apps.AddRange(apps);
            }
        }
    }
}
