using System.Windows;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public ProgramData ProgramSettings
		{
			get;
			private set;
		}

		public SettingsWindow()
		{
			ProgramSettings = null;
			InitializeComponent();

			using (ConfigurationManager man = new ConfigurationManager())
			{
				this.ProgramSettings = man.GetProgramSettings();
			}

			this.Loaded += (s, e) =>
				{
					stack1.DataContext = this.ProgramSettings;
				};

			this.Closing += (s, e) =>
				{
					if (ProgramSettings != null) ProgramSettings = null;

					(this.Owner as MainWindow).Focus();
				};
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			if (ProgramSettings != null)
			{
				using (ConfigurationManager man = new ConfigurationManager())
				{
					if (man.SaveProgramSettings(ProgramSettings))
						MessageBox.Show(this, "Настройки успешно изменены.", string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			this.Close();
		}

		private void btnExit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}