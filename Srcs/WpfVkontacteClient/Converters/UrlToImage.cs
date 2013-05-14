using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfVkontacteClient.Converters
{
	[ValueConversion(typeof(string), typeof(ImageSource))]
	public sealed class UrlToImageConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}
			string url = (string)value;
			byte[] photo = App.Current.ImageCacheInstance.GetImage(url);

			if (photo != null)
			{
				BitmapImage image = new BitmapImage();
				image.BeginInit();
				image.StreamSource = new MemoryStream(photo);
				image.EndInit();
				return image;
			}
			else
			{
				//ManualResetEventSlim e=new ManualResetEventSlim(false);
				BitmapImage image = null;
				//byte[] array = null;
				//ImageDownloader.DownloadImage(url, new Action<bool, byte[]>(
				//                                    delegate(bool ok, byte[] bytes)
				//                                        {
				//                                            if (ok)
				//                                            {
				//                                                array = bytes;
				//                                                e.Set();
				//                                            }

				//                                        }));
				//if (e.Wait(20000))
				//{
				//    image=new BitmapImage();
				//    image.BeginInit();
				//    image.StreamSource = new MemoryStream(array);
				//    image.EndInit();
				//}
				image = new BitmapImage();
				image.BeginInit();
				image.UriSource = new Uri(url);
				image.EndInit();
				return image;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public static UrlToImageConverter Instance = new UrlToImageConverter();
	}
}