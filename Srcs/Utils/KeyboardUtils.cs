using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
{
	public static class KeybordUtils
	{
		private static readonly Dictionary<char, char> En2Rus = new Dictionary<char, char>();
		private static readonly Dictionary<char, char> Rus2En = new Dictionary<char, char>();

		private static readonly char[] EnKeyboard = {
		'q','w','e','r','t','y','u','i','o','p','[',']',
		'a','s','d','f','g','h','j','k','l',';','\'',
		'z','x','c','v','b','n','m',',','.','`'};

		private static readonly char[] RusKeyboard = {
		'й','ц','у','к','е','н','г','ш','щ','з','х','ъ',
		'ф','ы','в','а','п','р','о','л','д','ж','э','я',
		'ч','с','м','и','т','ь','б','ю','ё'};

		static KeybordUtils()
		{
			for (int i = 0; i < 33; i++)
			{
				if (Rus2En.ContainsKey(RusKeyboard[i]) || En2Rus.ContainsKey(EnKeyboard[i]))
					continue;

				Rus2En.Add(RusKeyboard[i], EnKeyboard[i]);
				En2Rus.Add(EnKeyboard[i], RusKeyboard[i]);
			}
		}

		public static string KeyboardSwitch(string text)
		{
			List<int> bigIndx = new List<int>();
			//Loop text and find text indexes of upper letters
			for (int i = 0; i < text.Length; i++)
			{
				if (Char.IsUpper(text[i]))
					bigIndx.Add(i);
			}

			StringBuilder builder = new StringBuilder(text.ToLower());

			bool isRussian = builder.ToString().IndexOfAny(RusKeyboard) != -1;

			for (int i = 0; i < text.Length; i++)
			{
				if (isRussian ? Rus2En.ContainsKey(builder[i]) : En2Rus.ContainsKey(builder[i]))
				{
					builder[i] = isRussian ? Rus2En[builder[i]] : En2Rus[builder[i]];
				}
			}

			//Convert lowered text to upper text accordint to indexes
			for (int i = 0; i < builder.Length; i++)
			{
				if (bigIndx.Contains(i))
					builder[i] = Char.ToUpper(builder[i]);
			}

			return builder.ToString();
		}
	}
}