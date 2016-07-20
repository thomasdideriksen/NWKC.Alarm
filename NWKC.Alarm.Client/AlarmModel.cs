using System;
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

namespace NWKC.Alarm.Client
{
    abstract class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string propertyName)
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
                    _proxy.SnoozeActiveAlarm(_alarmId, TimeSpan.FromMinutes(5.0)); // Note: Snooze time is hardcoded to 5 minutes (TODO: Move this to configuration file, maybe)
                });
            }
        }
    }

    class AlarmManagerModel : ModelBase
    {
        ServiceProxy _proxy;
        Dispatcher _dispatcher;
        ObservableCollection<AlarmModel> _activeAlarms;

        public AlarmManagerModel()
        {
            _proxy = new ServiceProxy(ActiveAlarmsChangedCallback);
            _dispatcher = Dispatcher.CurrentDispatcher;
            _activeAlarms = new ObservableCollection<AlarmModel>();

            RefreshActiveAlarms();
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
            var ids = _proxy.EnumerateActiveAlarms();

            _activeAlarms.Clear();
            foreach (var id in ids)
            {
                var description = _proxy.GetAlarmDescription(id);
                _activeAlarms.Add(new AlarmModel(_proxy, id, description));
            }
        }

        public ObservableCollection<AlarmModel> ActiveAlarms
        {
            get { return _activeAlarms; }
        }
    }
}
