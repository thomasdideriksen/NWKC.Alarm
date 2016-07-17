using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWKC.Alarm.Common;

namespace NWKC.Alarm.Service
{
    class AlarmSchedule : IAlarmControls
    {
        IAlarmCallbacks _callbacks;

        // Note: _callbacks = OperationContext.Current.GetCallbackChannel<IAlarmCallbacks>();


        public void ConnectToAlarmService()
        {
            throw new NotImplementedException();
        }

        public int CreateAlarm(AlarmDescription alarmDescription)
        {
            throw new NotImplementedException();
        }

        public void DeleteAlarm(int alarmId)
        {
            throw new NotImplementedException();
        }

        public void DismissActiveAlarm(int alarmId)
        {
            throw new NotImplementedException();
        }

        public int[] EnumerateActiveAlarms()
        {
            throw new NotImplementedException();
        }

        public int[] EnumerateAlarms()
        {
            throw new NotImplementedException();
        }

        public AlarmDescription GetAlarmDescriptionById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
