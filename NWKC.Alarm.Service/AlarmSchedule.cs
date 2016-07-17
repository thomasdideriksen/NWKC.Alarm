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

namespace NWKC.Alarm.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    class AlarmSchedule : IAlarmControls
    {
        Dictionary<int, AlarmDescription> _alarms;
        HashSet<int> _activeAlarms;
        double _lastTickTime;
        SoundPlayer _soundPlayer;
        List<IAlarmCallbacks> _clients;
        bool _soundIsPlaying;
        static int _nextId = 1;

        public AlarmSchedule()
        {
            _alarms = new Dictionary<int, AlarmDescription>();
            _activeAlarms = new HashSet<int>();
            _lastTickTime = Helpers.SecondsSinceMidnight(DateTime.Now);
            _clients = new List<IAlarmCallbacks>();

            Stream alarmAudioStream = GetEmbeddedResource("alarm.wav");
            _soundPlayer = new SoundPlayer(alarmAudioStream);
            _soundIsPlaying = false;
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

        Stream GetEmbeddedResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            foreach (var candidate in names)
            {
                if (candidate.ToLower().EndsWith(name.ToLower()))
                {
                    return assembly.GetManifestResourceStream(candidate);
                }
            }
            return null;
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
