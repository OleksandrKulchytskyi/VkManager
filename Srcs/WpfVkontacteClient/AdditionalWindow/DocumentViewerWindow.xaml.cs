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
using WpfVkontacteClient.Entities;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for DocumentViewerWindow.xaml
	/// </summary>
	public partial class DocumentViewerWindow : Window
	{
		VKontakteApiWrapper m_wrapper = null;

		public DocumentViewerWindow (VKontakteApiWrapper wrapper)
		{
			InitializeComponent();
			if (wrapper == null)
				throw new ArgumentNullException("wrapper");
			m_wrapper = wrapper;
		}

		public DocumentViewerWindow(VKontakteApiWrapper wrapper, List<Friend> friends)
			: this(wrapper)
		{
			if (friends == null)
				throw new ArgumentNullException("friends");
			Friends = friends;
		}

		public List<Friend> Friends
		{
			get { return (List<Friend>)GetValue(FriendsProperty); }
			set { SetValue(FriendsProperty, value); }
		}

		// Define a user friends
		public static readonly DependencyProperty FriendsProperty =
			DependencyProperty.Register("Friends", typeof(List<Friend>), typeof(PhotoViewerWindow), new UIPropertyMetadata(null));


		private void cmbSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsInitialized)
				return;

			if (((cmbSource.SelectedItem as ComboBoxItem).Content as string).Contains("друзей"))
			{
				txtFriends.Visibility = System.Windows.Visibility.Visible;
				cmbFriends.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				txtFriends.Visibility = System.Windows.Visibility.Collapsed;
				cmbFriends.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		private void cmbFriends_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((cmbFriends.SelectedItem as Friend) != null)
			{
				carouselContr.ItemsSource= m_wrapper.DocumentsGet((cmbFriends.SelectedItem as Friend).UserId, null);
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var data=m_wrapper.DocumentsGet(null, null);
			if (data == null)
				data = new List<UserDocument>();
			carouselContr.ItemsSource = data;
		}
	}
}
