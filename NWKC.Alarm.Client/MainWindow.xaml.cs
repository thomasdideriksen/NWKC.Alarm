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

        public MainWindow()
        {
            InitializeComponent();
            
            this.WindowState = WindowState.Minimized;
            this.Hide();

            var assembly = Assembly.GetExecutingAssembly();
            var iconStream = Helpers.GetEmbeddedResource("icon.ico", assembly);

            _icon = new NotifyIcon();
            _icon.Icon = new System.Drawing.Icon(iconStream);
            _icon.Visible = true;
            _icon.Click += _icon_Click;
        }

        private void _icon_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs args = e as System.Windows.Forms.MouseEventArgs;
            if (args != null && _configWindow == null)
            {
                _configWindow = new ConfigurationWindow();
                _configWindow.Show();
                _configWindow.Closing += _configWindow_Closing;
            }
        }

        private void _configWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _configWindow = null;
        }
    }
}
