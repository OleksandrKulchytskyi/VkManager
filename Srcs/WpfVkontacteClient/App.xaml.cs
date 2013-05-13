using LogModule;
using System.IO;
using System.Windows;

namespace WpfVkontacteClient
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private System.Threading.Mutex mutex = null;
		private bool isFirstRun;

		#region Properties
		public string AppFolder
		{
			get;
			private set;
		}

		internal Utils.ImageCache ImageCacheInstance
		{
			get;
			private set;
		}

		internal Utils.ByteCache DataCache
		{
			get;
			private set;
		}

		internal Utils.FriendsCache FriendsCache
		{
			get;
			private set;
		}

		internal ProgramData ProgramSettings
		{
			get;
			set;
		}

		public new static App Current
		{
			get { return (App)Application.Current; }
		}
		#endregion

		public App()
			: base()
		{
			AppFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

			this.DispatcherUnhandledException += DispatcheUnhandledHandler;
		}

		private void DispatcheUnhandledHandler(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Fatal,
												"DispatcherUnhandledException was occur in program", e.Exception.Message,
												e.Exception.InnerException != null ? e.Exception.InnerException.Message : string.Empty,
												e.Exception.StackTrace);

			MessageBox.Show(e.Exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
			if (e.Exception is System.Net.WebException)
				e.Handled = true;
			else
				Application.Current.Shutdown(0);
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			mutex = new System.Threading.Mutex(true, "WPF Vkontakte windows client", out isFirstRun);

			if (isFirstRun)
			{
				//Create ours cache repository
				ImageCacheInstance = Utils.ImageCache.OpenOrCreate(Path.Combine(AppFolder, "image.cache"));
				DataCache = Utils.ByteCache.OpenOrCreate(Path.Combine(AppFolder, "data.cache"));
				FriendsCache = Utils.FriendsCache.OpenOrCreate(Path.Combine(AppFolder, "Friends.cachedb"));
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, "Создание кеша данных успешно завершено");

				using (ConfigurationManager man = new ConfigurationManager())
				{
					ProgramSettings = man.GetProgramSettings();
					if (ProgramSettings != null)
					{
						FileInfo fi = new FileInfo(Path.Combine(AppFolder, "image.cache"));
						double fileLen = Utils.AmountUtils.ConvertBytesToMegabytes(fi.Length);
						if (fileLen > ProgramSettings.ImageCacheMaxSize)
							ImageCacheInstance.ClearAll();

						fi = new FileInfo(Path.Combine(AppFolder, "data.cache"));
						double fileLen2 = Utils.AmountUtils.ConvertBytesToMegabytes(fi.Length);
						if (fileLen2 > ProgramSettings.DataCacheMaxSize)
							DataCache.ClearAll();

						fi = null;
					}
				}

				if (!Directory.Exists("Audio"))
					Directory.CreateDirectory(Path.Combine(AppFolder, "Audio"));

				if (!Directory.Exists("Video"))
					Directory.CreateDirectory(Path.Combine(AppFolder, "Video"));

				if (!Directory.Exists("Photo"))
					Directory.CreateDirectory(Path.Combine(AppFolder, "Photo"));


				System.Threading.Thread.Sleep(200);

				using (ConfigurationManager man = new ConfigurationManager())
				{
					UserData data = null;
					AdditionalWindow.SelectUserWindow select = new AdditionalWindow.SelectUserWindow();
					select.ShowInTaskbar = true;
					select.WindowStartupLocation = WindowStartupLocation.CenterScreen;
					if (select.ShowDialog() == true)
						data = select.SelectedUser;
					else
					{
						Application.Current.Shutdown();
						LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, "Пользователь не был выбран", "Закрытие окна выбора пользователя");
						return;
					}

					WpfVkontacteClient.MainWindow main = new MainWindow();
					main.WindowStartupLocation = WindowStartupLocation.CenterScreen;
					main.ShowInTaskbar = true;
					main.UserSettings = data;
					main.Show();
				}
			}
			else
			{
				Win32Helper.ActivateFirstAppRun();
				App.Current.Shutdown();
			}

			base.OnStartup(e);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);

			if (isFirstRun)
			{
				if (this.ProgramSettings.IsClearImageCache && ImageCacheInstance != null)
				{
					int count = ImageCacheInstance.GetRecordsCount();
					ImageCacheInstance.ClearAll();
					LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, "Очистка кеша изображений");
				}

				FriendsCache.Dispose();
				DataCache.Dispose();
				ImageCacheInstance.Dispose();
			}

			if (mutex != null)
				mutex.Dispose();
		}
	}
}
