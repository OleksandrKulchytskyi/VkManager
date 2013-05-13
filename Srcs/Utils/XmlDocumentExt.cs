using System;
using System.Xml;

namespace Utils
{
	sealed public class XMLDocumentEx : XmlDocument
	{
		public bool AddNode(XmlNode oSource, String sName)
		{
			return AddNode(oSource, sName, null);
		}

		public bool AddNode(XmlNode oSource, String sName, String sParent)
		{
			try
			{
				if (sName != null && oSource != null)
				{
					// create the new node with given name
					XmlNode oNewNode = CreateElement(sName);
					// copy the contents from the source node

					oNewNode.InnerXml = oSource.InnerXml;
					// if there is no parent node specified, then add
					// the new node as a child node of the root node

					if (sParent != null) sParent = sParent.Trim();
					if (sParent == null || sParent.Equals(String.Empty))
					{
						DocumentElement.AppendChild(oNewNode);
						return true;
					}
					// otherwise add the new node as a child of the parent node
					else
					{
						if (!sParent.Substring(0, 2).Equals("//")) sParent = "//" + sParent;
						XmlNode oParent = SelectSingleNode(sParent);
						if (oParent != null)
						{
							oParent.AppendChild(oNewNode);
							return true;
						}
					}
				}
			}
			catch (Exception)
			{
				// error handling code
			}
			return false;
		}

		public bool AddNodes(XmlNodeList oSourceList, String sName)
		{
			return AddNodes(oSourceList, sName, null);
		}

		public bool AddNodes(XmlNodeList oSourceList, String sName, String sParent)
		{
			try
			{
				if (oSourceList != null)
				{
					// call AddNode for each item in the source node list
					// return true only if all nodes are added successfully

					int i = 0;
					while (i < oSourceList.Count)
					{
						if (!AddNode(oSourceList.Item(i), sName, sParent)) return false;
						i++;
					}
					return true;
				}
			}
			catch (Exception)
			{
				// error handling code
			}
			return false;
		}

		public bool MergeNode(XmlNode oSource, String sName)
		{
			return MergeNode(oSource, sName, null);
		}

		public bool MergeNode(XmlNode oSource, String sName, String sParent)
		{
			try
			{
				if (sName != null && oSource != null)
				{
					XmlNode theNode = null;
					// if there is no parent node specified ...

					if (sParent != null) sParent = sParent.Trim();
					if (sParent == null || sParent.Equals(String.Empty))
					{
						// if the node with specified name does not exist,
						// add it as a child node of the root node

						theNode = SelectSingleNode("//" + sName);
						if (theNode == null)
						{
							theNode = CreateElement(sName);
							DocumentElement.AppendChild(theNode);
						}
					}					// if the parent node is specified ...

					else
					{
						// find the parent node
						if (!sParent.Substring(0, 2).Equals("//")) sParent = "//" + sParent;
						XmlNode theParent = SelectSingleNode(sParent);
						if (theParent != null)
						{
							// if the node with specified name does not exist, create
							// it first, then add it as a child node of the parent node

							theNode = theParent.SelectSingleNode(sName);
							if (theNode == null)
							{
								theNode = CreateElement(sName);
								theParent.AppendChild(theNode);
							}
						}
					}
					// merge the content of the source node into
					// the node with specified name

					if (theNode != null)
					{
						theNode.InnerXml += oSource.InnerXml;
						return true;
					}
				}
			}
			catch (Exception)
			{
			}
			return false;
		}
	}
}