using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Utils
{
	public enum ParseMode
	{
		WithArray = 0,
		WhithoutArray = 1
	}

	public class JsonParser
	{
		private string cleaned;
		private Dictionary<string, string> data;

		public JsonParser(string input)
		{
			data = new Dictionary<string, string>();
			cleaned = string.Empty;
			if (!string.IsNullOrEmpty(input))
			{
				cleaned = input.Replace("{", "").Replace("}", "").Trim();
			}
		}

		public Dictionary<string, string> Parse(ParseMode mode)
		{
			switch (mode)
			{
				case ParseMode.WhithoutArray:
					if (data.Count > 0)
						data.Clear();
					string[] arr = cleaned.Split(',');
					foreach (string item in arr)
					{
						string[] tmp = item.Split(':');
						data.Add(tmp[0].Trim().Replace("\"", ""), tmp[1].Replace("\"", "").Trim());
					}
					break;

				case ParseMode.WithArray:
					if (data.Count > 0)
						data.Clear();
					arr = cleaned.Split(',');
					for (int i = 0; i < arr.Length; i++)
					{
						string[] tmp = arr[i].Split(':');
						data.Add(tmp[0].Trim(), tmp[1].Trim());

						if (i == 0)
							break;
					}

					string upd = cleaned.Substring(cleaned.IndexOf("updates:")).Remove(0, 8).Trim();
					upd = upd.Remove(upd.Length - 1, 1).Remove(0, 1);

					Regex oRegex = new Regex(@"(?<data>[\w*|\d+])");

					MatchCollection oMatchCollection = oRegex.Matches(upd);
					List<string> list = new List<string>();
					foreach (Match oMatch in oMatchCollection)
					{
						list.Add(oMatch.Groups["data"].Value);
					}

					break;

				default:
					break;
			}
			return data;
		}
	}
}