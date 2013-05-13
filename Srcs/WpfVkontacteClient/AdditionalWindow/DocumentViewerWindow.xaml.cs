using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Utils.Helpers;
using WpfVkontacteClient.Entities;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for DocumentViewerWindow.xaml
	/// </summary>
	public partial class DocumentViewerWindow : Window
	{
		GenericWeakReference<VKontakteApiWrapper> _vkWrapper = null;

		public DocumentViewerWindow(VKontakteApiWrapper wrapper)
		{
			InitializeComponent();
			if (wrapper == null)
				throw new ArgumentNullException("wrapper");
			_vkWrapper = new GenericWeakReference<VKontakteApiWrapper>(wrapper);
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
			if (!IsInitialized) return;

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
				carouselContr.ItemsSource = _vkWrapper.Target.DocumentsGet((cmbFriends.SelectedItem as Friend).UserId, null);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var data = _vkWrapper.Target.DocumentsGet(null, null);
			if (data == null)
				data = new List<UserDocument>();
			carouselContr.ItemsSource = data;
			this.Closing += DocumentViewerWindow_Closing;
		}

		void DocumentViewerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Friends = null;
			_vkWrapper = null;
		}
	}
}
