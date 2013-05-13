using System;
using System.IO;
using System.Text;

namespace LogModule
{
	public class LoggingModule
	{
		/// <summary>
		/// Log severity type
		/// </summary>
		public enum Severity
		{
			Information = 0,
			Warning = 1,
			Error,
			Fatal
		}
		
		private readonly string logFileName = "WpfVkLog.dat";
		private static object m_lock = new object();

		private static LoggingModule m_instance;

		public static readonly string MainDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

		public string FileName
		{
			get;
			private set;
		}

		public Severity LoggingLevel
		{
			get;
			set;
		}

		public LoggingModule()
		{
			FileName = Path.Combine(MainDirectory, logFileName);
			this.CheckIfExistAndCreate(FileName);
			this.CheckSizeAndClear();
			this.LoggingLevel = Severity.Error;
		}

		public LoggingModule(string fileName, bool isDelete)
		{
			if (!string.IsNullOrEmpty(fileName))
			{
				this.FileName = fileName;
				if (File.Exists(this.FileName) && isDelete)
				{
					FileInfo fi = new FileInfo(this.FileName);
					fi.Attributes = FileAttributes.Normal;
					fi.Delete();
				}
			}
			else
			{
				FileName = logFileName;
				CheckIfExistAndCreate(FileName);
				this.CheckSizeAndClear();
			}
		}

		/// <summary>
		/// LoggingModule instance
		/// </summary>
		public static LoggingModule Instance
		{
			get
			{
				lock (m_lock)
				{
					if (m_instance == null)
					{
						m_instance = new LoggingModule();
					}
					System.Threading.Monitor.PulseAll(m_lock);
					return m_instance;
				}
			}
		}

		protected void CheckIfExistAndCreate(string fName)
		{
			FileInfo fi = new FileInfo(fName);
			if (!fi.Exists)
			{
				FileStream fs = fi.Create();
				if (fs != null)
				{
					byte[] data = Encoding.UTF8.GetBytes("WPF Vkontakte client LOG FILE \n\r");
					fs.Write(data, 0, data.Length);
					fs.Flush();
					fi.Attributes = FileAttributes.Normal;
					fs.Close();
					fs.Dispose();
				}
			}
			fi = null;
		}

		public event EventHandler<LogEventArgs> Logged;

		protected void OnLogged(string msg)
		{
			EventHandler<LogEventArgs> handler = System.Threading.Interlocked.CompareExchange(ref Logged, null, null);
			if (handler != null)
			{
				handler(this, new LogEventArgs(msg));
			}
		}

		public void WriteMessage(Severity type, params string[] messages)
		{
			FileStream appendTextToFile = new FileStream(this.FileName, FileMode.Append);
			StreamWriter swLog = new StreamWriter(appendTextToFile, Encoding.UTF8);
			swLog.WriteLine(string.Format("{0} {1} ==> {2} ", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(), type));

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < messages.Length; i++)
			{
				sb.AppendLine(messages[i]);
			}

			sb.AppendLine("---------------------------------------------");
			swLog.WriteLine(sb.ToString());

			if (swLog != null)
			{
				swLog.Close();
				swLog.Dispose();
			}

			if (appendTextToFile != null)
			{
				appendTextToFile.Close();
				appendTextToFile.Dispose();
			}

			OnLogged(sb.ToString());
			sb.Clear();
		}

		protected void CheckSizeAndClear()
		{
			FileInfo fi = new FileInfo(this.FileName);
			if (fi.Exists && (((double)(fi.Length / 1024 / 1024)) > 10.0))
			{
				FileStream fs = fi.OpenWrite();
				fs.SetLength(35);
				fs.Flush();
				fs.Close();
				fs.Dispose();
			}
			fi = null;
		}
	}
}
