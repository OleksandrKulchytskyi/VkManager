using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;
using System.Globalization;
using System.Windows.Media;

namespace WpfVkontacteClient.Converters
{
	[ValueConversion(typeof(byte[]), typeof(ImageSource))]
	public sealed class BytesToImageConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}
			byte[] array = (byte[])value;
			BitmapImage image = new BitmapImage();
			image.BeginInit();
			image.StreamSource = new MemoryStream(array);
			image.EndInit();
			return (ImageSource)image;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}
			BitmapImage source = (BitmapImage)value;
			byte[] ret = new byte[source.StreamSource.Length];
			source.StreamSource.Read(ret, 0, ret.Length);
			return ret;
		}


		public static BytesToImageConverter Instance = new BytesToImageConverter();
	}
}
