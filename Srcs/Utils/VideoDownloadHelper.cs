using System.Text.RegularExpressions;

namespace Utils
{
	public static class VideoDownloadHelper
	{
		/// <summary>
		/// Find first text in string with specified parameters
		/// </summary>
		/// <param name="text"></param>
		/// <param name="strStart"></param>
		/// <param name="strEnd"></param>
		/// <param name="lazy"></param>
		/// <returns></returns>
		public static string FindFirst(string text, string strStart, string strEnd, bool lazy)
		{
			string text2 = lazy ? "?" : "";
			string pattern = string.Concat(new string[]
									{
										"(?<=(", strStart,
										")).*",  text2,
										"(?=",  strEnd,
										")"
									});
			string result;
			try
			{
				Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
				result = regex.Match(text).Groups[0].Value;
			}
			catch
			{
				result = "";
			}
			return result;
		}

		public static string FindFirst(string text, string strStart, string strEnd)
		{
			return FindFirst(text, strStart, strEnd, true);
		}

		public static string GetVideo(string url)
		{
			HttpRequests httpRequests = new HttpRequests();
			httpRequests.GetRequest(url);
			string text = httpRequests.TextFromResponse();
			string text2 = FindFirst(text, "var video_host = '", "';");
			if (text2.IndexOf(".vkontakte.ru/") > 0)
			{
				return GetVideoVkontakte(text);
			}
			if (text2.IndexOf(".vkadre.ru") > 0)
			{
				return GetVideoVkadre(text);
			}
			return GetVideoVkadre(text);
		}

		private static string GetVideoVkontakte(string text)
		{
			string text2 = FindFirst(text, "var video_host = '", "';");
			string text3 = FindFirst(text, "var video_uid = '", "';");
			string text4 = FindFirst(text, "var video_vtag = '", "';");
			string a = FindFirst(text, "var video_no_flv = ", ";");
			string text5 = FindFirst(text, "var video_max_hd = '", "';");
			string text6 = (a == "0") ? ".flv" : ".mp4";
			string text7 = "";
			string a2;
			if (text6 == ".mp4" && (a2 = text5) != null)
			{
				if (!(a2 == "0"))
				{
					if (!(a2 == "1"))
					{
						if (!(a2 == "2"))
						{
							if (a2 == "3")
							{
								text7 = ".720";
							}
						}
						else
						{
							text7 = ".480";
						}
					}
					else
					{
						text7 = ".360";
					}
				}
				else
				{
					text7 = ".240";
				}
			}
			return string.Concat(new string[]
			{
				text2,
				"u",
				text3,
				"/video/",
				text4,
				text7,
				text6
			});
		}

		private static string GetVideoVkadre(string text)
		{
			string text2 = FindFirst(text, "var video_host = '", "';");
			string text3 = FindFirst(text, "var video_vtag = '", "';");
			string text4 = FindFirst(text, "vkid=", "&");
			return string.Concat(new string[]
			{
				"http://",
				text2,
				"/assets/videos/",
				text3,
				text4,
				".vk.flv"
			});
		}
	}
}