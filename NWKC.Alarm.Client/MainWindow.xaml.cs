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

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var alarms = _proxy.EnumerateAlarms();

            int thomas = 0;
        }
    }
}
