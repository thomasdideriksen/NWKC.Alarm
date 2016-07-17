using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using NWKC.Alarm.Common;

namespace NWKC.Alarm.Client
{
    public delegate void AlarmCalback(int alarmId);

    class ServiceProxy : IAlarmCallbacks
    {
        IAlarmControls _channel;
        AlarmCalback _callback;

        public ServiceProxy(AlarmCalback alarmCallback)
        {
            _callback = alarmCallback;
            string address = string.Format("{0}/{1}", Constants.Uri, Constants.EndpointAddres);

            DuplexChannelFactory<IAlarmControls> factory = new DuplexChannelFactory<IAlarmControls>(
                new InstanceContext(this),
                new NetNamedPipeBinding(NetNamedPipeSecurityMode.None),
                new EndpointAddress(address));

            _channel = factory.CreateChannel();
            _channel.ConnectToAlarmService();
        }

        public void AlarmBecameActive(int alarmId)
        {
            if (_callback != null)
            {
                _callback(alarmId);
            }
        }

        void SafeCall(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {

            }
        }
        
        public int CreateAlarm(AlarmDescription alarmDescription)
        {
            int ret = -1;
            SafeCall(() => { ret = _channel.CreateAlarm(alarmDescription); });
            return ret;
        }

        public void DeleteAlarm(int alarmId)
        {
            SafeCall(() => { _channel.DeleteAlarm(alarmId); });
        }

        public int[] EnumerateAlarms()
        {
            int[] ret = new int[0];
            SafeCall(() => { ret = _channel.EnumerateAlarms(); });
            return ret;
        }

        public int[] EnumerateActiveAlarms()
        {
            int[] ret = new int[0];
            SafeCall(() => { ret = _channel.EnumerateActiveAlarms(); });
            return ret;
        }

        public AlarmDescription GetAlarmDescriptionById(int alarmId)
        {
            AlarmDescription ret = null;
            SafeCall(() => { ret = _channel.GetAlarmDescriptionById(alarmId); });
            return ret;
        }

        public void DismissActiveAlarm(int alarmId)
        {
            SafeCall(() => { _channel.DismissActiveAlarm(alarmId); });
        }
    }
}
