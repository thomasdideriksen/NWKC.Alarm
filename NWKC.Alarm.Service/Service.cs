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

namespace NWKC.Alarm.Service
{
    public partial class Service : ServiceBase
    {
        AutoResetEvent _event;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _event = new AutoResetEvent(false);

            IAlarmControls alarmSchedule = new AlarmSchedule();

            // Create host
            using (ServiceHost host = new ServiceHost(alarmSchedule, new Uri(Constants.Uri)))
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
                host.Abort();
            }
        }

        protected override void OnStop()
        {
            _event.Set();
        }
    }
}
