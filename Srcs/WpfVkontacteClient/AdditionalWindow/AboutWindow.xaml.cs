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

namespace WpfVkontacteClient
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	public partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			InitializeComponent();
			Run obj=null;
			foreach (var item in txtAppDescription.Inlines)
			{
				if ((item as System.Windows.Documents.Run) == null)
					continue;
				else
				{
					if ((item as Run).Text.IndexOf("[v]") != -1)
					{
						obj = (item as Run);
						break;
					}
				}
			}
			if (obj != null)
				obj.Text = obj.Text.Replace("[v]", typeof(AboutWindow).Assembly.GetName().Version.ToString());
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
