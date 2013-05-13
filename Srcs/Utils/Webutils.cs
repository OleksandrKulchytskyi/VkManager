using System.IO;
using System.Net;
using System.Text;

namespace Utils
{
	public static class WebUtils
	{
		public static string GetResponse(string strRequest)
		{
			string result = string.Empty;

			WebRequest request = WebRequest.Create(strRequest);
			WebResponse response = null;

			Stream stream = null;
			try
			{
				response = request.GetResponse();
				stream = response.GetResponseStream();
				byte[] data = new byte[stream.Length];
				stream.Write(data, 0, data.Length);
				result = Encoding.UTF8.GetString(data);
			}
			catch { }
			finally
			{
				if (stream != null)
					stream.Close();
				if (response != null)
					response.Close();
				if (request != null)
					request = null;
			}

			return result;
		}
	}
}