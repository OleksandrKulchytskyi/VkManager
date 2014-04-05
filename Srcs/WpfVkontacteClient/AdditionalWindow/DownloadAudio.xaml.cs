using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfVkontacteClient.Entities;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for DownloadAudio.xaml
	/// </summary>
	public partial class DownloadAudio : Window
	{
		private AsyncDownloader loader = null;
		private static readonly char[] invalidPath = System.IO.Path.GetInvalidFileNameChars();

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
			loader.DataToDownload.ForEach(PrepareFilePath);
			loader.DownloadingComplete += new EventHandler(loader_DownloadingComplete);
			loader.ProgressChanged += new EventHandler(loader_ProgressChanged);
			prgOverall.Value = 0;
			loader.Download();
		}

		private void PrepareFilePath(ItemToLoad item)
		{
			bool valid = true;
			string fname = System.IO.Path.GetFileName(item.Url);
			for (int i = 0; i < fname.Length; i++)
			{
				for (int j = 0; j < invalidPath.Length; j++)
				{
					if (fname[i] == invalidPath[j])
					{
						valid = false;
						break;
					}
				}
				if (!valid)
					break;
			}
			if (!valid)
				fname = Guid.NewGuid().ToString("N");
			item.PathToSave = System.IO.Path.Combine((Application.Current as App).AppFolder,
				"Audio", fname);
		}

		private void loader_ProgressChanged(object sender, EventArgs e)
		{
			prgOverall.Value = loader.OverallPercentage;
		}

		private void loader_DownloadingComplete(object sender, EventArgs e)
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