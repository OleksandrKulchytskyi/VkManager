using System;
using System.Windows.Data;

namespace WpfVkontacteClient.Converters
{
	public class InstanceToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return System.Windows.Visibility.Collapsed;
			else
				return System.Windows.Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}