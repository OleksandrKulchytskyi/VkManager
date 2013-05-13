using System;
using System.Windows;

namespace WpfVkontacteClient
{
	/// <summary>
	/// Interaction logic for LoginWindow.xaml
	/// </summary>
	public partial class LoginWindow : Window
	{
		/// <summary>
		/// Данные сессии
		/// </summary>
		private string _sessionData;
		public string SessionData
		{
			get { return _sessionData; }
		}

		public LoginWindow(long appId, int settings)
		{
			InitializeComponent();
			string appScope = VKontakteApiWrapper.GenerateAppRightsScope;
			var uriNew = new Uri(string.Concat("http://api.vkontakte.ru/oauth/authorize?client_id=", appId.ToString(), "&scope=", appScope), UriKind.Absolute);
			//"http://api.vkontakte.ru/oauth/authorize?client_id={0}&scope=offline,wall";

			//Uri uri = new Uri(String.Format("http://vkontakte.ru/login.php?app={0}&layout=popup&type=browser&settings={1}", appId, settings), UriKind.Absolute);
			webBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(webBrowser_Navigated);
			webBrowser.ScriptErrorsSuppressed = false;
			webBrowser.Navigate(uriNew);
		}

		void webBrowser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
		{
			bool error = false;
			Uri new_page = e.Url;
			if (new_page.AbsolutePath.IndexOf("blank.html", StringComparison.OrdinalIgnoreCase) != -1)
			{
				try
				{
					_sessionData = Uri.UnescapeDataString(new_page.Fragment.Split("=".ToCharArray())[1]);
				}
				catch (IndexOutOfRangeException)
				{
					error = true;
					MessageBox.Show("Login error", "Error occured in login", MessageBoxButton.OK, MessageBoxImage.Error);
				}

				this.DialogResult = error == true ? false : true;
				this.Close();
			}
			else if (new_page.AbsolutePath.IndexOf("blank.html") != -1 &&
				(new_page.Fragment.IndexOf("error") != -1 || new_page.Fragment.IndexOf("error_description") != -1))
			{
				this.DialogResult = false;
				this.Close();
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!webBrowser.Disposing)
				webBrowser.Dispose();

			GC.Collect(1, GCCollectionMode.Forced);
			GC.WaitForPendingFinalizers();
		}
	}
}
