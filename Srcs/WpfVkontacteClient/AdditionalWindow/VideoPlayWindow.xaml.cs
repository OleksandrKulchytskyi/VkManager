using System;
using System.Windows;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for VideoPlayWindow.xaml
	/// </summary>
	public partial class VideoPlayWindow : Window
	{
		public Uri UriPath
		{
			get;
			private set;
		}

		protected string playerPath = string.Empty;

		public VideoPlayWindow(string path)
		{
			InitializeComponent();

			this.Closing += (s, e) =>
			{
				if (media1.IsPlaying)
					media1.Stop();

				media1.MediaOpened -= new RoutedEventHandler(media1_MediaOpened);
				media1.MediaEnded -= new RoutedEventHandler(media1_MediaEnded);
			};

			if (!string.IsNullOrEmpty(path))
			{
				playerPath = path;
				UriPath = new Uri(path, UriKind.RelativeOrAbsolute);
			}

			media1.MediaOpened += new RoutedEventHandler(media1_MediaOpened);
			media1.MediaEnded += new RoutedEventHandler(media1_MediaEnded);
		}

		private void media1_MediaEnded(object sender, RoutedEventArgs e)
		{
			prgVideo.Value = media1.MediaDuration;
		}

		private void media1_MediaOpened(object sender, RoutedEventArgs e)
		{
			prgVideo.Value = 0;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			string url = Utils.VideoDownloadHelper.FindFirst(playerPath, "src=\"", "\"");
			if (string.IsNullOrEmpty(url)) url = playerPath;
			string videoPath = Utils.VideoDownloadHelper.GetVideo(url);

			if (videoPath.ToLower().Contains("assets/videos/.vk"))
			{
				this.LayoutRoot.Children.Clear();
				this.Width = 700;
				this.Height = 600;
				WpfVkontacteClient.Controls.YouTubeViewer viewer = new Controls.YouTubeViewer();
				viewer.Width = 680; viewer.Height = 570;
				viewer.UpdateLayout();

				viewer.VideoUrl = playerPath;
				this.LayoutRoot.Children.Add(viewer);
				return;
			}

			media1.Source = new Uri(videoPath);
			if (!media1.IsPlaying) media1.Play();
		}

		private void sldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (media1 != null && media1.IsPlaying)
				media1.Volume = sldVolume.Value;
		}

		private void btnPlay_Click(object sender, RoutedEventArgs e)
		{
			if (!media1.IsPlaying) media1.Play();
		}

		private void btnPause_Click(object sender, RoutedEventArgs e)
		{
			if (media1.IsPlaying) media1.Stop();
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			if (media1.IsPlaying) media1.Pause();
		}
	}
}