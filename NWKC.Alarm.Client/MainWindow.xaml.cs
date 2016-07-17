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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NWKC.Alarm.Common;

namespace NWKC.Alarm.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServiceProxy _proxy;

        public MainWindow()
        {
            InitializeComponent();
            _proxy = new ServiceProxy(AlarmCallback);
        }

        void AlarmCallback(int alarmId)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var desc = _proxy.GetAlarmDescriptionById(alarmId);
                Console.WriteLine("*** ALARM: {0}: {1}", alarmId, desc.Message);
            }), null);
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
