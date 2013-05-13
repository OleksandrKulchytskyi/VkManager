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

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for FindWindow.xaml
	/// </summary>
	public partial class FindWindow : Window
	{
		public string SearchText
		{
			get;
			private set;
		}

		public FindWindow()
		{
			InitializeComponent();
			SearchText = string.Empty;
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			this.SearchText = txtSearch.Text;
			this.DialogResult = true;
		}
	}
}
