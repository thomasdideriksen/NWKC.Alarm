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
using System.Threading;

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
        DateTime _lastTickTime;
        SoundPlayer _soundPlayer;
        List<IAlarmCallbacks> _clients;
        object _lockApi;
        object _lockLoadSave;
        bool _soundIsPlaying;
        static int _nextId = 1;

        public AlarmSchedule()
        {
            _lockApi = new object();
            _lockLoadSave = new object();
            _alarms = new Dictionary<int, AlarmDescription>();
            _activeAlarms = new HashSet<int>();
            _lastTickTime = DateTime.Now;
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
            lock (_lockLoadSave)
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
            AlarmDescription[] alarms = _alarms.Values.ToArray();

            ThreadPool.QueueUserWorkItem((o) =>
            {
                lock (_lockLoadSave)
                {
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
            });
        }

        int CreateAlarmInternal(AlarmDescription alarmDescription)
        {
            int id = _nextId++;
            _alarms.Add(id, alarmDescription);
            return id;
        }

        void ActivateAlarm(int alarmId)
        {
            _activeAlarms.Add(alarmId);

            if (!_soundIsPlaying)
            {
                _soundPlayer.PlayLooping();
                _soundIsPlaying = true;
            }
            
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

        void StopSoundIfNoActiveAlarms()
        {
            if (_activeAlarms.Count == 0 && _soundIsPlaying)
            {
                _soundPlayer.Stop();
                _soundIsPlaying = false;
            }
        }
        
        //
        // Public API methods
        //

        public void ConnectToAlarmService()
        {
            lock (_lockApi)
            {
                var client = OperationContext.Current.GetCallbackChannel<IAlarmCallbacks>();
                if (client != null && !_clients.Contains(client))
                {
                    _clients.Add(client);
                }
            }
        }

        public int CreateAlarm(AlarmDescription alarmDescription)
        {
            lock (_lockApi)
            {
                int id = CreateAlarmInternal(alarmDescription);
                SaveAlarms();
                return id;
            }
        }

        public void DeleteAlarm(int alarmId)
        {
            lock (_lockApi)
            {
                if (_alarms.ContainsKey(alarmId))
                {
                    _alarms.Remove(alarmId);
                }
            }
        }
        
        public void SnoozeActiveAlarm(int alarmId, TimeSpan snoozeTime)
        {
            lock (_lockApi)
            {
                if (_activeAlarms.Contains(alarmId))
                {
                    _activeAlarms.Remove(alarmId);

                    var alarm = _alarms[alarmId];
                    var toNow = DateTime.Now.Subtract(alarm.Time);
                    alarm.SnoozeDeltaSeconds = toNow.Add(snoozeTime).TotalSeconds;

                    SaveAlarms();
                }

                StopSoundIfNoActiveAlarms();
            }
        }

        public void DismissActiveAlarm(int alarmId)
        {
            lock (_lockApi)
            {
                if (_activeAlarms.Contains(alarmId))
                {
                    _activeAlarms.Remove(alarmId);
                }

                // TODO: Maybe clean up OneTime alarms here to avoid unbounded growth of settings.xml

                StopSoundIfNoActiveAlarms();
            }
        }
        
        public int[] EnumerateActiveAlarms()
        {
            lock (_lockApi)
            {
                return _activeAlarms.ToArray();
            }
        }

        public int[] EnumerateAlarms()
        {
            lock (_lockApi)
            {
                return _alarms.Keys.ToArray();
            }
        }

        public AlarmDescription GetAlarmDescription(int id)
        {
            lock (_lockApi)
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
            lock (_lockApi)
            {
                var tickTime = DateTime.Now;
                List<int> alarmsToActivate = new List<int>();

                foreach (var key in _alarms.Keys)
                {
                    var desc = _alarms[key];
                    var alarmTime = desc.Time.Add(TimeSpan.FromSeconds(desc.SnoozeDeltaSeconds));

                    //
                    // Alarm should fire when:
                    //     
                    // ----------|----------|----------|----------> time
                    //           ^          ^          ^
                    //       last-tick    alarm      tick
                    //

                    switch (desc.Type)
                    {
                        case AlarmType.OneTime:
                            if (alarmTime > _lastTickTime &&
                                alarmTime <= tickTime)
                            {
                                alarmsToActivate.Add(key);
                            }
                            break;

                        case AlarmType.RecurringWeekly:
                            const double secondsPerWeek = 7 * 24 * 60 * 60;

                            double alarmSeconds = alarmTime.ToUnixTimeInSeconds() % secondsPerWeek;
                            double tickSeconds = tickTime.ToUnixTimeInSeconds() % secondsPerWeek;
                            double lastTickSeconds = _lastTickTime.ToUnixTimeInSeconds() % secondsPerWeek;

                            if (alarmSeconds > lastTickSeconds &&
                                alarmSeconds <= tickSeconds)
                            {
                                alarmsToActivate.Add(key);
                            }
                            break;
                    }
                }

                _lastTickTime = tickTime;

                foreach (var alarmId in alarmsToActivate)
                {
                    ActivateAlarm(alarmId);
                }
            }
        }
    }
}
