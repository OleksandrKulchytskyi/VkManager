using System;
using System.Net;
using System.Windows;
using System.Windows.Media;
using WpfVkontacteClient.Entities;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for WindowAudio.xaml
	/// </summary>
	public partial class WindowAudio : Window
	{
		private WebClient client = null;
		protected bool IsClosingWind = false;
		protected UserAudio userAudio;
		MediaPlayer player = new MediaPlayer();

		public string CurrentAudio
		{
			get;
			private set;
		}

		public WindowAudio(UserAudio audio)
		{
			InitializeComponent();
			this.Closing += (s, e) =>
				{
					ClearObjects();
				};
			client = new WebClient();

			if (audio != null)
				userAudio = audio;
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			client.DownloadProgressChanged += (send, arg) =>
				{
					prgAudio.Value = arg.ProgressPercentage;
					if (this.IsClosingWind)
						(send as WebClient).CancelAsync();
				};

			client.DownloadDataCompleted += (send, arg) =>
				{
					if (arg.Error == null)
					{
						App.Current.DataCache.SaveData(userAudio.Url, arg.Result);
						if (!string.IsNullOrEmpty(arg.UserState.ToString()))
						{
							System.IO.FileInfo fi = new System.IO.FileInfo(arg.UserState.ToString());
							if (fi.Exists)
							{
								fi.Attributes = System.IO.FileAttributes.Normal;
								fi.Delete();
							}
							else
							{
								System.IO.FileStream fs =fi.Create();
								fs.Write(arg.Result, 0, (int)arg.Result.LongLength);
								fs.Flush();
								if (fs != null)
								{
									fs.Close();
									fs.Dispose();
								}
							}
						}
					}
				};

			CurrentAudio = System.IO.Path.Combine((Application.Current as App).AppFolder, "Audio",
													System.IO.Path.GetFileName(userAudio.Url));

			if (!App.Current.DataCache.IsCached(userAudio.Url))
			{
				client.DownloadDataAsync(new Uri(userAudio.Url, UriKind.RelativeOrAbsolute), CurrentAudio);
			}
			else
			{
				System.IO.FileStream fs = new System.IO.FileStream(CurrentAudio, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				byte[] data = App.Current.DataCache.GetData(userAudio.Url);
				if (data != null)
				{
					fs.Write(data, 0, (int)data.LongLength);
					fs.Flush();
				}
				if (fs != null)
				{
					fs.Close();
					fs.Dispose();
					data = null;
				}
				prgAudio.Value = 100;
				player.Open(new Uri(CurrentAudio, UriKind.RelativeOrAbsolute));
			}

		}

		private void btnPlay_Click(object sender, RoutedEventArgs e)
		{
			if (!player.IsNull())
				player.Play();
		}


		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			if (!player.IsNull())
				player.Stop();
		}

		private void btnPause_Click(object sender, RoutedEventArgs e)
		{
			if (player.CanPause)
				player.Pause();
		}

		private void slderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (player != null)
				player.Volume = slderVolume.Value;
		}

		private void ClearObjects()
		{
			IsClosingWind = true;
			if (!client.IsNull())
			{
				if (client.IsBusy)
					client.CancelAsync();
				client = null;
			}

			userAudio = null;
			if (player != null)
			{
				player.Close();
				player = null;
			}

			if (!string.IsNullOrEmpty(CurrentAudio))
			{
				System.IO.File.Delete(CurrentAudio);
			}

			GC.Collect();
			GC.WaitForPendingFinalizers();
			DialogResult = false;
		}
	}
}
