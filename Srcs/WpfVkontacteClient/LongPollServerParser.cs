using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WpfVkontacteClient
{
	public class LongPollServerParser
	{

		public static string ParseServerResponse(string response)
		{
			JObject obj = JObject.Parse(response);
			bool isFirst = true;
			var tokenList = (IEnumerable<JToken>)(obj["updates"]);
			if (tokenList.Count() == 0)
				return string.Empty;

			var sb = new StringBuilder();
			foreach (var e in tokenList)
			{
				if (isFirst)
				{
					sb.Append(e.ToString());
					isFirst = false;
					continue;
				}
				sb.Append(string.Format(";{0}", e.ToString()));
			}
			
			return sb.ToString();
		}

		public static string GetNewTsValue(string response)
		{
			if (response.IndexOf("ts") < 0)
				return string.Empty;

			string tsdata = response.Replace('{',' ').Split(',')[0].Split(':')[1].Trim();
			return tsdata;
		}

		public static bool IsLongPoolServerResponseContentEmpty(string response)
		{
			if (LongPoolServerContainsError(response))
				return true;
			string updatesString = response.Replace('{', ' ').Replace('}', ' ').Trim().Substring(response.IndexOf("updates"));
			string data = updatesString.Split(':')[1];
			return data.Length < 3;
		}

		public static bool LongPoolServerContainsError(string response)
		{
			return (response.IndexOf("failed") > 0);
		}
	}
}
