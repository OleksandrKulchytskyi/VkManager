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
using System.Net;

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

		protected VKontakteApiWrapper VkWrapper
		{
			get;
			set;
		}
		public PhotoPreviewWindow(Entities.PhotoExteded photo, VKontakteApiWrapper wrapper)
		{
			InitializeComponent();
			
			if (photo != null)
				PhotoObj = photo;

			if (wrapper != null)
				this.VkWrapper = wrapper;

			CurrentFoto = System.IO.Path.Combine((Application.Current as App).AppFolder,
				"Photo", System.IO.Path.GetFileName(photo.SourceBig));

			if (App.Current.ImageCacheInstance.IsCached(photo.SourceBig))
			{
				imgPreview.Source = PhotoObj.SourceBig.GetImage();
			}
			else
			{
				WebClient web = new WebClient();
				web.DownloadDataAsync(new Uri(PhotoObj.SourceBig, UriKind.RelativeOrAbsolute));
				web.DownloadDataCompleted += (s, e) =>
					{
						if (e.Error == null)
						{
							App.Current.ImageCacheInstance.SaveImage(PhotoObj.SourceBig, e.Result);
						}
					};
			}

			if (!System.IO.File.Exists(CurrentFoto))
			{
				using (WebClient client = new WebClient())
				{
					client.DownloadFileCompleted += (send, arg) =>
						{
							imgPreview.Source = new BitmapImage(new Uri(CurrentFoto,
																		UriKind.RelativeOrAbsolute));
						};
					client.DownloadFileAsync(new Uri(PhotoObj.SourceBig, UriKind.RelativeOrAbsolute), CurrentFoto);
				}

			}

			this.Loaded += new RoutedEventHandler(PhotoPreviewWindow_Loaded);
			headerLabel.MouseLeftButtonDown += new MouseButtonEventHandler(headerLabel_MouseLeftButtonDown);
		}

		void headerLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				this.DragMove();
		}

		void PhotoPreviewWindow_Loaded(object sender, RoutedEventArgs e)
		{
			lstComents.ItemsSource = VkWrapper.GetPhotoComments(PhotoObj.PhotoId.ToString(), PhotoObj.OwnerId.ToString(), null, null);
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			headerLabel.MouseLeftButtonDown -= new MouseButtonEventHandler(headerLabel_MouseLeftButtonDown);
			PhotoObj = null;
			VkWrapper = null;
			this.DialogResult = false;
			GC.Collect();
			GC.WaitForPendingFinalizers();
			this.Close();
		}
	}
}
