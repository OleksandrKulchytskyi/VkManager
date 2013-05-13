namespace Utils
{
	public static class AmountUtils
	{
		public static double ConvertBytesToMegabytes(long bytes)
		{
			return (bytes / 1024f) / 1024f;
		}

		public static long ConvertBytesToKilobytes(long bytes)
		{
			return (bytes / 1024);
		}

		public static double ConvertKilobytesToMegabytes(long kilobytes)
		{
			return kilobytes / 1024f;
		}
	}
}