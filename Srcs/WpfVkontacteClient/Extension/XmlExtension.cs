using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace WpfVkontacteClient
{
	public static class XmlDocumentExtensions
	{
		public static XmlDocument ToXmlDocument(this XDocument xDocument)
		{
			if (xDocument != null)
			{
				XmlDocument xmlDocument = new XmlDocument();
				using (var xmlReader = xDocument.CreateReader())
				{
					xmlDocument.Load(xmlReader);
				}
				return xmlDocument;
			}
			else
				return null;
		}

		public static XDocument ToXDocument(this XmlDocument xmlDocument)
		{
			using (XmlNodeReader nodeReader = new XmlNodeReader(xmlDocument))
			{
				nodeReader.MoveToContent();
				return XDocument.Load(nodeReader);
			}
		}

		public static Dictionary<string, XmlNode> GetFileDictionary(XmlDocument xml)
		{
			Dictionary<string, XmlNode> result = new Dictionary<string, XmlNode>();
			//перебираем все элементы File
			foreach (XmlNode file in xml.SelectNodes("//File"))
			{
				//получаем родительский элемент Dir
				XmlNode dir = file.ParentNode;
				//получаем имя директории
				string fullDirName = dir.Attributes["dname"].Value;
				//разбиваем имя директории на две чати, одна - до первого слеша, другая - после
				string[] parts = fullDirName.Split(new char[] { '\\' }, 2);
				//берем вторую часть и добавляем имя файла, получаем относительный путь файла
				string filePath = parts[1] + file.InnerText;
				//заносим путь файла в дикшинари в качестве ключа, а XML нод в качестве значения
				result.Add(filePath, file);
			}

			return result;
		}
	}
}