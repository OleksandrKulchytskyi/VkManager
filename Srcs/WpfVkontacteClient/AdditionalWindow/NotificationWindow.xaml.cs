using System;
using System.Windows;
using System.Windows.Threading;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for NotificationWindow.xaml
	/// </summary>
	public partial class NotificationWindow : Window
	{
		public NotificationWindow()
		{
			InitializeComponent();

			storyNotify.Completed += new EventHandler(storyNotify_Completed);

			Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
			{
				var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
				var corner = transform.Transform(new Point(System.Windows.SystemParameters.PrimaryScreenWidth, System.Windows.SystemParameters.PrimaryScreenHeight));
				this.Left = corner.X - this.ActualWidth;
				this.Top = corner.Y - this.ActualHeight - 100;
			}));
		}

		private void storyNotify_Completed(object sender, EventArgs e)
		{
			storyNotify.Completed -= new EventHandler(storyNotify_Completed);
			this.Close();
		}
	}
}