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
using WpfVkontacteClient.Entities;
using System.Net;
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for DownloadAudio.xaml
	/// </summary>
	public partial class DownloadAudio : Window
	{
		private AsyncDownloader loader = null;

		public DownloadAudio(List<UserAudio> audioList)
		{
			InitializeComponent();
			if (audioList != null && audioList.Count > 0)
				AudioList = audioList;
			lstAudio.ItemsSource = AudioList;

			this.Closing += (s, e) =>
				{
					if (AudioList != null && AudioList.Count > 0)
						AudioList.Clear();
					AudioList = null;

					loader.DownloadingComplete -= new EventHandler(loader_DownloadingComplete);
					loader.ProgressChanged -= new EventHandler(loader_ProgressChanged);
					loader.Dispose();

					GC.Collect(0);
					GC.WaitForPendingFinalizers();
				};
		}

		public List<UserAudio> AudioList
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
			loader = new AsyncDownloader();
			loader.DataToDownload = AudioList.Cast<ItemToLoad>().ToList();
			loader.DataToDownload.ForEach(
				new Action<ItemToLoad>((item) => item.PathToSave =
												System.IO.Path.Combine((Application.Current as App).AppFolder, "Audio", System.IO.Path.GetFileName(item.Url))
												));
			loader.DownloadingComplete += new EventHandler(loader_DownloadingComplete);
			loader.ProgressChanged += new EventHandler(loader_ProgressChanged);
			prgOverall.Value = 0;
			loader.Download();
		}

		void loader_ProgressChanged(object sender, EventArgs e)
		{
			prgOverall.Value = loader.OverallPercentage;
		}

		void loader_DownloadingComplete(object sender, EventArgs e)
		{
			MessageBox.Show("Downloads complete", "", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void headerLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
				this.DragMove();
		}
	}
}
