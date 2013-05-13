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

namespace WpfVkontacteClient
{
	/// <summary>
	/// Interaction logic for PhotoViewerWindow.xaml
	/// </summary>
	public partial class PhotoViewerWindow : Window
	{
		VKontakteApiWrapper m_wrapper = null;

		public PhotoViewerWindow(VKontakteApiWrapper wrapper)
		{
			InitializeComponent();
			this.Closing += new System.ComponentModel.CancelEventHandler(PhotoViewerWindow_Closing);

			if (wrapper == null)
				throw new ArgumentNullException("wrapper");
			m_wrapper = wrapper;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			cmbAlbums.ItemsSource = m_wrapper.GetUserAlbum(null, null);
		}

		void PhotoViewerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public PhotoViewerWindow(VKontakteApiWrapper wrapper, List<Friend> friends)
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
				cmbAlbums.ItemsSource = m_wrapper.GetUserAlbum(null, null);
			}
		}

		private void cmbFriends_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((cmbFriends.SelectedItem as Friend) != null)
			{
				cmbAlbums.ItemsSource = m_wrapper.GetUserAlbum((cmbFriends.SelectedItem as Friend).UserId.ToString(), null);
			}
		}

		private void cmbAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((cmbAlbums.SelectedItem as UserAlbum) != null)
			{
				if ((cmbFriends.SelectedItem as Friend) != null)
					carouselContr.ItemsSource = m_wrapper.GetPhotosFromAlbum((cmbFriends.SelectedItem as Friend).UserId.ToString(),
												(cmbAlbums.SelectedItem as UserAlbum).AlbumId.ToString(), null);
				else
					carouselContr.ItemsSource = m_wrapper.GetPhotosFromAlbum(m_wrapper.UserId.ToString(),
													(cmbAlbums.SelectedItem as UserAlbum).AlbumId.ToString(), null);
			}
		}
	}
}
