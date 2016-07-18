using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWKC.Alarm.Common;
using System.ServiceModel;
using System.Reflection;
using System.IO;
using System.Media;
using System.Xml.Serialization;

namespace NWKC.Alarm.Service
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single, 
        IncludeExceptionDetailInFaults = true, 
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    class AlarmSchedule : IAlarmControls
    {
        Dictionary<int, AlarmDescription> _alarms;
        HashSet<int> _activeAlarms;
        double _lastTickTime;
        SoundPlayer _soundPlayer;
        List<IAlarmCallbacks> _clients;
        object _loadSaveLock;
        bool _soundIsPlaying;
        static int _nextId = 1;

        public AlarmSchedule()
        {
            _loadSaveLock = new object();
            _alarms = new Dictionary<int, AlarmDescription>();
            _activeAlarms = new HashSet<int>();
            _lastTickTime = Helpers.SecondsSinceMidnight(DateTime.Now);
            _clients = new List<IAlarmCallbacks>();

            var assembly = Assembly.GetExecutingAssembly();
            Stream alarmAudioStream = Helpers.GetEmbeddedResource("alarm.wav", assembly);
            _soundPlayer = new SoundPlayer(alarmAudioStream);
            _soundIsPlaying = false;

            LoadAlarms();
        }

        string SettingsPath
        {
            get
            {
                string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NWKCAlarm");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                return Path.Combine(directory, "settings.xml");
            }
        }

        void LoadAlarms()
        {
            lock (_loadSaveLock)
            {
                AlarmDescription[] alarms = null;

                try
                {
                    var deserializer = new XmlSerializer(typeof(AlarmDescription[]));
                    using (var stream = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read))
                    {
                        alarms = deserializer.Deserialize(stream) as AlarmDescription[];
                    }
                }
                catch (Exception ex)
                { }

                if (alarms != null)
                {
                    foreach (var alarm in alarms)
                    {
                        CreateAlarmInternal(alarm);
                    }
                }
            }
        }

        void SaveAlarms()
        {
            lock (_loadSaveLock)
            {
                AlarmDescription[] alarms;
                lock (_alarms)
                {
                    alarms = _alarms.Values.ToArray();
                }

                try
                {
                    var serializer = new XmlSerializer(typeof(AlarmDescription[]));
                    using (var stream = new FileStream(SettingsPath, FileMode.Create, FileAccess.Write))
                    {
                        serializer.Serialize(stream, alarms);
                    }
                }
                catch (Exception ex)
                { }
            }
        }

        public void ConnectToAlarmService()
        {
            var client = OperationContext.Current.GetCallbackChannel<IAlarmCallbacks>();
            lock (_clients)
            {
                if (client != null && !_clients.Contains(client))
                {
                    _clients.Add(client);
                }
            }
        }

        public int CreateAlarm(AlarmDescription alarmDescription)
        {
            int id = CreateAlarmInternal(alarmDescription);
            SaveAlarms();
            return id;
        }

        int CreateAlarmInternal(AlarmDescription alarmDescription)
        {
            lock(_alarms)
            {
                int id = _nextId++;
                _alarms.Add(id, alarmDescription);
                return id;
            }
        }

        public void DeleteAlarm(int alarmId)
        {
            lock (_alarms)
            {
                if (_alarms.ContainsKey(alarmId))
                {
                    _alarms.Remove(alarmId);
                }
            }
        }

        void ActivateAlarm(int alarmId)
        {
            lock (_activeAlarms)
            {
                _activeAlarms.Add(alarmId);

                if (!_soundIsPlaying)
                {
                    _soundPlayer.PlayLooping();
                    _soundIsPlaying = true;
                }
            }

            lock (_clients)
            {
                List<IAlarmCallbacks> invalidClients = new List<IAlarmCallbacks>();

                foreach (var client in _clients)
                {
                    try
                    {
                        client.AlarmBecameActive(alarmId);
                    }
                    catch (Exception ex)
                    {
                        invalidClients.Add(client);
                    }
                }

                foreach (var invalidClient in invalidClients)
                {
                    _clients.Remove(invalidClient);
                }
            }
        }

        public void DismissActiveAlarm(int alarmId)
        {
            lock (_activeAlarms)
            {
                if (_activeAlarms.Contains(alarmId))
                {
                    _activeAlarms.Remove(alarmId);
                }

                if (_activeAlarms.Count == 0 && _soundIsPlaying)
                {
                    _soundPlayer.Stop();
                    _soundIsPlaying = false;
                }
            }
        }

        public int[] EnumerateActiveAlarms()
        {
            lock (_activeAlarms)
            {
                return _activeAlarms.ToArray();
            }
        }

        public int[] EnumerateAlarms()
        {
            lock (_alarms)
            {
                return _alarms.Keys.ToArray();
            }
        }

        public AlarmDescription GetAlarmDescriptionById(int id)
        {
            lock (_alarms)
            {
                if (_alarms.ContainsKey(id))
                {
                    return _alarms[id];
                }
                else
                {
                    return null;
                }
            }
        }

        public void Tick()
        {
            var now = DateTime.Now;
            double tickTime = Helpers.SecondsSinceMidnight(now);
            List<int> alarmsToActivate = new List<int>();

            lock (_alarms)
            {
                foreach (var key in _alarms.Keys)
                {
                    var desc = _alarms[key];

                    if (now.DayOfWeek == desc.DayOfWeek)
                    {
                        if (desc.SecondsSinceMidnight <= tickTime && // Alarm time met or exceeded
                            desc.SecondsSinceMidnight > _lastTickTime) // We haven't already fired the alarm for this item
                        {
                            alarmsToActivate.Add(key);
                        }
                    }
                }

                _lastTickTime = tickTime;
            }

            foreach (var alarmId in alarmsToActivate)
            {
                ActivateAlarm(alarmId);
            }
        }
    }
}
