using System.Text;

namespace Utils
{
	public static class StringBuilderExtension
	{
		public static bool IsNullOrWhiteSpace(this StringBuilder poStringBuilder)
		{
			if (poStringBuilder.IsNullOrEmpty() == false)
				for (int x = poStringBuilder.Length - 1; x >= 0; x--)
					if (char.IsWhiteSpace(poStringBuilder[x]) == false)
						return false;
			return true;
		}

		public static bool IsNullOrEmpty(this StringBuilder poStringBuilder)
		{
			return (poStringBuilder == null) || (poStringBuilder.Length == 0);
		}
	}
}