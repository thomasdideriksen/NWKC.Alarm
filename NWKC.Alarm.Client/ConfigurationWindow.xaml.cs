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
            _proxy = new ServiceProxy(AlarmCallback, DisconnectCallback);
            _proxy.Connect();
            this.Loaded += ConfigurationWindow_Loaded;
        }
        
        private void ConfigurationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var display = System.Windows.SystemParameters.WorkArea;
            this.Left = display.Right - this.Width;
            this.Top = display.Bottom - this.Height;
            this.Topmost = true;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            Close();
        }

        void DisconnectCallback()
        {

        }

        void AlarmCallback()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Console.WriteLine("Active alarms changed");
            }));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            DateTime time = DateTime.Now;
            time = time.Add(TimeSpan.FromSeconds(4));

            AlarmDescription desc = new AlarmDescription();
            desc.Type = AlarmType.RecurringWeekly;
            desc.Time = time;
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

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            var alarms = _proxy.EnumerateActiveAlarms();
            foreach (var alarm in alarms)
            {
                _proxy.SnoozeActiveAlarm(alarm, TimeSpan.FromSeconds(5.0));
            }
        }
    }
}
