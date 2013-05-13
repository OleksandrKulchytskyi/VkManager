using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Utils
{
	public class HttpRequests
	{
		protected HttpWebResponse HttpWebResponse;
		protected string[] SCookies;
		protected CookieContainer Cookie;
		public bool AutoRedirect;

		public HttpRequests()
		{
			this.Cookie = new CookieContainer();
		}

		private void GetCookiesFromResponse()
		{
			try
			{
				if (!string.IsNullOrEmpty(this.HttpWebResponse.Headers["Set-Cookie"]))
				{
					this.SCookies = this.HttpWebResponse.Headers["Set-Cookie"].Split(new char[]
					{
						','
					});
					string[] sCookies = this.SCookies;
					for (int i = 0; i < sCookies.Length; i++)
					{
						string text = sCookies[i];
						string[] array = text.Split(new char[]
						{
							';'
						});
						if (array.Length >= 2)
						{
							this.Cookie.Add(new Cookie(array[0].Split(new char[]
							{
								'='
							})[0], array[0].Split(new char[]
							{
								'='
							})[1], array[1].Split(new char[]
							{
								'='
							})[1], (array.Length > 2) ? array[2].Split(new char[]
							{
								'='
							})[1] : ""));
						}
					}
				}
			}
			catch
			{
			}
		}

		public bool GetRequest(string url, string referer)
		{
			bool result = false;
			try
			{
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
				httpWebRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US)AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.70 Safari/533.4";
				httpWebRequest.Referer = referer;
				httpWebRequest.Headers.Add("Cache-Control", "max-age=0");
				httpWebRequest.Accept = "application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5";
				httpWebRequest.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
				httpWebRequest.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
				httpWebRequest.Headers.Add("Accept-Charset", "windows-1251,utf-8;q=0.7,*;q=0.3");
				httpWebRequest.AllowAutoRedirect = this.AutoRedirect;
				httpWebRequest.CookieContainer = this.Cookie;
				this.HttpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				this.GetCookiesFromResponse();
				result = true;
			}
			catch
			{
				result = false;
				return result;
			}
			return result;
		}

		public bool GetRequest(string url)
		{
			return this.GetRequest(url, "");
		}

		public bool PostRequest(string url, string referer, string sQueryString, Encoding encode)
		{
			bool result = false;
			try
			{
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
				httpWebRequest.Method = "POST";
				httpWebRequest.AllowAutoRedirect = this.AutoRedirect;
				httpWebRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US)AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.70 Safari/533.4";
				httpWebRequest.Referer = referer;
				httpWebRequest.Headers.Add("Cache-Control", "max-age=0");
				httpWebRequest.ContentType = "application/x-www-form-urlencoded";
				httpWebRequest.Accept = "application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5";
				httpWebRequest.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
				httpWebRequest.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
				httpWebRequest.Headers.Add("Accept-Charset", "windows-1251,utf-8;q=0.7,*;q=0.3");
				httpWebRequest.CookieContainer = this.Cookie;
				sQueryString = sQueryString.Replace("+", "%2B");
				byte[] bytes = encode.GetBytes(sQueryString);
				httpWebRequest.ContentLength = (long)bytes.Length;
				httpWebRequest.GetRequestStream().Write(bytes, 0, bytes.Length);
				this.HttpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				this.GetCookiesFromResponse();
				result = true;
			}
			catch
			{
				result = false;
				return result;
			}
			return result;
		}

		public bool PostRequest(string url, string referer, string sQueryString)
		{
			return this.PostRequest(url, referer, sQueryString, Encoding.Default);
		}

		public string TextFromResponse(Encoding encode)
		{
			Stream stream = this.HttpWebResponse.GetResponseStream();
			if (this.HttpWebResponse.ContentEncoding.ToLower().Contains("gzip"))
			{
				stream = new GZipStream(stream, CompressionMode.Decompress);
			}
			else
			{
				if (this.HttpWebResponse.ContentEncoding.ToLower().Contains("deflate"))
				{
					stream = new DeflateStream(stream, CompressionMode.Decompress);
				}
			}
			if (stream != null)
			{
				StreamReader streamReader = new StreamReader(stream, encode);
				return streamReader.ReadToEnd();
			}
			return null;
		}

		public string TextFromResponse()
		{
			return this.TextFromResponse(Encoding.Default);
		}

		public void Free()
		{
			if (this.HttpWebResponse != null)
			{
				this.HttpWebResponse.Close();
			}
		}
	}
}