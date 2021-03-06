﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWKC.Alarm.Common;
using System.Windows;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading;

namespace NWKC.Alarm.Client
{
    abstract class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;
        Action _action;

        public Command(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_action != null)
            {
                _action();
            }
        }
    }

    class AlarmModel : ModelBase
    {
        AlarmDescription _description;
        ServiceProxy _proxy;
        int _alarmId;

        public AlarmModel(ServiceProxy proxy, int alarmId, AlarmDescription description)
        {
            _description = description;
            _alarmId = alarmId;
            _proxy = proxy;
        }

        public string Message
        {
            get { return _description.Message; }
        }

        public DateTime Time
        {
            get { return _description.Time; }
        }
        public int Id
        {
            get { return _alarmId; }
        }

        public Command Dismiss
        {
            get
            {
                return new Command(() =>
                {
                    _proxy.DismissActiveAlarm(_alarmId);
                });
            }
        } 

        public Command Snooze
        {
            get
            {
                return new Command(() =>
                {
                    _proxy.SnoozeActiveAlarm(_alarmId, TimeSpan.FromSeconds(5.0)); // Note: Snooze time is hardcoded to 5 minutes (TODO: Move this to configuration file, maybe)
                });
            }
        }
    }

    delegate void ActiveAlarmsChangedDelegate(int[] added, int[] removed, int oldCount);

    class AlarmManagerModel : ModelBase
    {
        ServiceProxy _proxy;
        Dispatcher _dispatcher;
        ObservableCollection<AlarmModel> _activeAlarms;
        bool _connnected;
        volatile bool _connecting;

        public event ActiveAlarmsChangedDelegate ActiveAlarmsChanged;

        public AlarmManagerModel()
        {
            _proxy = new ServiceProxy(ActiveAlarmsChangedCallback, DisconnectedCallback);
            _dispatcher = Dispatcher.CurrentDispatcher;
            _activeAlarms = new ObservableCollection<AlarmModel>();
            _connnected = false;
            _connecting = false;

            Connect();
        }

        public bool Connected
        {
            get { return _connnected; }
            set
            {
                if (_connnected != value)
                {
                    _connnected = value;
                    NotifyPropertyChanged("Connected");
                }
            }
        }

        void Connect()
        {
            if (_connecting)
            {
                return;
            }

            _connecting = true;

            ThreadPool.QueueUserWorkItem((o) =>
            {
                while (!_proxy.Connect())
                {
                    Thread.Sleep(500);
                }

                _dispatcher.BeginInvoke(new Action(() =>
                {
                    _connecting = false;
                    Connected = true;

                    RefreshActiveAlarms();
                }));
            });
        }

        void DisconnectedCallback()
        {
            Connected = false;
            Connect();
        }

        void ActiveAlarmsChangedCallback()
        {
            _dispatcher.BeginInvoke(new Action(() =>
            {
                RefreshActiveAlarms();
            }));
        }

        void RefreshActiveAlarms()
        {
            // Create exisiting (old) set of IDs
            HashSet<int> oldSet = new HashSet<int>();
            foreach (var activeAlarm in _activeAlarms)
            {
                oldSet.Add(activeAlarm.Id);
            }

            // Keep track of the existing (old) count
            int oldCount = _activeAlarms.Count;

            // Create new set of IDs
            var ids = _proxy.EnumerateActiveAlarms();
            HashSet<int> newSet = new HashSet<int>(ids);

            // Figure out which IDs to add and which to remove
            var toAdd = newSet.Except(oldSet).ToArray();
            var toRemove = oldSet.Except(newSet).ToArray();

            // Create ID to model mapping
            Dictionary<int, AlarmModel> _modelById = new Dictionary<int, AlarmModel>();
            foreach (var model in _activeAlarms)
            {
                _modelById.Add(model.Id, model);
            }

            // Remove IDs
            foreach (var id in toRemove)
            {
                var model = _modelById[id];
                _activeAlarms.Remove(model);
            }
        
            // Add IDs
            foreach (var id in toAdd)
            {
                var description = _proxy.GetAlarmDescription(id);
                _activeAlarms.Add(new AlarmModel(_proxy, id, description));
            }

            // Fire event
            if (ActiveAlarmsChanged != null)
            {
                ActiveAlarmsChanged(toAdd, toRemove, oldCount);
            }
        }

        public ObservableCollection<AlarmModel> ActiveAlarms
        {
            get { return _activeAlarms; }
        }
    }
}
