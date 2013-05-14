using System.Windows;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for VideoLinksWindow.xaml
	/// </summary>
	public partial class VideoLinksWindow : Window
	{
		public VideoLinksWindow()
		{
			InitializeComponent();
		}

		private void btnDownload_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(txtLinks.Text))
			{
				var strCol = new System.Collections.Specialized.StringCollection();
				strCol.AddRange(txtLinks.Text.Split('\r'));
				var videoList = VKontakteApiWrapper.Instance.VideoGetByIds(Utils.StringUtils.GetUserAndObjectIDFromUrl(strCol));
				this.Close();

				DownloadVideo wind = new DownloadVideo(videoList);
				wind.Owner = App.Current.MainWindow;
				wind.Show();
			}
		}
	}
}