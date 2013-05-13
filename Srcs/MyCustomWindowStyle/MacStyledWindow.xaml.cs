﻿using System;
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

namespace MyCustomWindowStyle
{
	/// <summary>
	/// Interaction logic for MacStyledWindow.xaml
	/// </summary>
	public partial class MacStyledWindow : ResourceDictionary
	{
		public MacStyledWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handles the MouseLeftButtonDown event. This event handler is used here to facilitate
		/// dragging of the Window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var window = (Window)((FrameworkElement)sender).TemplatedParent;
			window.DragMove();
		}

		/// <summary>
		/// Fires when the user clicks the Close button on the window's custom title bar.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			var window = (Window)((FrameworkElement)sender).TemplatedParent;
			window.Close();
		}

		/// <summary>
		/// Fires when the user clicks the minimize button on the window's custom title bar.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void minimizeButton_Click(object sender, RoutedEventArgs e)
		{
			var window = (Window)((FrameworkElement)sender).TemplatedParent;
			window.WindowState = WindowState.Minimized;
		}

		/// <summary>
		/// Fires when the user clicks the maximize button on the window's custom title bar.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void maximizeButton_Click(object sender, RoutedEventArgs e)
		{
			var window = (Window)((FrameworkElement)sender).TemplatedParent;
			// Check the current state of the window. If the window is currently maximized, return the
			// window to it's normal state when the maximize button is clicked, otherwise maximize the window.
			if (window.WindowState == WindowState.Maximized)
				window.WindowState = WindowState.Normal;
			else window.WindowState = WindowState.Maximized;
		}
	}
}
