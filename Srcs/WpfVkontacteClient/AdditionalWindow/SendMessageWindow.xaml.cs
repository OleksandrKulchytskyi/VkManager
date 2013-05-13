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
	/// Interaction logic for SendMessageWindow.xaml
	/// </summary>
	public partial class SendMessageWindow : Window
	{
		private UserInfos userInfos
		{
			get;
			set;
		}
		
		private UserMessage UsrMsg
		{
			get;
			set;
		}

		private VKontakteApiWrapper wrapper = null;
		private long MessageId = 0;

		public SendMessageWindow(VKontakteApiWrapper wrapper, UserInfos info, UserMessage msgToReply)
		{
			InitializeComponent();

			if (!msgToReply.IsNull())
			{
				UsrMsg = msgToReply;
				txtSubect.Text = this.UsrMsg.MessageTitle;
			}

			if (info != null)
				this.userInfos = info;
			if (wrapper != null)
				this.wrapper = wrapper;
		}

		void closeButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			CleanUp();
			this.Close();
		}

		private void CleanUp()
		{
			if (!userInfos.IsNull())
				userInfos = null;

			if (wrapper != null)
				wrapper = null;

			if (UsrMsg != null)
				UsrMsg = null;

			GC.Collect(0);
		}

		private void btnSend_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(txtMessage.Text))
			{
				MessageBox.Show("Вы не можете послать пустое сообщение", "Введите текст сообщения", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			this.MessageId = wrapper.SendUserMessage(userInfos.Id.ToString(), txtMessage.Text, txtSubect.Text, false);
			string infoMsg = string.Format("Сообщение отправлено {0}",
										(Application.Current as App).FriendsCache.GetOnlyUserFullName(userInfos.Id));
			msgInfoPopup.PlacementTarget = this;
			msgInfoPopup.DataContext = infoMsg;
			msgInfoPopup.IsOpen = true;

			txtMessage.Text = string.Empty;
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			CleanUp();
			this.DialogResult = false;
			this.Close();
		}

		private void exp1_Expanded(object sender, RoutedEventArgs e)
		{
			if (userInfos != null)
			{
				lstMsgHistory.ItemsSource = wrapper.GetMessageHistory(userInfos.Id.ToString(), null, "100");
			}
			this.Height = 420;
			lstMsgHistory.Height = lstMsgHistory.Height + 100;
		}

		private void exp1_Collapsed(object sender, RoutedEventArgs e)
		{
			this.Height = 265;
			lstMsgHistory.Height = lstMsgHistory.Height - 100;
		}

		void Convert_click(object sender, RoutedEventArgs e)
		{
			if (e.OriginalSource is MenuItem)
			{
				if (((sender as MenuItem).Header as string).ToLower().Contains("русс"))
				{
					txtMessage.Text = Utils.KeybordUtils.KeyboardSwitch(txtMessage.Text);
				}
				else
					txtMessage.Text = Utils.KeybordUtils.KeyboardSwitch(txtMessage.Text);
			}
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			var button = (sender as Button);
			if (button != null)
			{
				var popup = Extension.TryFindParent<System.Windows.Controls.Primitives.Popup>(button);
				if (popup == null)
					return;
				popup.IsOpen = false;
			}
		}
	}
}
