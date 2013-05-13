using System.IO;

namespace Utils
{
	/// <summary>
	/// Stream utils class
	/// </summary>
	public static class StreamUtils
	{
		/// <summary>
		/// Copy data from one stream to another
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		static public void CopyStream(Stream source, Stream destination)
		{
			byte[] buffer = new byte[32768];
			int bytesRead;
			do
			{
				bytesRead = source.Read(buffer, 0, buffer.Length);
				destination.Write(buffer, 0, bytesRead);
			}
			while (bytesRead != 0);
		}
	}
}