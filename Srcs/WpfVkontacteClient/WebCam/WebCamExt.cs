using System;
using System.Collections.Generic;

namespace WpfVkontacteClient.WebCam
{
	using Microsoft.Win32;
	using System.Collections;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Windows;

	public delegate void WebCameraFrameHandler(object sender, WebCameraEventArgs e);

	public class WebCameraEventArgs : EventArgs
	{
		private System.Drawing.Bitmap frame;

		public WebCameraEventArgs(System.Drawing.Bitmap frame)
			: base()
		{
			this.frame = frame;
		}

		public System.Drawing.Bitmap Frame
		{
			get { return frame; }
		}
	}

	public class DeviceInfo
	{
		public string Name { get; set; }

		public string Version { get; set; }

		public int Index { get; set; }

		public DeviceInfo(int index)
		{
			this.Name = string.Empty;
			this.Version = string.Empty;
			this.Index = index;
		}

		public DeviceInfo(string name, string version, int index)
		{
			this.Name = name;
			this.Version = version;
			this.Index = index;
		}
	}

	public class ImageGrabberEventArgs : EventArgs
	{
		// Methods
		public ImageGrabberEventArgs() :
			base()
		{
		}

		// Properties
		public System.Drawing.Image DeviceImage
		{
			get;
			set;
		}
	}

	public class ImageGrabber
	{
		// Fields
		private readonly int m_hwndParent;

		private readonly ImageGrabberEventArgs args = new ImageGrabberEventArgs();
		private bool bStopped = true;
		private ImageHandler ImageCaptured;
		private const int m_Height = 240;
		private int m_TimeToCapture_milliseconds = 50;
		private const int m_Width = 320;
		private int mCapHwnd;
		private System.Drawing.Image tempImg;
		private IDataObject tempObj;
		private readonly System.Windows.Forms.Timer tmrRefresh;
		public const int WM_CAP_CONNECT = 0x40a;
		public const int WM_CAP_COPY = 0x41e;
		public const int WM_CAP_DISCONNECT = 0x40b;
		public const int WM_CAP_DLG_VIDEOCOMPRESSION = 0x42e;
		public const int WM_CAP_DLG_VIDEODISPLAY = 0x42b;
		public const int WM_CAP_DLG_VIDEOFORMAT = 0x429;
		public const int WM_CAP_DLG_VIDEOSOURCE = 0x42a;
		public const int WM_CAP_GET_FRAME = 0x43c;
		public const int WM_CAP_GET_VIDEOFORMAT = 0x42c;
		public const int WM_CAP_SET_PREVIEW = 0x432;
		public const int WM_CAP_SET_VIDEOFORMAT = 0x42d;
		public const int WM_CAP_START = 0x400;
		public const int WM_USER = 0x400;

		// Events
		public event ImageHandler ImageCaptured2
		{
			add
			{
				ImageHandler handler2;
				ImageHandler imageCaptured = this.ImageCaptured;
				do
				{
					handler2 = imageCaptured;
					ImageHandler handler3 = (ImageHandler)Delegate.Combine(handler2, value);
					imageCaptured = Interlocked.CompareExchange<ImageHandler>(ref this.ImageCaptured, handler3, handler2);
				}
				while (imageCaptured != handler2);
			}
			remove
			{
				ImageHandler handler2;
				ImageHandler imageCaptured = this.ImageCaptured;
				do
				{
					handler2 = imageCaptured;
					ImageHandler handler3 = (ImageHandler)Delegate.Remove(handler2, value);
					imageCaptured = Interlocked.CompareExchange<ImageHandler>(ref this.ImageCaptured, handler3, handler2);
				}
				while (imageCaptured != handler2);
			}
		}

		// Methods
		public ImageGrabber(int hwndParent)
		{
			this.m_hwndParent = hwndParent;
			this.tmrRefresh = new System.Windows.Forms.Timer();
			this.tmrRefresh.Interval = 100;
			this.tmrRefresh.Tick += new EventHandler(this.tmrRefresh_Tick);
		}

		[DllImport("avicap32.dll")]
		public static extern int capCreateCaptureWindowA(string lpszWindowName, int dwStyle, int X, int Y, int nWidth, int nHeight, int hwndParent, int nID);

		[DllImport("user32")]
		public static extern int CloseClipboard();

		[DllImport("user32")]
		public static extern int EmptyClipboard();

		~ImageGrabber()
		{
			this.Stop();
		}

		[DllImport("user32")]
		public static extern int OpenClipboard(int hWnd);

		[DllImport("user32")]
		public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

		public void Start()
		{
			try
			{
				this.Stop();
				this.mCapHwnd = capCreateCaptureWindowA("WebCap", 0, 0, 0, 320, 240, this.m_hwndParent, 0);
				System.Windows.Forms.Application.DoEvents();
				SendMessage(this.mCapHwnd, 0x40a, 0, 0);
				SendMessage(this.mCapHwnd, 0x432, 0, 0);
				this.tmrRefresh.Interval = this.m_TimeToCapture_milliseconds;
				this.bStopped = false;
				this.tmrRefresh.Start();
			}
			catch (Exception exception)
			{
				MessageBox.Show("An error ocurred while starting the video capture. Check that your webcamera is connected properly and turned on.\r\n\n" + exception.Message);
				this.Stop();
			}
		}

		public void Stop()
		{
			try
			{
				this.bStopped = true;
				this.tmrRefresh.Stop();
				System.Windows.Forms.Application.DoEvents();
				SendMessage(this.mCapHwnd, 0x40b, 0, 0);
			}
			catch (Exception)
			{
			}
		}

		private void tmrRefresh_Tick(object sender, EventArgs e)
		{
			try
			{
				this.tmrRefresh.Stop();
				SendMessage(this.mCapHwnd, 0x43c, 0, 0);
				SendMessage(this.mCapHwnd, 0x41e, 0, 0);
				if (this.ImageCaptured != null)
				{
					this.tempObj = Clipboard.GetDataObject();
					if (this.tempObj != null)
					{
						this.tempImg = (System.Drawing.Bitmap)this.tempObj.GetData(DataFormats.Bitmap);
					}
					GC.Collect();
					if (this.tempImg == null)
					{
						throw new Exception("");
					}
					this.args.DeviceImage = this.tempImg.GetThumbnailImage(320, 240, null, IntPtr.Zero);
					this.ImageCaptured(this, this.args);
				}
				System.Windows.Forms.Application.DoEvents();
				if (!this.bStopped)
				{
					this.tmrRefresh.Start();
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show("An error ocurred while capturing the video image. The video capture will now be terminated.\r\n\n" + exception.Message);
				this.Stop();
			}
		}

		// Properties
		public int Milliseconds
		{
			get
			{
				return this.m_TimeToCapture_milliseconds;
			}
			set
			{
				this.m_TimeToCapture_milliseconds = value;
			}
		}

		// Nested Types
		public delegate void ImageHandler(object source, ImageGrabberEventArgs e);
	}

	public class WebCamGrabber : IDisposable
	{
		#region Native API

		// Camera API
		private const int WM_CAP_START = 1024; // WM_USER

		private const int WM_CAP_SET_CALLBACK_FRAME = WM_CAP_START + 5;
		private const int WM_CAP_DRIVER_CONNECT = WM_CAP_START + 10;
		private const int WM_CAP_DRIVER_DISCONNECT = WM_CAP_START + 11;
		private const int WM_CAP_DLG_VIDEODISPLAY = WM_CAP_START + 42;
		private const int WM_CAP_SET_VIDEOFORMAT = WM_CAP_START + 45;
		private const int WM_CAP_SET_PREVIEW = WM_CAP_START + 50;
		private const int WM_CAP_SET_PREVIEWRATE = WM_CAP_START + 52;
		private const int WM_CAP_GRAB_FRAME = WM_CAP_START + 60;
		private const int WM_CAP_GRAB_FRAME_NOSTOP = WM_CAP_START + 61;

		[DllImport("avicap32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		protected static extern bool capGetDriverDescriptionA(short wDriverIndex, [MarshalAs(UnmanagedType.VBByRefStr)] ref String lpszName,
			int cbName, [MarshalAs(UnmanagedType.VBByRefStr)] ref String lpszVer, int cbVer);

		[DllImport("avicap32.dll", EntryPoint = "capCreateCaptureWindow")]
		[return: MarshalAs(UnmanagedType.SysInt)]
		private static extern int capCreateCaptureWindow(string lpszWindowName, int dwStyle, int X, int Y,
			int nWidth, int nHeight, IntPtr hwndParent, int nID);

		[DllImport("user32", EntryPoint = "SendMessage")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SendMessage(int hWnd, uint wMsg, int wParam, int lParam);

		[DllImport("user32", EntryPoint = "SendMessage")]
		[return: MarshalAs(UnmanagedType.SysInt)]
		private static extern int SendBitmapMessage(int hWnd, uint wMsg, int wParam, ref BITMAPINFO lParam);

		[DllImport("user32", EntryPoint = "SendMessage")]
		[return: MarshalAs(UnmanagedType.SysInt)]
		private static extern int SendHeaderMessage(int hWnd, uint wMsg, int wParam, CallBackDelegate lParam);

		//This function enable destroy the window child
		[DllImport("user32")]
		protected static extern bool DestroyWindow(int hwnd);

		[StructLayout(LayoutKind.Sequential)]
		public struct VIDEOHEADER
		{
			public IntPtr lpData;
			public uint dwBufferLength;
			public uint dwBytesUsed;
			public uint dwTimeCaptured;
			public uint dwUser;
			public uint dwFlags;

			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.SafeArray)]
			private byte[] dwReserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFOHEADER
		{
			public uint biSize;
			public int biWidth;
			public int biHeight;
			public ushort biPlanes;
			public ushort biBitCount;
			public uint biCompression;
			public uint biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public uint biClrUsed;
			public uint biClrImportant;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFO
		{
			public BITMAPINFOHEADER bmiHeader;
			public int bmiColors;
		}

		#endregion Native API

		private delegate void CallBackDelegate(IntPtr hwnd, ref VIDEOHEADER hdr);

		private CallBackDelegate delegateFrameCallBack;

		public event WebCameraFrameHandler OnCameraFrame;

		private AutoResetEvent autoEvent = new AutoResetEvent(false);
		private Thread frameThread = null;

		private bool bStart = false;
		private bool isStoped = false;
		private IntPtr m_hWnd = IntPtr.Zero;
		private int camHwnd, parentHwnd;
		private int preferredFPSms, camID;
		private int frameWidth, frameHeight;

		public int PreferredFPS
		{
			get { return 1000 / preferredFPSms; }
			set
			{
				if (value == 0)
					preferredFPSms = 0;
				else if (value > 0 && value <= 30)
				{
					preferredFPSms = 1000 / value;
				}
			}
		}

		public int ID
		{
			get { return camID; }
		}

		public int FrameHeight
		{
			get { return frameHeight; }
		}

		public int FrameWidth
		{
			get { return frameWidth; }
		}

		public List<DeviceInfo> Devices
		{
			get;
			private set;
		}

		public WebCamGrabber(int frameWidth, int frameHeight, int preferredFPS, int camID, IntPtr parentHwnd)
		{
			Devices = new List<DeviceInfo>();
			GetAllCapturesDevices();

			this.frameWidth = frameWidth;
			this.frameHeight = frameHeight;
			this.m_hWnd = parentHwnd;
			this.camID = camID;
			PreferredFPS = preferredFPS;

			delegateFrameCallBack = FrameCallBack;
		}

		private void FrameCallBack(IntPtr hwnd, ref VIDEOHEADER hdr)
		{
			WebCameraFrameHandler handler = OnCameraFrame;

			if (handler != null)
			{
				System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(frameWidth, frameHeight,
					3 * frameWidth, System.Drawing.Imaging.PixelFormat.Format24bppRgb, hdr.lpData);
				OnCameraFrame(this, new WebCameraEventArgs(bmp));
			}

			// block thread for preferred milleseconds
			if (preferredFPSms == 0)
				autoEvent.WaitOne();
			else
				autoEvent.WaitOne(preferredFPSms, false);
		}

		private void GetAllCapturesDevices()
		{
			if (Devices != null)
				Devices.Clear();

			String dName = "".PadRight(100);
			String dVersion = "".PadRight(100);

			for (short i = 0; i < 6; i++)
			{
				if (capGetDriverDescriptionA(i, ref dName, 100, ref dVersion, 100))
				{
					DeviceInfo d = new DeviceInfo(i);
					d.Name = dName.Trim();
					d.Version = dVersion.Trim();
					Devices.Add(d);
				}
			}
		}

		public void RefreshDevices()
		{
			GetAllCapturesDevices();
		}

		/// <summary>
		/// Allow waiting worker (FrameGrabber) thread to proceed
		/// </summary>
		public void Set()
		{
			autoEvent.Set();
		}

		public void Start()
		{
			try
			{
				if (Devices.Count > 0)
					camHwnd = capCreateCaptureWindow("", 0, 0, 0, frameWidth, frameHeight, m_hWnd, camID);

				// connect to the device
				if (SendMessage(camHwnd, WM_CAP_DRIVER_CONNECT, 0, 0))
				{
					BITMAPINFO bInfo = new BITMAPINFO();
					bInfo.bmiHeader = new BITMAPINFOHEADER();
					bInfo.bmiHeader.biSize = (uint)Marshal.SizeOf(bInfo.bmiHeader);
					bInfo.bmiHeader.biWidth = frameWidth;
					bInfo.bmiHeader.biHeight = frameHeight;
					bInfo.bmiHeader.biPlanes = 1;
					bInfo.bmiHeader.biBitCount = 24; // bits per frame, 24 - RGB

					//Enable preview mode. In preview mode, frames are transferred from the
					//capture hardware to system memory and then displayed in the capture
					//window using GDI functions.
					SendMessage(camHwnd, WM_CAP_SET_PREVIEW, 1, 0);
					SendMessage(camHwnd, WM_CAP_SET_PREVIEWRATE, 34, 0); // sets the frame display rate in preview mode
					SendBitmapMessage(camHwnd, WM_CAP_SET_VIDEOFORMAT, Marshal.SizeOf(bInfo), ref bInfo);

					frameThread = new Thread(new ThreadStart(this.FrameGrabber));
					bStart = true;       // First, set variable
					frameThread.Priority = ThreadPriority.Lowest;
					frameThread.Start(); // Only then put thread to the queue
				}
				else
					throw new Exception("Cannot connect to Web camera device");
			}
			catch (Exception ex)
			{
				LogModule.LoggingModule.Instance.WriteMessage(LogModule.LoggingModule.Severity.Error, ex.Message);
				Stop();
			}
		}

		public void Stop()
		{
			if (!isStoped)
			{
				isStoped = true;
				try
				{
					bStart = false;
					Set();
					SendMessage(camHwnd, WM_CAP_DRIVER_DISCONNECT, 0, 0);
				}
				catch { }
			}
		}

		private void FrameGrabber()
		{
			while (bStart) // if worker active thread is still required
			{
				try
				{
					// get the next frame. This is the SLOWEST part of the program
					SendMessage(camHwnd, WM_CAP_GRAB_FRAME_NOSTOP, 0, 0);
					SendHeaderMessage(camHwnd, WM_CAP_SET_CALLBACK_FRAME, 0, delegateFrameCallBack);
				}
				catch (Exception)
				{
					this.Stop();
				}
			}
		}

		public void Dispose()
		{
			this.Stop();
			Devices.Clear();

			Devices = null;
			autoEvent.Close();

			if (frameThread != null && frameThread.ThreadState == ThreadState.Running)
				frameThread.Abort();

			frameThread = null;
			GC.SuppressFinalize(this);
		}
	}

	public class VideoCapture : IDisposable
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFOHEADER
		{
			[MarshalAs(UnmanagedType.I4)]
			public Int32 biSize;

			[MarshalAs(UnmanagedType.I4)]
			public Int32 biWidth;

			[MarshalAs(UnmanagedType.I4)]
			public Int32 biHeight;

			[MarshalAs(UnmanagedType.I2)]
			public short biPlanes;

			[MarshalAs(UnmanagedType.I2)]
			public short biBitCount;

			[MarshalAs(UnmanagedType.I4)]
			public Int32 biCompression;

			[MarshalAs(UnmanagedType.I4)]
			public Int32 biSizeImage;

			[MarshalAs(UnmanagedType.I4)]
			public Int32 biXPelsPerMeter;

			[MarshalAs(UnmanagedType.I4)]
			public Int32 biYPelsPerMeter;

			[MarshalAs(UnmanagedType.I4)]
			public Int32 biClrUsed;

			[MarshalAs(UnmanagedType.I4)]
			public Int32 biClrImportant;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFO
		{
			[MarshalAs(UnmanagedType.Struct, SizeConst = 40)]
			public BITMAPINFOHEADER bmiHeader;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
			public Int32[] bmiColors;
		}

		private const int WS_CHILD = 0x40000000;
		private const int WS_VISIBLE = 0x10000000;
		private const int SWP_SHOWWINDOW = 0x0040;
		private const int WM_CAP_START = 0x0400;
		private const int WM_CAP_DRIVER_CONNECT = (WM_CAP_START + 10);
		private const int WM_CAP_DRIVER_DISCONNECT = (WM_CAP_START + 11);
		private const int WM_CAP_SET_PREVIEWRATE = (WM_CAP_START + 52);
		private const int WM_CAP_SET_PREVIEW = (WM_CAP_START + 50);
		private const int WM_CAP_GRAB_FRAME = (WM_CAP_START + 60);
		private const int WM_CAP_GET_VIDEOFORMAT = (WM_CAP_START + 44);

		private IntPtr hCapture = (IntPtr)0;
		private int width = 0;
		private int height = 0;

		public bool CreateCaptureWindow(IntPtr hParent, int x, int y)
		{
			hCapture = capCreateCaptureWindow("", WS_CHILD | WS_VISIBLE, x, y, 32, 24, hParent, 0);
			if ((int)hCapture == 0) { return false; }

			SendMessage(hCapture, WM_CAP_DRIVER_CONNECT, 0, 0);
			GetVideoFormat();
			SetWindowPos(hCapture, 0, x, y, width, height, SWP_SHOWWINDOW);

			return true;
		}

		public void ReleaseCaptureWindow()
		{
			if ((int)hCapture != 0)
			{
				SendMessage(hCapture, WM_CAP_DRIVER_DISCONNECT, 0, 0);
				DestroyWindow(hCapture);
				hCapture = (IntPtr)0;
			}
		}

		public void StartPreview()
		{
			SendMessage(hCapture, WM_CAP_SET_PREVIEWRATE, 15, 0);
			SendMessage(hCapture, WM_CAP_SET_PREVIEW, 1, 0);
		}

		public void StopPreview()
		{
			SendMessage(hCapture, WM_CAP_SET_PREVIEW, 0, 0);
		}

		public void GetFrame()
		{
			SendMessage(hCapture, WM_CAP_GRAB_FRAME, 0, 0);
		}

		private void GetVideoFormat()
		{
			BITMAPINFO buffer = new BITMAPINFO();
			int size = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
			buffer.bmiHeader.biSize = size;

			SendMessage(hCapture, WM_CAP_GET_VIDEOFORMAT,
				size, ref buffer);

			width = buffer.bmiHeader.biWidth;
			height = buffer.bmiHeader.biHeight;
		}

		[DllImport("User32.dll")]
		private static extern bool SendMessage(IntPtr hWnd, int wMsg, int wParam, ref BITMAPINFO lParam);

		[DllImport("User32.dll")]
		private static extern bool SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

		[DllImport("User32.dll")]
		private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y,
												int cx, int cy, int wFlags);

		[DllImport("user32.dll")]
		private static extern int DestroyWindow(IntPtr hwnd);

		[DllImport("avicap32.dll")]
		private static extern IntPtr capCreateCaptureWindow(string lpszWindowName, int dwStyle, int x, int y, int nWidth, int nHeight,
															IntPtr hWnd, int nID);

		public void Dispose()
		{
			ReleaseCaptureWindow();
			GC.SuppressFinalize(this);
		}
	}

	public class WebCam : IDisposable
	{
		private const short WM_CAP = 0x400;
		private const int WM_CAP_DRIVER_CONNECT = 0x40a;
		private const int WM_CAP_DRIVER_DISCONNECT = 0x40b;
		private const int WM_CAP_EDIT_COPY = 0x41e;
		private const int WM_CAP_SET_PREVIEW = 0x432;
		private const int WM_CAP_SET_OVERLAY = 0x433;
		private const int WM_CAP_SET_PREVIEWRATE = 0x434;
		private const int WM_CAP_SET_SCALE = 0x435;
		private const int WS_CHILD = 0x40000000;
		private const int WS_VISIBLE = 0x10000000;
		private const short SWP_NOMOVE = 0x2;
		private short SWP_NOZORDER = 0x4;
		private short HWND_BOTTOM = 1;

		//This function enables enumerate the web cam devices
		[DllImport("avicap32.dll")]
		protected static extern bool capGetDriverDescriptionA(short wDriverIndex, [MarshalAs(UnmanagedType.VBByRefStr)]ref String lpszName,
		   int cbName, [MarshalAs(UnmanagedType.VBByRefStr)] ref String lpszVer, int cbVer);

		//This function enables create a  window child with so that you can display it in a picturebox for example
		[DllImport("avicap32.dll")]
		protected static extern int capCreateCaptureWindowA([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpszWindowName,
			int dwStyle, int x, int y, int nWidth, int nHeight, int hWndParent, int nID);

		//This function enables set changes to the size, position, and Z order of a child window
		[DllImport("user32")]
		protected static extern int SetWindowPos(int hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

		//This function enables send the specified message to a window or windows
		[DllImport("user32", EntryPoint = "SendMessageA")]
		protected static extern int SendMessage(int hwnd, int wMsg, int wParam, [MarshalAs(UnmanagedType.AsAny)] object lParam);

		//This function enable destroy the window child
		[DllImport("user32")]
		protected static extern bool DestroyWindow(int hwnd);

		// Normal device ID
		private int DeviceID = 0;

		// Handle value to preview window
		private int hHwnd = 0;

		//The devices list
		private ArrayList ListOfDevices = new ArrayList();

		//The picture to be displayed
		public System.Windows.Forms.PictureBox Container
		{
			get;
			set;
		}

		/// <summary>
		/// This function is used to load the list of the devices
		/// </summary>
		public void Load()
		{
			string Name = String.Empty.PadRight(100);
			string Version = String.Empty.PadRight(100);
			bool EndOfDeviceList = false;
			short index = 0;

			// Load name of all avialable devices into the lstDevices .
			do
			{
				// Get Driver name and version
				EndOfDeviceList = capGetDriverDescriptionA(index, ref Name, 100, ref Version, 100);
				// If there was a device add device name to the list
				if (EndOfDeviceList) ListOfDevices.Add(Name.Trim());
				index += 1;
			}
			while (!(EndOfDeviceList == false));
		}

		/// <summary>
		/// Function used to display the output from a video capture device, you need to create
		/// a capture window.
		/// </summary>
		public void OpenConnection()
		{
			string DeviceIndex = Convert.ToString(DeviceID);
			IntPtr oHandle = Container.Handle;

			if (Container == null)
				System.Windows.MessageBox.Show("You should set the container property");

			// Open Preview window in picturebox .
			// Create a child window with capCreateCaptureWindowA so you can display it in a picturebox.
			hHwnd = capCreateCaptureWindowA(ref DeviceIndex, WS_VISIBLE | WS_CHILD, 0, 0, 640, 480, oHandle.ToInt32(), 0);
			// Connect to device
			if (SendMessage(hHwnd, WM_CAP_DRIVER_CONNECT, DeviceID, 0) != 0)
			{
				// Set the preview scale
				SendMessage(hHwnd, WM_CAP_SET_SCALE, -1, 0);
				// Set the preview rate in terms of milliseconds
				SendMessage(hHwnd, WM_CAP_SET_PREVIEWRATE, 66, 0);
				// Start previewing the image from the camera
				SendMessage(hHwnd, WM_CAP_SET_PREVIEW, -1, 0);
				// Resize window to fit in picturebox
				SetWindowPos(hHwnd, HWND_BOTTOM, 0, 0, Container.Height, Container.Width, SWP_NOMOVE | SWP_NOZORDER);
			}
			else
			{
				// Error connecting to device close window
				DestroyWindow(hHwnd);
			}
		}

		/// <summary>
		/// Close windows
		/// </summary>
		private void CloseConnection()
		{
			SendMessage(hHwnd, WM_CAP_DRIVER_DISCONNECT, DeviceID, 0);
			// close window
			DestroyWindow(hHwnd);
		}

		/// <summary>
		/// Save Image
		/// </summary>
		public void SaveImage()
		{
			IDataObject data;
			System.Drawing.Image oImage;
			SaveFileDialog sfdImage = new SaveFileDialog();
			sfdImage.Filter = "(*.bmp)|*.bmp";
			// Copy image to clipboard
			SendMessage(hHwnd, WM_CAP_EDIT_COPY, 0, 0);
			// Get image from clipboard and convert it to a bitmap
			data = Clipboard.GetDataObject();
			if (data.GetDataPresent(typeof(System.Drawing.Bitmap)))
			{
				oImage = (System.Drawing.Image)data.GetData(typeof(System.Drawing.Bitmap));
				Container.Image = oImage;
				CloseConnection();
				if (sfdImage.ShowDialog() == true)
				{
					oImage.Save(sfdImage.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
				}
			}
		}

		/// <summary>
		/// This function is used to dispose the connection to the device
		/// </summary>

		#region IDisposable Members

		public void Dispose()
		{
			CloseConnection();
			GC.SuppressFinalize(this);
		}

		#endregion IDisposable Members
	}
}