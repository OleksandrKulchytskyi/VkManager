using System.Windows;

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