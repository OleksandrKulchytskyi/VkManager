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
using UIControls;

namespace WpfVkontacteClient.AdditionalWindow
{
	/// <summary>
	/// Interaction logic for ModernFindWindow.xaml
	/// </summary>
	public partial class ModernFindWindow : Window
	{
		public ModernFindWindow()
		{
			InitializeComponent();
			this.Closing += new System.ComponentModel.CancelEventHandler(ModernFindWindow_Closing);

			// Supply the control with the list of sections
			List<string> sections = new List<string> { "Имя", "Фамилия" };
			srchContr.SectionsList = sections;

			// Choose a style for displaying sections
			srchContr.SectionsStyle = SearchTextBox.SectionsStyles.CheckBoxStyle;
			// Add a routine handling the event OnSearch
			srchContr.OnSearch += new RoutedEventHandler(srchContr_OnSearch);

			SearchSection = string.Empty;
			SearchCriteria = string.Empty;
		}

		public string SearchCriteria { get; protected set; }
		public string SearchSection { get; protected set; }

		void ModernFindWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			srchContr.OnSearch -= new RoutedEventHandler(srchContr_OnSearch);
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
				this.DragMove();
		}

		void srchContr_OnSearch(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			SearchEventArgs searchArgs = e as SearchEventArgs;
			
			if (searchArgs == null) return;

			SearchCriteria = searchArgs.Keyword;
			if (searchArgs.Sections != null && searchArgs.Sections.Count > 0)
				SearchSection = searchArgs.Sections[0];

			this.Close();
		}

	}
}
