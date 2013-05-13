using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Utils
{
	public static class StringUtils
	{
		public static string EscapeXmlString(this string data)
		{
			return data.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
		}

		public static List<string> GetUserAndObjectIDFromUrl(StringCollection strCol)
		{
			List<string> retList = new List<string>();
			if (strCol != null && strCol.Count > 0)
			{
				foreach (string s in strCol)
				{
					if (string.IsNullOrEmpty(s) || s.Length < 3)
						continue;

					int ind = s.LastIndexOf('/');
					string subStr = s.Substring(ind + 6);
					if (subStr.Contains('?'))
					{
						retList.Add(subStr.Remove(subStr.IndexOf('?')));
						continue;
					}
					retList.Add(subStr);
				}
			}
			return retList;
		}
	}
}