using System.Windows;
using System.Windows.Controls;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for SelectUserWindow.xaml
	/// </summary>
	public partial class SelectUserWindow : Window
	{
		public UserData SelectedUser
		{
			get { return (UserData)GetValue(SelectedUserProperty); }
			set { SetValue(SelectedUserProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SelectedUser.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SelectedUserProperty =
			DependencyProperty.Register("SelectedUser", typeof(UserData), typeof(SelectUserWindow), new FrameworkPropertyMetadata(null));

		public SelectUserWindow()
		{
			InitializeComponent();
			SelectedUser = null;
			expander1.DataContext = SelectedUser;
			this.Closing += new System.ComponentModel.CancelEventHandler(SelectUserWindow_Closing);
		}

		private void SelectUserWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (SelectedUser == null)
				Application.Current.Shutdown();
		}

		private void btnLoadUser_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedUser != null)
			{
				using (ConfigurationManager man = new ConfigurationManager())
				{
					SelectedUser = man.GetData(SelectedUser.UserName);
				}
			}
			else
			{
				MessageBox.Show("Необходимо выбрать пользователя", "", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			this.DialogResult = true;
		}

		private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
		{
			using (ConfigurationManager man = new ConfigurationManager())
			{
				if (man.RemoveData(man.GetData((cmbUsers.SelectedItem as UserData).UserName)))
					MessageBox.Show("Пользователь успешено удален", "Информация", MessageBoxButton.OK,
						 MessageBoxImage.Information);

				else
					MessageBox.Show("Ошибка удаления пользователя", "Информация", MessageBoxButton.OK,
						 MessageBoxImage.Error);
			}
		}

		private void btnCreate_Click(object sender, RoutedEventArgs e)
		{
			if (txtAppId.Text.IsNullOrEmpty())
			{
				MessageBox.Show("Application Id не может быть пустым");
				return;
			}

			using (ConfigurationManager man = new ConfigurationManager())
			{
				UserData data = new UserData();
				data.UserName = txtUserName.Text;
				data.Password = txtUserPassword.Text;
				data.Email = txtEmail.Text;

				data.AppId = long.Parse(txtAppId.Text);
				data.AccessKey = txtAccessKey.Text;
				if (man.CreateUser(data))
					MessageBox.Show("Пользователь успешено создан", "Информация", MessageBoxButton.OK,
						 MessageBoxImage.Information);

				else
					MessageBox.Show("Ошибка создания пользователя", "Информация", MessageBoxButton.OK,
						 MessageBoxImage.Error);
			}
			Refresh();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Refresh();
		}

		private void Refresh()
		{
			using (ConfigurationManager man = new ConfigurationManager())
			{
				cmbUsers.ItemsSource = man.GetAllData();
			}
		}

		private void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			Refresh();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		private void cmbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((cmbUsers.SelectedItem as UserData) != null)
			{
				this.SelectedUser = (cmbUsers.SelectedItem as UserData);
			}
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedUser != null)
			{
				if (MessageBox.Show(string.Format("Вы действительно хотите удалить пользователя \n\r {0}", SelectedUser.ToString()), "",
					MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					using (ConfigurationManager man = new ConfigurationManager())
					{
						if (man.RemoveData(SelectedUser))
						{
							MessageBox.Show("Пользователь успешно удален");
						}
						else
						{
							MessageBox.Show("Ошибка удаления");
						}

						this.Refresh();
					}
				}
			}
		}

		private void btnEdit_Click(object sender, RoutedEventArgs e)
		{
		}
	}
}