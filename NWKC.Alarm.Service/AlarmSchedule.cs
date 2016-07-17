using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWKC.Alarm.Common;
using System.ServiceModel;

namespace NWKC.Alarm.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    class AlarmSchedule : IAlarmControls
    {
        IAlarmCallbacks _callbacks;

        public void ConnectToAlarmService()
        {
            _callbacks = OperationContext.Current.GetCallbackChannel<IAlarmCallbacks>();
        }

        public int CreateAlarm(AlarmDescription alarmDescription)
        {
            return 0;
        }

        public void DeleteAlarm(int alarmId)
        {
            
        }

        public void DismissActiveAlarm(int alarmId)
        {
            
        }

        public int[] EnumerateActiveAlarms()
        {
            return new int[] { 1, 2, 3 };
        }

        public int[] EnumerateAlarms()
        {
            return new int[] { 1, 2, 3, 4 };
        }

        public AlarmDescription GetAlarmDescriptionById(int id)
        {
            return null;
        }

        public void Tick()
        {
            // TODO
        }
    }
}
