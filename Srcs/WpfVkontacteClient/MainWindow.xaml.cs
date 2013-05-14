using LogModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Utils;
using Utils.Helpers;
using WpfVkontacteClient.AdditionalWindow;
using WpfVkontacteClient.Entities;

namespace WpfVkontacteClient
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private VKSettings settings;
		private VKontakteApiWrapper wrapper = null;
		private Entities.LongPollServerInfo LongPollInfo = null;
		private DispatcherTimer m_timer = null;
		private System.Net.WebClient m_webClient = null;
		private GenericWeakReference<WaitWindow> _waitWindWeak = null;
		private WebCam.NewWebCam m_newWC = null;
		private PostInfo m_selectedPost = null;

		public bool IsConected
		{
			get;
			set;
		}

		private bool IsSelfStatus
		{
			get;
			set;
		}

		public List<Friend> AllFriends
		{
			get { return (List<Friend>)GetValue(AllFriendsProperty); }
			set { SetValue(AllFriendsProperty, value); }
		}

		public static readonly DependencyProperty AllFriendsProperty =
			DependencyProperty.Register("AllFriends", typeof(List<Friend>), typeof(MainWindow), new UIPropertyMetadata(null));

		private UserInfos currentUser = null;
		private UserMessage currentMessage = null;

		public UserData UserSettings
		{
			get;
			set;
		}

		public MainWindow()
		{
			InitializeComponent();

			AllFriends = new List<Friend>();
			IsConected = false;
			IsSelfStatus = false;
			UserSettings = null;

			settings = VKSettings.Documents | VKSettings.FreindsList | VKSettings.ExMessages |
						VKSettings.Audio | VKSettings.Video | VKSettings.ApplicationNotify |
						VKSettings.ExWall | VKSettings.Status;
			/*ID приложения генерируется слудующим образом
			 * войдя на сайт ВКонтакте надо в строке браузера
			 * набрать http://www.vkontakte.ru/apps.php?act=add
			 * На открывшейся странице необходимо заполнить поля
			 * и установить тип приложения Desktop Application.
			 * Узнать ID приложения можно через пункт "Администрирование"
			 * */

			this.Closing += (sender, arg) =>
				{
					using (var webCl = new System.Net.WebClient())
					{
						byte[] rezData = webCl.DownloadData("http://vkontakte.ru/login.php?op=logout");
						string resp = Encoding.UTF8.GetString(rezData);
						if (resp.Length > 10)
						{
						}
					}

					if (this.OwnedWindows.Count == 0)
					{
						CleanObjects();
						Application.Current.Shutdown();
					}
					else
					{
						foreach (Window window in this.OwnedWindows)
						{
							window.Close();
						}
						CleanObjects();
						Application.Current.Shutdown();
					}
				};
		}

		private void CleanObjects()
		{
			if (wrapper != null)
				wrapper = null;

			if (UserSettings != null)
				UserSettings = null;

			if (!currentUser.IsNull())
				currentUser = null;

			if (this.currentMessage != null)
				this.currentMessage = null;

			if (AllFriends != null && AllFriends.Count > 0)
				AllFriends.Clear();

			if (m_timer != null && m_timer.IsEnabled)
				m_timer.Stop();

			if (m_webClient != null)
			{
				m_webClient.DownloadDataCompleted -= new System.Net.DownloadDataCompletedEventHandler(m_webClient_DownloadDataCompleted);
				m_webClient.Dispose();
			}
		}

		#region Helpful methods

		private DependencyObject GetChildrenChck(DependencyObject element)
		{
			DependencyObject dep = element;
			if (dep != null && VisualTreeHelper.GetChildrenCount(dep) > 0)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dep); i++)
				{
					dep = VisualTreeHelper.GetChild(dep, i);
					if ((dep != null) && (dep is CheckBox))
						break;
				}
				return dep;
			}
			return null;
		}

		public List<T> GetVisualChildCollection<T>(object parent) where T : Visual
		{
			List<T> visualCollection = new List<T>();
			GetVisualChildCollection(parent as DependencyObject, visualCollection);
			return visualCollection;
		}

		private void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
		{
			int count = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < count; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);
				if (child is T)
				{
					visualCollection.Add(child as T);
				}
				else if (child != null)
				{
					GetVisualChildCollection(child, visualCollection);
				}
			}
		}

		private T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						return (T)child;
					}
					T childItem = FindVisualChild<T>(child);
					if (childItem != null)
						return childItem;
				}
			} return null;
		}

		public static T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				string controlName = child.GetValue(Control.NameProperty) as string;
				if (controlName == name)
				{
					return child as T;
				}
				else
				{
					T result = FindVisualChildByName<T>(child, name);
					if (result != null)
						return result;
				}
			}
			return null;
		}

		#endregion Helpful methods

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			DoLogin(UserSettings.AppId, UserSettings.AccessKey);
			stackStatus.DataContext = this;
		}

		private void DoLogin(long appId, string accessKey)
		{
			wrapper = new VKontakteApiWrapper(appId, accessKey, (int)settings);

			if (wrapper.Connect(this))
			{
				_waitWindWeak = new GenericWeakReference<WaitWindow>(new WaitWindow());
				_waitWindWeak.Target.Owner = this;
				_waitWindWeak.Target.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
				_waitWindWeak.Target.Show();
				IsConected = wrapper.IsConnected;
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, "User logged in in the system.", UserSettings.UserName);

				var task = System.Threading.Tasks.Task.Factory.StartNew(new Action(() =>
				{
					if ((Application.Current as App).FriendsCache.IsEmpty())
					{
						InitializeFriendsCache();
					}
					else
					{
						AllFriends.Clear();
						foreach (var item in (Application.Current as App).FriendsCache.GetAllDataFromFriends())
						{
							AllFriends.Add(new Friend(item.Uid, item.Name, item.LastName, item.NickName, item.Country,
															item.City, item.PhotoUrl));
						}

						if (wrapper.FriendsCount(null).HasValue && wrapper.FriendsCount(null).Value != AllFriends.Count)
							InitializeFriendsCache();
					}
				}), System.Threading.CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.FromCurrentSynchronizationContext());

				task.ContinueWith(t =>
				{
					if (_waitWindWeak != null)
					{
						_waitWindWeak.Target.Close();
						_waitWindWeak = null;
					}
					friendsList.ItemsSource = AllFriends;
					t.Dispose();
				}, System.Threading.CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());

				task.ContinueWith(t =>
				{
					if (_waitWindWeak != null)
					{
						_waitWindWeak.Target.Close();
						_waitWindWeak = null;
					}
					t.Dispose();
					friendsList.ItemsSource = AllFriends;
					MessageBox.Show(this, "Возникла ошибка при загрузке друзей. \n\r Пожалуйста, перезагрузите программу.");
					t.Dispose();
				}, System.Threading.CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext());

				if ((Application.Current as App).ProgramSettings.DetermineNewMessages)
				{
					LongPollInfo = wrapper.GetLongPollServerConnetInfo();

					m_timer = new DispatcherTimer();
					m_timer.Interval = new TimeSpan(0, 0, 22);
					m_timer.Tick += new EventHandler(m_timer_Tick);
					m_timer.Start();
				}
			}
			else
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Error, "User logging error", UserSettings.UserName);

				MessageBox.Show(this, "Вы не автоизировались на сервере Вконтакте.", "Ошибка",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>
		/// Initialize frindsCache from Vkontakte and load it to friendscahce db
		/// </summary>
		private void InitializeFriendsCache()
		{
			LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, "Инициализация кеша друзей");
			AllFriends = wrapper.GetAllFriends(null, null, null);
			if (AllFriends != null && AllFriends.Count > 0)
			{
				foreach (Friend friend in AllFriends)
				{
					(Application.Current as App).FriendsCache.AddToFriendsTable(friend.UserId, friend.FirstName, friend.LastName,
																				friend.Nick, friend.Country, friend.City, friend.Picture);
				}
			}

			var dict = (Application.Current as App).FriendsCache.GetFriendsWithoutDownloadedPhotoWithUrl();
			if (dict.Count > 0)
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, "Подготовка инициализация фотографий для кеша друзей");
				AdditionalWindow.WindowAvatarLoader loader = new WindowAvatarLoader(dict);
				loader.Owner = this;
				loader.SnapsToDevicePixels = true;
				loader.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
				loader.ShowDialog();
			}
		}

		private void m_timer_Tick(object sender, EventArgs e)
		{
			if (m_webClient == null)
			{
				m_webClient = new System.Net.WebClient();
				m_webClient.DownloadDataCompleted += new System.Net.DownloadDataCompletedEventHandler(m_webClient_DownloadDataCompleted);
			}

			string conInfo = string.Concat("http://", LongPollInfo.Server, "?act=a_check&key=", LongPollInfo.Key, "&ts=", LongPollInfo.Ts.ToString(), "&wait=25");
			//string connection = string.Format("http://{0}?act=a_check&key={1}&ts={2}&wait=25", LongPollInfo.Server, LongPollInfo.Key, LongPollInfo.Ts);
			if (!m_webClient.IsBusy)
				m_webClient.DownloadDataAsync(new Uri(conInfo));
		}

		private void m_webClient_DownloadDataCompleted(object sender, System.Net.DownloadDataCompletedEventArgs e)
		{
			if (e.Error == null && !e.Cancelled)
			{
				string response = Encoding.UTF8.GetString(e.Result);
				if (LongPollServerParser.LongPoolServerContainsError(response) ||
					LongPollServerParser.IsLongPoolServerResponseContentEmpty(response))
					return;

				LongPollInfo.Ts = long.Parse(LongPollServerParser.GetNewTsValue(response));

				string updates = LongPollServerParser.ParseServerResponse(response);
				string[] updItems = updates.Split(';');
				Dictionary<string, int> msgData = new Dictionary<string, int>();
				bool isFirstInvoke = true;
				StringBuilder sbOnlineData = new StringBuilder();

				if (updItems.Length > 0)
				{
					foreach (string item in updItems)
					{
						string withoutall = item.Replace('{', ' ').Replace('}', ' ').Replace('[', ' ').Replace('[', ' ').Trim();
						switch (withoutall[0])
						{
							case '4':
								if (withoutall.Split(',')[2].Trim() == "35") continue;

								if ((Application.Current as App).FriendsCache.IsFriendCached(long.Parse(withoutall.Split(',')[3])))
								{
									if (msgData.ContainsKey((Application.Current as App).FriendsCache.GetOnlyUserFullName(long.Parse(withoutall.Split(',')[3]))))
										msgData[((Application.Current as App).FriendsCache.GetOnlyUserFullName(long.Parse(withoutall.Split(',')[3])))] = +1;
									else
										msgData.Add(((Application.Current as App).FriendsCache.GetOnlyUserFullName(long.Parse(withoutall.Split(',')[3]))), 1);
								}
								else
								{
									if (msgData.ContainsKey(withoutall.Split(',')[3]))
										msgData[withoutall.Split(',')[3]] = +1;
									else
										msgData.Add(withoutall.Split(',')[3], 1);
								}
								break;

							case '8':
								if (isFirstInvoke)
								{
									isFirstInvoke = false;
									sbOnlineData.AppendLine("Online пользователи:");
								}
								if ((Application.Current as App).FriendsCache.IsFriendCached(long.Parse(withoutall.Split(',')[1].Replace('-', ' ').Trim())))
								{
									sbOnlineData.AppendLine((Application.Current as App).FriendsCache.GetOnlyUserFullName(
																long.Parse(withoutall.Split(',')[1].Replace('-', ' ').Trim())));
								}
								break;

							case '9':
								if (isFirstInvoke)
								{
									isFirstInvoke = false;
									sbOnlineData.AppendLine("Offline пользователи:");
								}
								if ((Application.Current as App).FriendsCache.IsFriendCached(long.Parse(withoutall.Split(',')[1].Replace('-', ' ').Trim())))
								{
									sbOnlineData.AppendLine((Application.Current as App).FriendsCache.GetOnlyUserFullName(
																long.Parse(withoutall.Split(',')[1].Replace('-', ' ').Trim())));
								}
								break;

							default:
								break;
						}//end switch
					}//end foreach

					#region creating user notification message

					if (msgData.Count > 0)
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendLine("Вы получили новое(ые) сообшение(я) от:");
						foreach (var msgInf in msgData)
						{
							sb.AppendLine(string.Format("Пользователь: {0}, количество:{1}", msgInf.Key, msgInf.Value));
						}

						NotificationWindow windNotify = new NotificationWindow();
						windNotify.LayoutRoot.DataContext = sb.ToString();
						windNotify.Owner = this;
						windNotify.Show();
						msgData.Clear();
					}
					if (sbOnlineData.ToString().Length > 10)
					{
						NotificationWindow windNotify = new NotificationWindow();
						windNotify.LayoutRoot.DataContext = sbOnlineData.ToString();
						windNotify.Owner = this;
						windNotify.Show();
						sbOnlineData.Clear();
					}

					#endregion creating user notification message
				}//end main if
			}
		}

		private void miExit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void miAbout_Click(object sender, RoutedEventArgs e)
		{
			AboutWindow about = new AboutWindow();
			about.Owner = this;
			about.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
			about.ShowDialog();
		}

		private void btnLoadFriends_Click(object sender, RoutedEventArgs e)
		{
			if (AllFriends == null && friendsList.ItemsSource == null)
			{
				InitializeFriendsCache();
				friendsList.ItemsSource = AllFriends;
				return;
			}
			friendsList.ItemsSource = null;
			friendsList.ItemsSource = AllFriends;
		}

		private void btnLoadOnFriends_Click(object sender, RoutedEventArgs e)
		{
			this.lstOnline.ItemsSource = wrapper.GetInfoForOnlineFriends(wrapper.GetUidOnlineFriends());
		}

		private void btnLoadInpMessage_Click(object sender, RoutedEventArgs e)
		{
			if ((cmbMessageType.SelectedItem as ComboBoxItem).Content.ToString() == "Входящие")
			{
				messagesInput.ItemsSource = wrapper.GetUserMessages(null, null, "100", VkMessageFilter.FromFriends, null, null);
			}
			else if ((cmbMessageType.SelectedItem as ComboBoxItem).Content.ToString() == "Отправленые")
			{
				messagesInput.ItemsSource = wrapper.GetUserMessages("1", null, "100", VkMessageFilter.FromFriends, null, null);
			}
			else
			{
				messagesInput.ItemsSource = wrapper.GetUserMessages(null, null, "100", VkMessageFilter.NeverReaded, null, null);
			}
		}

		private void messagesInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((messagesInput.SelectedItem as UserMessage) != null)
			{
				currentMessage = (messagesInput.SelectedItem as UserMessage);

				currentUser = wrapper.GetUserInfo((messagesInput.SelectedItem as UserMessage).UserId.ToString());
				if (currentUser != null)
				{
					txtMessFio.Text = currentUser.ToString();
					txtMessageSex.Text = Extension.StringToSex(currentUser.Sex);
					txtMessageTitle.Text = (messagesInput.SelectedItem as UserMessage).MessageTitle.EscapeXmlString();

					txtMessageBody.Visibility = currentMessage.HasAttachment ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
					lblMsgText.Visibility = txtMessageBody.Visibility;
					btnGetMsgAttachmnt.Visibility = currentMessage.HasAttachment ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

					if (txtMessageBody.Visibility == System.Windows.Visibility.Visible)
						txtMessageBody.Text = (messagesInput.SelectedItem as UserMessage).MessageBody.EscapeXmlString();

					BitmapImage img = new BitmapImage(new Uri(currentUser.PhotoMediumUrl, UriKind.RelativeOrAbsolute));
					imgFriendMes.Source = img;
				}
			}
		}

		private void btnMsgReply_Click(object sender, RoutedEventArgs e)
		{
			if (currentUser != null && currentUser.Id > 0)
			{
				SendMessageWindow send = new SendMessageWindow(this.wrapper, this.currentUser, this.currentMessage);
				send.Owner = this;
				send.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
				send.ShowDialog();
			}
			else
				MessageBox.Show(this, "Выберите пользователя которому необходимо отправить сообщение!", "!!!",
					 MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void btnMsgAttachment_Click(object sender, RoutedEventArgs e)
		{

		}

		private void MarkAsRead_Click(object sender, RoutedEventArgs e)
		{
			if (currentMessage != null)
			{
				if (wrapper.MarkMessageAsRead(currentMessage.MessageId.ToString()))
				{
					currentMessage.ReadState = true;
				}
			}
		}

		private void MarkAsNew_Click(object sender, RoutedEventArgs e)
		{
			if (currentMessage != null)
			{
				if (wrapper.MarkMessageAsNew(currentMessage.MessageId.ToString()))
					currentMessage.ReadState = false;
			}
		}

		private void RemoveMsg_Click(object sender, RoutedEventArgs e)
		{
			if (currentMessage != null)
			{
				if (wrapper.DeleteMessage(currentMessage.MessageId.ToString()))
				{
					messagesInput.ItemsSource = wrapper.GetUserMessages(null, null, "100", VkMessageFilter.FromFriends, null, null);
					MessageBox.Show(this, "Сообщение удалено", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
					MessageBox.Show(this, "Сообщение не удалено", "!!!!", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void mainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((sender as TabControl) != null && ((sender as TabControl).SelectedItem as TabItem) != null)
			{
				switch (((sender as TabControl).SelectedItem as TabItem).Header.ToString())
				{
					case "Фотографии":
						if ((cmbPhotoFriends.ItemsSource as List<Friend>) == null)
						{
							AllFriends = wrapper.GetAllFriends(null, null, null);
							cmbPhotoFriends.ItemsSource = AllFriends;
						}
						break;

					default:
						break;
				}
			}
		}

		private void cmbPhotoFriends_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((cmbPhotoFriends.SelectedItem as Friend) != null)
				lstUserAlbum.ItemsSource = wrapper.GetUserAlbum((cmbPhotoFriends.SelectedItem as Friend).UserId.ToString(), null);

			else
				lstUserAlbum.ItemsSource = wrapper.GetUserAlbum(null, null);
		}

		private void lstUserAlbum_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((lstUserAlbum.SelectedItem as UserAlbum) != null)
				lstPhotos.ItemsSource = wrapper.GetPhotosFromAlbum((cmbPhotoFriends.SelectedItem as Friend).UserId.ToString(),
																(lstUserAlbum.SelectedItem as UserAlbum).AlbumId.ToString(),
																null);
		}

		private void photoItem_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			UIElement elem = (UIElement)lstPhotos.InputHitTest(e.GetPosition(lstPhotos));
			while (elem != lstPhotos)
			{
				if (elem is ListBoxItem)
				{
					object selectedItem = ((ListBoxItem)elem).Content;
					// Handle the double click here
					if ((lstPhotos.SelectedItem as PhotoExteded) != null)
					{
						PhotoPreviewWindow previewWindow = new PhotoPreviewWindow(lstPhotos.SelectedItem as PhotoExteded,
																					this.wrapper);
						previewWindow.Owner = this;
						previewWindow.ShowDialog();
					}
					return;
				}
				elem = (UIElement)VisualTreeHelper.GetParent(elem);
			}
		}

		private void btnUpdateAudio_Click(object sender, RoutedEventArgs e)
		{
			if ((cmbAudioSource.SelectedItem as ComboBoxItem).Content.ToString().ToLower().Contains("мои аудиозаписи"))
				this.lstAudio.ItemsSource = wrapper.GetUserAudio(null, null, null, "0");
		}

		private void lstFriendsAudio_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((lstFriendsAudio.SelectedItem as Friend) != null)
				this.lstAudio.ItemsSource = wrapper.GetUserAudio((lstFriendsAudio.SelectedItem as Friend).UserId.ToString(),
																	null, null, "0");
		}

		private void lstAudio_DblClick(object sender, MouseButtonEventArgs e)
		{
			UIElement elem = (UIElement)lstAudio.InputHitTest(e.GetPosition(lstAudio));
			while (elem != lstAudio)
			{
				if (elem is ListBoxItem)
				{
					object selectedItem = ((ListBoxItem)elem).Content;
					// Handle the double click here
					if (selectedItem != null && selectedItem is UserAudio)
					{
						WindowAudio audio = new WindowAudio(selectedItem as UserAudio);
						audio.Owner = this;
						audio.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
						audio.ShowDialog();
					}

					return;
				}
				elem = (UIElement)VisualTreeHelper.GetParent(elem);
			}
		}

		private void SelectAllExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			List<UserAudio> downAudio = new List<UserAudio>();
			foreach (var item in lstAudio.Items)
			{
				if (item is UserAudio && (item as UserAudio).IsCheckedInList)
					downAudio.Add((item as UserAudio));
			}

			if (downAudio == null || downAudio.Count == 0)
				return;

			DownloadAudio downl = new DownloadAudio(downAudio);
			downl.Owner = this;
			downl.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
			downl.ShowInTaskbar = true;
			downl.Show();
		}

		private void SelectAllVideoExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			List<UserVideo> downVideo = new List<UserVideo>();
			foreach (var item in lstVideo.Items)
			{
				if (item is UserVideo && (item as UserVideo).IsCheckedInList)
					downVideo.Add((item as UserVideo));
			}

			if (downVideo == null || downVideo.Count == 0)
				return;

			DownloadVideo downl = new DownloadVideo(downVideo);
			downl.Owner = this;
			downl.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
			downl.ShowInTaskbar = true;
			downl.Show();
		}

		private void btnUpdateVideo_Click(object sender, RoutedEventArgs e)
		{
			if ((cmbVideoSource.SelectedItem as ComboBoxItem).Content.ToString().ToLower().Contains("мои видеозаписи"))
				this.lstVideo.ItemsSource = wrapper.GetVideo(null, null, null, "150", null);
		}

		private void lstFriendsVideo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((lstFriendsVideo.SelectedItem as Friend) != null)
				this.lstVideo.ItemsSource = wrapper.GetUserVideo((lstFriendsVideo.SelectedItem as Friend).UserId.ToString(),
																	null, null);
		}

		private void lstVideo_DblClick(object sender, MouseButtonEventArgs e)
		{
			UIElement elem = (UIElement)lstVideo.InputHitTest(e.GetPosition(lstVideo));
			while (elem != lstVideo)
			{
				if (elem is ListBoxItem)
				{
					object selectedItem = ((ListBoxItem)elem).Content;
					// Handle the double click here
					if (selectedItem != null && selectedItem is UserVideo)
					{
						//Todo: add logic here

						//WindowAudio audio = new WindowAudio(selectedItem as UserAudio);
						//audio.Owner = this;
						//audio.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
						//audio.ShowDialog();
					}
					return;
				}
				elem = (UIElement)VisualTreeHelper.GetParent(elem);
			}
		}

		private void btnUpdateStatus_Click(object sender, RoutedEventArgs e)
		{
			if ((cmbStatusSource.SelectedItem as ComboBoxItem).Content.ToString() == "Мой Статус")
			{
				this.txtStatus.Text = wrapper.GetUserStatus(null);
				this.IsSelfStatus = true;
			}
		}

		private void lstFriendsStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.txtStatus.Text = wrapper.GetUserStatus((lstFriendsStatus.SelectedItem as Friend).UserId.ToString());
		}

		private void btnChngStatus_Click(object sender, RoutedEventArgs e)
		{
			if (wrapper.SetUserStatus(txtStatus.Text))
			{
				MessageBox.Show(this, "Статус успешно изменён", "", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			else
				MessageBox.Show(this, "Ошибка изменения статуса");
		}

		private void FindMsg_Click(object sender, RoutedEventArgs e)
		{
			FindWindow find = new FindWindow();
			find.Owner = this;
			if (find.ShowDialog() == true)
			{
				messagesInput.ItemsSource = wrapper.SearchMessage(find.SearchText, null, "100");
			}
		}

		private void PlayVideo_Click(object sender, RoutedEventArgs e)
		{
			if ((lstVideo.SelectedItem as UserVideo) != null)
			{
				VideoPlayWindow play = new VideoPlayWindow((lstVideo.SelectedItem as UserVideo).PlayerUrl);
				play.Owner = this;
				play.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
				play.ShowDialog();
			}
		}

		private void miSettings_Click(object sender, RoutedEventArgs e)
		{
			AdditionalWindow.SettingsWindow settings = new SettingsWindow();
			settings.Owner = this;
			settings.Show();
		}

		private void btnWallGetAll_Click(object sender, RoutedEventArgs e)
		{
			List<PostInfo> postInfos = null;
			if ((cmbWallUsers.SelectedItem as ComboBoxItem).Content.ToString() == "Моя стена")
				postInfos = wrapper.WallGetPosts(null);
			else
				postInfos = wrapper.WallGetPosts((lstWallFriends.SelectedItem as Friend).UserId.ToString());

			lstWallPosts.ItemsSource = null;
			lstWallPosts.ItemsSource = postInfos;
		}

		private void cmbWallUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (((e.Source as ComboBox).SelectedItem as ComboBoxItem).Content.ToString().Contains("Моя друзья"))
			{
				if (AllFriends == null || AllFriends.Count == 0)
				{
					AllFriends = wrapper.GetAllFriends(null, null, null);
				}
				lstWallFriends.ItemsSource = AllFriends;
			}
		}

		private void lstWallPosts_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.m_selectedPost = (sender as ListBox).SelectedItem as PostInfo;
		}

		private void expWallMedia_Expanded(object sender, RoutedEventArgs e)
		{
			AttachmentInfo attach = ((sender as Expander).DataContext as PostInfo).Attachment;
			Grid grid = (sender as Expander).FindName("wallExpGrid") as Grid;

			if (attach != null && grid != null)
			{
				if (attach.Audio == null)
				{
					Controls.MyMediaControl audio = FindVisualChildByName<Controls.MyMediaControl>(grid, "myWallPlayer");
					if (audio != null)
						audio.Visibility = System.Windows.Visibility.Collapsed;
				}

				if (attach.Photo != null || attach.Video != null || attach.Graffiti != null)
				{
					Image img = FindVisualChildByName<Image>(grid, "imgWallItem");
					if (img != null)
					{
						img.Visibility = System.Windows.Visibility.Visible;
						switch (attach.Type.Trim().ToLower())
						{
							case "photo":
								BitmapImage bmp = new BitmapImage();
								bmp.BeginInit();
								bmp.DecodePixelHeight = 150;
								bmp.DecodePixelWidth = 130;
								bmp.UriSource = new Uri(attach.Photo.Source, UriKind.RelativeOrAbsolute);
								bmp.EndInit();
								img.Source = bmp;
								break;

							case "video":
								BitmapImage bmpVid = new BitmapImage();
								bmpVid.BeginInit();
								bmpVid.DecodePixelHeight = 150;
								bmpVid.DecodePixelWidth = 130;
								bmpVid.UriSource = new Uri(attach.Video.Image, UriKind.RelativeOrAbsolute);
								bmpVid.EndInit();
								img.Source = bmpVid;
								break;

							case "graffiti":
								BitmapImage bmpGraf = new BitmapImage();
								bmpGraf.BeginInit();
								bmpGraf.DecodePixelHeight = 150;
								bmpGraf.DecodePixelWidth = 130;
								bmpGraf.UriSource = new Uri(attach.Graffiti.Source, UriKind.RelativeOrAbsolute);
								bmpGraf.EndInit();
								img.Source = bmpGraf;
								break;

							default:
								break;
						}
					}
				}

				if (attach.Note != null)
				{
					TextBlock note = FindVisualChildByName<TextBlock>(grid, "txtWallNote");
					if (note != null)
					{
						note.TextWrapping = TextWrapping.Wrap;
						note.Visibility = System.Windows.Visibility.Visible;
					}
				}

				if (attach.Type == "app")
				{
					TextBlock note = FindVisualChildByName<TextBlock>(grid, "txtWallNote");
					if (note != null)
					{
						note.TextWrapping = TextWrapping.Wrap;
						note.Text = "Данный тип контента не поддерживается програмой!";
						note.Visibility = System.Windows.Visibility.Visible;
					}
				}
			}
		}

		private void expWallMedia_Collapsed(object sender, RoutedEventArgs e)
		{
			AttachmentInfo attach = ((sender as Expander).DataContext as PostInfo).Attachment;
			Grid grid = (sender as Expander).FindName("wallExpGrid") as Grid;

			if (attach != null && grid != null)
			{
				if (attach.Audio != null)
				{
					Controls.MyMediaControl audio = FindVisualChildByName<Controls.MyMediaControl>(grid, "myWallPlayer");
					if (audio != null && audio.AudioPlaying)
						audio.StopAllPlaying();
				}

				if (attach.Photo != null || attach.Video != null || attach.Graffiti != null)
				{
					Image img = FindVisualChildByName<Image>(grid, "imgWallItem");
					if (img != null)
					{
						img.Source = null;
						img.Visibility = System.Windows.Visibility.Collapsed;
					}
				}

				if (attach.Type == "app")
				{
					TextBlock note = FindVisualChildByName<TextBlock>(grid, "txtWallNote");
					if (note != null)
					{
						note.TextWrapping = TextWrapping.Wrap;
						note.Text = string.Empty;
						note.Visibility = System.Windows.Visibility.Collapsed;
					}
				}

				GC.Collect(0, GCCollectionMode.Optimized);
			}
		}

		private void cmbStatusSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsInitialized)
				return;

			if ((sender as ComboBox).SelectedIndex == 1)
				lstFriendsStatus.ItemsSource = AllFriends;
		}

		private void cmbVideoSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsInitialized)
				return;
			if (lstFriendsVideo.ItemsSource == null)
				lstFriendsVideo.ItemsSource = AllFriends;
		}

		private void cmbAudioSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsInitialized) return;

			if (lstFriendsAudio.ItemsSource == null)
				lstFriendsAudio.ItemsSource = AllFriends;
		}

		private void miAudioFolder_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(System.IO.Path.Combine((Application.Current as App).AppFolder, "Audio"));
		}

		private void miVideoFolder_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(System.IO.Path.Combine((Application.Current as App).AppFolder, "Video"));
		}

		private void miAlbums_Click(object sender, RoutedEventArgs e)
		{
			if (AllFriends == null || AllFriends.Count == 0)
			{
				MessageBox.Show(this, "Ошибка инициализации друзей");
				return;
			}

			PhotoViewerWindow albumsViewer = new PhotoViewerWindow(this.wrapper, AllFriends);
			albumsViewer.Owner = this;
			albumsViewer.ShowDialog();
		}

		private void miDocs_Click(object sender, RoutedEventArgs e)
		{
			DocumentViewerWindow viewer = new DocumentViewerWindow(wrapper, AllFriends);
			viewer.ShowInTaskbar = false;
			viewer.SnapsToDevicePixels = true;
			viewer.ShowActivated = true;
			viewer.ShowDialog();
		}

		private void friendsList_KeyDown(object sender, KeyEventArgs e)
		{
			if (!(sender is ListBox) && !((sender as ListBox).ItemsSource is List<Friend>))
				return;

			if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.F))
			{
				ModernFindWindow findWind = new ModernFindWindow();
				findWind.Owner = this;
				findWind.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
				if (findWind.ShowDialog() == true)
				{
					if (string.IsNullOrEmpty(findWind.SearchSection) || findWind.SearchSection == "Имя")
						(sender as ListBox).ItemsSource = from item in AllFriends where item.FirstName.Contains(findWind.SearchCriteria) select item;
					else
						(sender as ListBox).ItemsSource = from item in AllFriends where item.LastName.Contains(findWind.SearchCriteria) select item;
				}
			}
			else if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.Z))
				(sender as ListBox).ItemsSource = AllFriends;

			e.Handled = true;
		}

		private void miWebCam_Click(object sender, RoutedEventArgs e)
		{
			var strCol = new System.Collections.Specialized.StringCollection();
			strCol.Add("http://vkontakte.ru/video24571991_159586876");
			strCol.Add("http://vkontakte.ru/video24571991_143990466?section=all");

			var list = Utils.StringUtils.GetUserAndObjectIDFromUrl(strCol);

			if (list.Count > 0)
			{
			}
			//m_newWC = new WebCam.NewWebCam();
			//m_newWC.SetWindow(this);
			//m_newWC.Attach();

			//WebCamWindow webCam = new WebCamWindow();
			//webCam.DeviceName = "Microsoft WDM Image Capture (Win32)";
			//webCam.Owner = this;
			//webCam.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
			//webCam.Show();
			//webCam.webCamElement.Play();

			//System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(this);
			//using (WpfVkontacteClient.WebCam.WebCamGrabber cam = new WebCam.WebCamGrabber(300, 400, 25, 0, helper.Handle))
			//{
			//    cam.OnCameraFrame += new WebCam.WebCameraFrameHandler(cam_OnCameraFrame);
			//    cam.Start();
			//    cam.Stop();
			//    cam.OnCameraFrame -= new WebCam.WebCameraFrameHandler(cam_OnCameraFrame);
			//}
		}

		private void cam_OnCameraFrame(object sender, WebCam.WebCameraEventArgs e)
		{
			using (FileStream ms = new FileStream("C:\\1.bmp", FileMode.CreateNew))
			{
				e.Frame.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
				//ms.Position = 0;
				//BitmapImage bi = new BitmapImage();
				//bi.BeginInit();
				//bi.StreamSource = ms;
				//bi.EndInit();
			}
		}

		private void listboxItem_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
			{
				if (sender is ListBoxItem)
				{
					var ch = FindVisualChild<CheckBox>(sender as ListBoxItem);
					if (ch == null) return;

					if (ch.IsChecked == true)
						ch.IsChecked = false;
					else
						ch.IsChecked = true;
				}
			}
		}

		private void clearAllSelDownloads(object sender, RoutedEventArgs e)
		{
		}

		private void miDownVideoLinks_Click(object sender, RoutedEventArgs e)
		{
			VideoLinksWindow wind = new VideoLinksWindow();
			wind.Owner = this;
			wind.ShowInTaskbar = false;
			wind.ShowDialog();
		}
	}
}