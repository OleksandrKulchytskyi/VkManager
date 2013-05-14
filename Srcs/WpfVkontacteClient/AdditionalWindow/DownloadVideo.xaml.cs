using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfVkontacteClient.Entities;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for DownloadVideo.xaml
	/// </summary>
	public partial class DownloadVideo : Window
	{
		private AsyncDownloader loader = null;

		public DownloadVideo(List<UserVideo> videos)
		{
			InitializeComponent();

			if (videos != null && videos.Count > 0)
				this.VideoList = videos;

			this.lstVideo.ItemsSource = this.VideoList;

			this.Closing += (s, e) =>
				{
					if (this.VideoList != null && VideoList.Count > 0)
					{
						VideoList.Clear();
						VideoList = null;
					}

					if (loader != null)
					{
						loader.DownloadingComplete -= new EventHandler(loader_DownloadingComplete);
						loader.ProgressChanged -= new EventHandler(loader_ProgressChanged);
						loader.Dispose();
					}

					GC.Collect(0);
				};
		}

		public List<UserVideo> VideoList
		{
			get;
			private set;
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			string file = string.Empty;
			foreach (UserVideo video in VideoList)
			{
				string containerPath = string.Empty;

				if (!string.IsNullOrEmpty(video.PlayerUrl))
				{
					containerPath = video.PlayerUrl;
					file = System.IO.Path.Combine((Application.Current as App).AppFolder, "Video",
													   System.IO.Path.GetFileName(video.Title));
				}
				else
				{
					containerPath = video.Image;
					file = System.IO.Path.Combine((Application.Current as App).AppFolder, "Video",
													System.IO.Path.GetFileName(containerPath));
				}

				if (System.IO.File.Exists(file))
					System.IO.File.Delete(file);

				string url = Utils.VideoDownloadHelper.FindFirst(containerPath, "src=\"", "\"");
				if (string.IsNullOrEmpty(url))
					url = video.PlayerUrl;
				string videoPath = Utils.VideoDownloadHelper.GetVideo(url);

				//Check if this video hosted on YouTube
				if (videoPath.ToLower().Contains("assets/videos/.vk"))
				{
					video.State = LoadState.Fail;
					continue;
				}

				if (!string.IsNullOrEmpty(videoPath))
					file = string.Format("{0}{1}", file, System.IO.Path.GetExtension(videoPath));

				video.Url = videoPath;
				video.PathToSave = file;
			}

			loader = new AsyncDownloader();
			loader.DownloadingComplete += new EventHandler(loader_DownloadingComplete);
			loader.ProgressChanged += new EventHandler(loader_ProgressChanged);

			loader.DataToDownload = VideoList.Cast<ItemToLoad>().Where((load) => load.State == LoadState.None).ToList();
			loader.Download();
		}

		private void loader_ProgressChanged(object sender, EventArgs e)
		{
		}

		private void loader_DownloadingComplete(object sender, EventArgs e)
		{
			MessageBox.Show("Loading complete");
		}

		private void headerLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
				this.DragMove();
		}
	}
}