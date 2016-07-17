using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace NWKC.Alarm.Common
{
    public class Constants
    {
        public static string Uri
        {
            get
            {
                return "net.pipe://localhost/NWKC";
            }
        }

        public static string EndpointAddres
        {
            get
            {
                return "AlarmService";
            }
        }
    }

    public class AlarmDescription
    {
        public DayOfWeek DayOfWeek { get; set; }
        public double SecondsSinceMidnight { get; set; }
        public string Message { get; set; }
    }

    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IAlarmCallbacks))]
    public interface IAlarmControls
    {
        void ConnectToAlarmService();
        int CreateAlarm(AlarmDescription alarmDescription);
        void DeleteAlarm(int alarmId);
        int[] EnumerateAlarms();
        int[] EnumerateActiveAlarms();
        AlarmDescription GetAlarmDescriptionById(int id);
        void DismissActiveAlarm(int alarmId);
    }

    public interface IAlarmCallbacks
    {
        void AlarmBecameActive(int alarmId);
    }
}
