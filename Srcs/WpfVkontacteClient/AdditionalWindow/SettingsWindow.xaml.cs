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
					if (ProgramSettings != null)
					{
						ProgramSettings = null;
						GC.Collect();
						GC.WaitForPendingFinalizers();
					}

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
					{
						MessageBox.Show("Настройки успешно изменены", "", MessageBoxButton.OK,
							 MessageBoxImage.Information);
					}
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
