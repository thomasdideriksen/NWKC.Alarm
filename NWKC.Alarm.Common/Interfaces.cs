using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace NWKC.Alarm.Common
{
    public class AlarmDescription
    {
        public DayOfWeek DayOfWeek { get; set; }
        public double SecondsSinceMidnight { get; set; }
        public string Message { get; set; }
    }

    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IAlarmCallbacks))]
    public interface IAlarmControls
    {
        [OperationContract]
        void ConnectToAlarmService();

        [OperationContract]
        int CreateAlarm(AlarmDescription alarmDescription);

        [OperationContract]
        void DeleteAlarm(int alarmId);

        [OperationContract]
        int[] EnumerateAlarms();

        [OperationContract]
        int[] EnumerateActiveAlarms();

        [OperationContract]
        AlarmDescription GetAlarmDescriptionById(int id);

        [OperationContract]
        void DismissActiveAlarm(int alarmId);
    }

    public interface IAlarmCallbacks
    {
        [OperationContract(IsOneWay = true)]
        void AlarmBecameActive(int alarmId);
    }

    public class Constants
    {
        public static string Uri
        {
            get { return "net.pipe://localhost/NWKC"; }
        }

        public static string EndpointAddres
        {
            get { return "AlarmService"; }
        }
    }
}
