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
				var videoList= VKontakteApiWrapper.Instance.VideoGetByIds(Utils.StringUtils.GetUserAndObjectIDFromUrl(strCol));
				this.Close();

				DownloadVideo wind = new DownloadVideo(videoList);
				wind.Show();
			}
		}
	}
}
