using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Utils
{
	public static class XmlUtils
	{
		/// <summary>
		/// Formatting an XML string (useful for Silverlight)
		/// </summary>
		/// <param name="stringXml">input XML string</param>
		/// <returns></returns>
		public static string FormatXml(string stringXml)
		{
			StringReader stringReader = new StringReader(stringXml);
			//XDocument represents an XML Document, allowing in memory
			// processing of it (primarily via Linq).
			XDocument xDoc = XDocument.Load(stringReader);
			var stringBuilder = new StringBuilder();
			XmlWriter xmlWriter = null;
			try
			{
				var settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.ConformanceLevel = ConformanceLevel.Auto;
				settings.IndentChars = " ";
				settings.OmitXmlDeclaration = true;
				//The XDocument can write to an XmlWriter.
				//The writer formats the well-formed xml.
				//If not well-formed no formating will occur.
				xmlWriter = XmlWriter.Create(stringBuilder, settings);
				xDoc.WriteTo(xmlWriter);
			}
			finally
			{
				if (xmlWriter != null)
					xmlWriter.Close();
			}
			return stringBuilder.ToString();
		}
	}
}