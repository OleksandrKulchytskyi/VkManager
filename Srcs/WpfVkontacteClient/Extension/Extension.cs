using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfVkontacteClient
{
	public static class Extension
	{
		/// <summary>
		/// Конвертируем строку в булевское значение
		/// </summary>
		/// <param name="value">значение(1\0)</param>
		/// <returns>true or false</returns>
		public static bool FromStringToBool(string value)
		{
			if (value.ToLower() == "1")
				return true;
			return false;
		}

		public static string StringToSex(string value)
		{
			if (value.ToLower() == "1")
				return "Жениский";
			return "Мужской";
		}

		public static string NumberToCountry(string countryNumber)
		{
			switch (countryNumber.Trim())
			{
				case "1":
					{ return "Россия"; }

				case "2":
					{ return "Украина"; }

				default:
					{ return string.Empty; }
			}
		}

		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}

		public static bool IsNull(this object obj)
		{
			return obj == null ? true : false;
		}

		public static ImageSource GetImage(this string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				return null;
			}
			byte[] photo = App.Current.ImageCacheInstance.GetImage(url);

			if (photo != null)
			{
				BitmapImage image = new BitmapImage();
				image.BeginInit();
				image.StreamSource = new System.IO.MemoryStream(photo);
				image.EndInit();
				return image;
			}
			else
			{
				//ManualResetEventSlim e=new ManualResetEventSlim(false);
				BitmapImage image = null;
				//byte[] array = null;
				//ImageDownloader.DownloadImage(url, new Action<bool, byte[]>(
				//                                    delegate(bool ok, byte[] bytes)
				//                                        {
				//                                            if (ok)
				//                                            {
				//                                                array = bytes;
				//                                                e.Set();
				//                                            }

				//                                        }));
				//if (e.Wait(20000))
				//{
				//    image=new BitmapImage();
				//    image.BeginInit();
				//    image.StreamSource = new MemoryStream(array);
				//    image.EndInit();
				//}
				image = new BitmapImage();
				image.BeginInit();
				image.UriSource = new Uri(url);
				image.EndInit();
				return image;
			}
		}

		/// <summary>
		/// Finds a parent of a given item on the visual tree.
		/// </summary>
		/// <typeparam name="T">The type of the queried item.</typeparam>
		/// <param name="child">A direct or indirect child of the
		/// queried item.</param>
		/// <returns>The first parent item that matches the submitted
		/// type parameter. If not matching item can be found, a null
		/// reference is being returned.</returns>
		public static T TryFindParent<T>(this DependencyObject child)
			where T : DependencyObject
		{
			//get parent item
			DependencyObject parentObject = GetParentObject(child);

			//we've reached the end of the tree
			if (parentObject == null) return null;

			//check if the parent matches the type we're looking for
			T parent = parentObject as T;
			if (parent != null)
			{
				return parent;
			}
			else
			{
				//use recursion to proceed with next level
				return TryFindParent<T>(parentObject);
			}
		}

		/// <summary>
		/// This method is an alternative to WPF's
		/// <see cref="VisualTreeHelper.GetParent"/> method, which also
		/// supports content elements. Keep in mind that for content element,
		/// this method falls back to the logical tree of the element!
		/// </summary>
		/// <param name="child">The item to be processed.</param>
		/// <returns>The submitted item's parent, if available. Otherwise
		/// null.</returns>
		public static DependencyObject GetParentObject(this DependencyObject child)
		{
			if (child == null) return null;

			//handle content elements separately
			ContentElement contentElement = child as ContentElement;
			if (contentElement != null)
			{
				DependencyObject parent = ContentOperations.GetParent(contentElement);
				if (parent != null) return parent;

				FrameworkContentElement fce = contentElement as FrameworkContentElement;
				return fce != null ? fce.Parent : null;
			}

			//also try searching for parent in framework elements (such as DockPanel, etc)
			FrameworkElement frameworkElement = child as FrameworkElement;
			if (frameworkElement != null)
			{
				DependencyObject parent = frameworkElement.Parent;
				if (parent != null) return parent;
			}

			//if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
			return VisualTreeHelper.GetParent(child);
		}
	}

	public class Win32Helper
	{
		[DllImport("User32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("User32.dll")]
		private static extern bool IsIconic(IntPtr hWnd);

		[DllImport("User32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		private const int SW_RESTORE = 9;

		public static void ActivateFirstAppRun()
		{
			Process currentProcess = Process.GetCurrentProcess();
			Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
			foreach (Process process in processes)
			{
				if (process.Id != currentProcess.Id)
				{
					IntPtr hWnd = process.MainWindowHandle;
					if (hWnd != IntPtr.Zero)
					{
						if (IsIconic(hWnd))
							ShowWindow(hWnd, SW_RESTORE);
						else
							SetForegroundWindow(hWnd);
					}
				}
			}
		}
	}
}