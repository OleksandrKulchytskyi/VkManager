using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Utils.Helpers;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for PhotoPreviewWindow.xaml
	/// </summary>
	public partial class PhotoPreviewWindow : Window
	{
		public Entities.PhotoExteded PhotoObj
		{
			get;
			private set;
		}

		protected string CurrentFoto
		{
			get;
			set;
		}

		private GenericWeakReference<VKontakteApiWrapper> _vkWrapperWeak;

		public PhotoPreviewWindow(Entities.PhotoExteded photo, VKontakteApiWrapper wrapper)
		{
			InitializeComponent();

			if (photo != null) PhotoObj = photo;

			if (wrapper != null)
				this._vkWrapperWeak = new GenericWeakReference<VKontakteApiWrapper>(wrapper);

			CurrentFoto = Path.Combine((Application.Current as App).AppFolder, "Photo", Path.GetFileName(photo.SourceBig));

			if (App.Current.ImageCacheInstance.IsCached(photo.SourceBig))
				imgPreview.Source = PhotoObj.SourceBig.GetImage();
			else
			{
				WebClient web = new WebClient();
				web.DownloadDataAsync(new Uri(PhotoObj.SourceBig, UriKind.RelativeOrAbsolute));
				web.DownloadDataCompleted += web_DownloadDataCompleted;
			}

			if (!System.IO.File.Exists(CurrentFoto))
			{
				using (WebClient client = new WebClient())
				{
					client.DownloadFileCompleted += client_DownloadFileCompleted;
					client.DownloadFileAsync(new Uri(PhotoObj.SourceBig, UriKind.RelativeOrAbsolute), CurrentFoto);
				}
			}

			this.Loaded += new RoutedEventHandler(PhotoPreviewWindow_Loaded);
			headerLabel.MouseLeftButtonDown += new MouseButtonEventHandler(headerLabel_MouseLeftButtonDown);
		}

		private void web_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
		{
			(sender as WebClient).DownloadDataCompleted -= web_DownloadDataCompleted;
			if (e.Error == null)
				App.Current.ImageCacheInstance.SaveImage(PhotoObj.SourceBig, e.Result);
		}

		private void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			(sender as WebClient).DownloadFileCompleted -= client_DownloadFileCompleted;
			imgPreview.Source = new BitmapImage(new Uri(CurrentFoto, UriKind.RelativeOrAbsolute));
		}

		private void headerLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
		}

		private void PhotoPreviewWindow_Loaded(object sender, RoutedEventArgs e)
		{
			lstComents.ItemsSource = _vkWrapperWeak.Target.GetPhotoComments(PhotoObj.PhotoId.ToString(), PhotoObj.OwnerId.ToString(), null, null);
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			headerLabel.MouseLeftButtonDown -= new MouseButtonEventHandler(headerLabel_MouseLeftButtonDown);
			PhotoObj = null;
			_vkWrapperWeak = null;
			this.DialogResult = false;

			this.Close();
		}
	}
}