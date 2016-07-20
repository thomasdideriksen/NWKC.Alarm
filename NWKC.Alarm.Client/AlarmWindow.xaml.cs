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
using System.Windows.Threading;

namespace NWKC.Alarm.Client
{
    /// <summary>
    /// Interaction logic for AlarmWindow.xaml
    /// </summary>
    public partial class AlarmWindow : Window
    {
        public static readonly RoutedEvent WindowOpenedEvent = EventManager.RegisterRoutedEvent("WindowOpened", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(AlarmWindow));
        public static readonly RoutedEvent WindowClosedEvent = EventManager.RegisterRoutedEvent("WindowClosed", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(AlarmWindow));

        public event RoutedEventHandler WindowOpened
        {
            add { AddHandler(WindowOpenedEvent, value); }
            remove { RemoveHandler(WindowOpenedEvent, value); }
        }

        public event RoutedEventHandler WindowClosed
        {
            add { AddHandler(WindowClosedEvent, value); }
            remove { RemoveHandler(WindowClosedEvent, value); }
        }
        
        public void OpenWindow()
        {
            _border.RaiseEvent(new RoutedEventArgs(AlarmWindow.WindowOpenedEvent));
            _window.Show();
        }

        public void CloseWindow()
        {
            _border.RaiseEvent(new RoutedEventArgs(AlarmWindow.WindowClosedEvent));
        }

        private void _storyboardClose_Completed(object sender, EventArgs e)
        {
            this.Hide();
        }

        public AlarmWindow()
        {
            InitializeComponent();
            ConfigureWindow();

            _storyboardClose.Completed += _storyboardClose_Completed;
        }

        void ConfigureWindow()
        {
            var display = System.Windows.SystemParameters.WorkArea;
            this.Width = display.Width;
            this.Left = 0;
            this.Top = (display.Height - this.Height) * 0.5;
            this.Topmost = true;
        }
    }
}
