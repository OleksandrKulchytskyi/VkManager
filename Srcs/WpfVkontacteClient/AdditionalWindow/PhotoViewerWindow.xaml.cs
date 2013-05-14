using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Utils.Helpers;
using WpfVkontacteClient.Entities;

namespace WpfVkontacteClient
{
	/// <summary>
	/// Interaction logic for PhotoViewerWindow.xaml
	/// </summary>
	public partial class PhotoViewerWindow : Window
	{
		private GenericWeakReference<VKontakteApiWrapper> _wrapperWeak = null;

		public PhotoViewerWindow(VKontakteApiWrapper wrapper)
		{
			InitializeComponent();
			this.Closing += new System.ComponentModel.CancelEventHandler(PhotoViewerWindow_Closing);

			if (wrapper == null)
				throw new ArgumentNullException("wrapper");
			_wrapperWeak = new GenericWeakReference<VKontakteApiWrapper>(wrapper);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			cmbAlbums.ItemsSource = _wrapperWeak.Target.GetUserAlbum(null, null);
		}

		private void PhotoViewerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_wrapperWeak = null;
			Friends = null;
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
				cmbAlbums.ItemsSource = _wrapperWeak.Target.GetUserAlbum(null, null);
			}
		}

		private void cmbFriends_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((cmbFriends.SelectedItem as Friend) != null)
			{
				cmbAlbums.ItemsSource = _wrapperWeak.Target.GetUserAlbum((cmbFriends.SelectedItem as Friend).UserId.ToString(), null);
			}
		}

		private void cmbAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((cmbAlbums.SelectedItem as UserAlbum) != null)
			{
				if ((cmbFriends.SelectedItem as Friend) != null)
					carouselContr.ItemsSource = _wrapperWeak.Target.GetPhotosFromAlbum((cmbFriends.SelectedItem as Friend).UserId.ToString(),
												(cmbAlbums.SelectedItem as UserAlbum).AlbumId.ToString(), null);
				else
					carouselContr.ItemsSource = _wrapperWeak.Target.GetPhotosFromAlbum(_wrapperWeak.Target.UserId.ToString(),
													(cmbAlbums.SelectedItem as UserAlbum).AlbumId.ToString(), null);
			}
		}
	}
}