using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NWKC.Alarm.Common;
using System.ServiceModel;
using System.Threading;
using System.Timers;

namespace NWKC.Alarm.Service
{
    public partial class Service : ServiceBase
    {
        AutoResetEvent _event;
        AlarmSchedule _alarmSchedule;
        System.Timers.Timer _timer;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //Thread.Sleep(15000);

            _event = new AutoResetEvent(false);
            _alarmSchedule = new AlarmSchedule();

            _timer = new System.Timers.Timer();
            _timer.AutoReset = true;
            _timer.Elapsed += TimerElapsed;
            _timer.Interval = 1000.0;
            _timer.Enabled = true;
            _timer.Start();

            ThreadPool.QueueUserWorkItem((o) =>
            {
                // Create host
                using (ServiceHost host = new ServiceHost(_alarmSchedule, new Uri(Constants.Uri)))
                {
                    // Add service endpoint
                    host.AddServiceEndpoint(
                        typeof(IAlarmControls),
                        new NetNamedPipeBinding(NetNamedPipeSecurityMode.None),
                        Constants.EndpointAddres);

                    // Open host - this will make it available to other processes
                    host.Open();

                    // Wait for service termination
                    _event.WaitOne();

                    // Cleanup
                    host.Close();
                }
            });
        }
        
        protected override void OnStop()
        {
            _timer.Stop();
            _timer.Enabled = false;
            _timer.Dispose();
            _timer = null;

            _alarmSchedule = null;

            _event.Set();
            _event = null;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_alarmSchedule != null)
            {
                _alarmSchedule.Tick();
            }
        }
    }
}
