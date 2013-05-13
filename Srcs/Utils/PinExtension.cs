using System.Collections.Generic;
using System.Linq;

namespace Utils
{
	public static class PinExtension
	{
		public static T GetNextItem<T>(this IEnumerable<T> source, T current)
		{
			T prevItem = current;
			for (int i = 0; i < source.ToList().Count; i++)
			{
				if (current.Equals(source.ToList()[i]) && i != source.ToList().Count - 1)
				{
					prevItem = source.ToList()[i - 1];
					break;
				}
			}
			return prevItem;
		}

		public static T GetPreviousItem<T>(this IEnumerable<T> source, T current)
		{
			T prevItem = current;
			for (int i = 0; i < source.ToList().Count; i++)
			{
				if (current.Equals(source.ToList()[i]) && i > 0)
				{
					prevItem = source.ToList()[i - 1];
					break;
				}
			}
			return prevItem;
		}

		public static int GetPreviousIndex<T>(this IEnumerable<T> source, T current)
		{
			int index = 0;
			for (int i = 0; i < source.ToList().Count; i++)
			{
				if (current.Equals(source.ToList()[i]) && i > 0)
				{
					index = i;
					break;
				}
			}
			return index;
		}
	}
}