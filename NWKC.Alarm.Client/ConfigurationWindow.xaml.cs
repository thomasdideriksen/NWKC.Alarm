using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NWKC.Alarm.Common;
using System.Windows.Threading;

namespace NWKC.Alarm.Client
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        ServiceProxy _proxy;

        public ConfigurationWindow()
        {
            InitializeComponent();
            _proxy = new ServiceProxy(AlarmCallback);
        }

        void AlarmCallback(int alarmId)
        {
            Console.Write("--> {0}", alarmId);
            var desc = _proxy.GetAlarmDescriptionById(alarmId);
            Console.WriteLine(desc.Message);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            AlarmDescription desc = new AlarmDescription();
            desc.DayOfWeek = DateTime.Now.DayOfWeek;
            desc.SecondsSinceMidnight = Helpers.SecondsSinceMidnight(DateTime.Now) + 4.0;
            desc.Message = "This is a test!";

            int id = _proxy.CreateAlarm(desc);
            Console.WriteLine("Created alarm: {0}", id);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var alarms = _proxy.EnumerateActiveAlarms();
            foreach (var alarm in alarms)
            {
                _proxy.DismissActiveAlarm(alarm);
            }
        }
    }
}
