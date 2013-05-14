using System.Windows;

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