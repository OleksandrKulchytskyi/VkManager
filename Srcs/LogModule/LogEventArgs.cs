using System;

namespace LogModule
{
	public class LogEventArgs : EventArgs
	{
		public string Message
		{
			get;
			set;
		}

		public LogEventArgs(string msg)
			: base()
		{
			this.Message = msg;
		}
	}
}
