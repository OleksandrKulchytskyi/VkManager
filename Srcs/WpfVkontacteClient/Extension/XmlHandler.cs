using System.Collections.Generic;
using System.Xml;
using WpfVkontacteClient.Entities;

namespace WpfVkontacteClient.Extensions
{
	/// <summary>
	/// saves /loads media items from xml files
	/// </summary>
	public class XmlHandler
	{
		#region public methods

		/// <summary>
		/// saves media items to an xml file
		/// </summary>
		public static bool WriteXmlFile(string filename, List<UserAudio> mediaItems)
		{
			bool result = false;
			using (XmlTextWriter writer = new XmlTextWriter(@filename, null))
			{
				//Write the root element
				writer.WriteStartElement("UserAudios");
				//write sub elements
				foreach (UserAudio mi in mediaItems)
				{
					writer.WriteStartElement("UserAudio");
					writer.WriteElementString("ItemUri", @mi.Url.ToString());
					writer.WriteElementString("AudioId", mi.AudioId.ToString());
					writer.WriteEndElement();
				}
				// end the root element
				writer.WriteEndElement();
				result = true; ;
			}
			return result;
		}

		/// <summary>
		/// loads media items from an xml file
		/// </summary>
		public static List<UserAudio> ReadXmlFile(string filename)
		{
			List<UserAudio> mediaItemsRead = new List<UserAudio>();
			XmlDocument xd = new XmlDocument();
			xd.Load(@filename);
			XmlNodeList xNodes = xd.SelectNodes("/UserAudios/UserAudio");
			foreach (XmlNode xn in xNodes)
			{
				XmlNode xp = xn.SelectSingleNode("ItemUri");
				string path = xp.InnerText;
				XmlNode xr = xn.SelectSingleNode("AudioId");
				string rating = xr.InnerText;
				mediaItemsRead.Add(new UserAudio(xp.InnerText, long.Parse(xr.InnerText)));
			}
			return mediaItemsRead;
		}

		#endregion public methods
	}
}