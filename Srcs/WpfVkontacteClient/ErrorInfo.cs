using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfVkontacteClient
{
	/// <summary>
	/// VKontacte error info
	/// </summary>
	public class VKErrorInfo
	{
		public VKErrorInfo()
		{
			this.ErrorParams = null;
			this.ErrorMessage = string.Empty;
			this.ErrorCode = -1;
		}

		public VKErrorInfo(int code, string message, List<VKErrorParams> parameters)
		{
			this.ErrorParams = parameters;
			this.ErrorMessage = message;
			this.ErrorCode = code;

			LogModule.LoggingModule.Instance.WriteMessage(LogModule.LoggingModule.Severity.Error, code.ToString(), message);
		}

		public int ErrorCode
		{
			get;
			set;
		}

		public string ErrorMessage
		{
			get;
			set;
		}

		public List<VKErrorParams> ErrorParams
		{
			get;
			set;
		}
	}

	public class VKErrorParams
	{
		public string Key
		{
			get;
			set;
		}
		public string Value
		{
			get;
			set;
		}
	}
}
