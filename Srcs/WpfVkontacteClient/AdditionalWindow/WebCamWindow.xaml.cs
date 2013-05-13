using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfVkontacteClient.AdditionalWindow
{
    /// <summary>
    /// Interaction logic for WebCamWindow.xaml
    /// </summary>
    public partial class WebCamWindow : Window
    {
        public WebCamWindow()
        {
            InitializeComponent();
        }
        
        public string DeviceName
        {
            get { return (string)GetValue(DeviceNameProperty); }
            set { SetValue(DeviceNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DeviceName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeviceNameProperty =
            DependencyProperty.Register("DeviceName", typeof(string), typeof(WebCamWindow), new UIPropertyMetadata(string.Empty));

        
    }
}
