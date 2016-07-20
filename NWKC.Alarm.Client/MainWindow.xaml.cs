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
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace NWKC.Alarm.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon _icon;
        ConfigurationWindow _configWindow;
        AlarmWindow _alarmWindow;

        public MainWindow()
        {
            InitializeComponent();

            //_menu = new System.Windows.Controls.ContextMenu();
            //_menu.StaysOpen = false;
            //_menu.Focusable = false;
            //_menu.Items.Add(new System.Windows.Controls.MenuItem() { Header = "Thomas" });
            //_menu.Items.Add(new System.Windows.Controls.MenuItem() { Header = "Dideriksen" });
            //_menu.Opened += (s, e) =>
            //{
            //    DispatcherTimer 
            //    _menu.CaptureMouse();
            //};

            this.WindowState = WindowState.Minimized;
            this.Hide();

            var assembly = Assembly.GetExecutingAssembly();
            var iconStream = Helpers.GetEmbeddedResource("icon.ico", assembly);

            _icon = new NotifyIcon();
            _icon.Icon = new System.Drawing.Icon(iconStream);
            _icon.Visible = true;
            _icon.Click += _icon_Click;





            _icon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
            {
                new System.Windows.Forms.MenuItem("Configure...", (o, e) => {
                    if (_configWindow == null)
                    {
                        _configWindow = new ConfigurationWindow();
                        _configWindow.Closing += ClosingWindow;
                    }
                    _configWindow.Show();
                }),
                new System.Windows.Forms.MenuItem("Show", (o, e) => {
                    if (_alarmWindow == null)
                    {
                        _alarmWindow = new AlarmWindow();
                    }
                    _alarmWindow.OpenWindow();
                }),
                  new System.Windows.Forms.MenuItem("Hide", (o, e) => {
                    if (_alarmWindow != null)
                    {
                        _alarmWindow.CloseWindow();
                    }
                }),
            });
        }
    
        private void _icon_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs args = e as System.Windows.Forms.MouseEventArgs;
            if (args != null)
            {
                // _menu.IsOpen = true;
                //bool capture = _menu.CaptureMouse();
                
                
                /*
                if (_configWindow == null)
                {
                    _configWindow = new ConfigurationWindow();
                    _configWindow.Closing += _configWindow_Closing;
                }
                _configWindow.Show();
                */
            }
        }

        private void ClosingWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Window window = sender as Window;
            window.Hide();
            e.Cancel = true;
        }
    }
}
