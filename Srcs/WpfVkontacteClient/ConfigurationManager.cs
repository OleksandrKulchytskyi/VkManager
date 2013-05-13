using LogModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace WpfVkontacteClient
{
	public sealed class ConfigurationManager : IDisposable
	{
		private string config, setting = null;
		private XDocument doc = null;
		private XDocument setDoc = null;
		private bool disposed = false;

		public ConfigurationManager()
		{
			config = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
								"Configuration.xml");
			setting = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
								"Settings.xml");
			if (File.Exists(config))
				doc = XDocument.Load(config);
			else
			{
				this.CreateConfig();
				doc = XDocument.Load(config);
			}

			if (File.Exists(setting))
				this.setDoc = XDocument.Load(setting);
			else
			{
				this.CreateSettings();
				setDoc = XDocument.Load(setting);
			}
		}

		private bool CreateConfig()
		{
			bool result = false;

			try
			{
				XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
												new XElement("Configuration"));
				doc.Save(config);
				result = true;
			}
			catch (Exception ex)
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Error, "Error in creation config file", ex.Message);
			}
			return result;
		}

		private bool CreateSettings()
		{
			bool result = false;

			try
			{
				XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
												new XElement("ProgramSettings",
													new XElement("ClearImageCache", "false"),
													new XElement("ImageCacheMaxSize", 10),
													new XElement("DataCacheMaxSize", 50),
													new XElement("DetermineNewMessages",1)
													));
				doc.Save(setting);
				result = true;
			}
			catch (Exception ex)
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Error, "Error in creation settings file", ex.Message);
			}

			return result;
		}

		public List<UserData> GetAllData()
		{
			if (doc != null)
				return (from c in doc.Element("Configuration").Elements("User")
						select new UserData()
						{
							AppId = long.Parse(c.Element("AppId").Value),
							UserName = c.Attribute("name").Value,
							Password = c.Element("Password").Value,
							AccessKey = c.Element("AccessKey").Value,
							Email = c.Element("Email").Value
						}).ToList();

			return null;
		}

		public UserData GetData(string userName)
		{
			if (doc != null)
			{
				UserData data = (from c in doc.Element("Configuration").Elements("User")
								 where c.Attribute("name").Value.ToLower() == userName.ToLower()
								 select new UserData()
								 {
									 AppId = long.Parse(c.Element("AppId").Value),
									 UserName = c.Attribute("name").Value,
									 Password = c.Element("Password").Value,
									 AccessKey = c.Element("AccessKey").Value,
									 Email = c.Element("Email").Value
								 }).FirstOrDefault();

				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, "Configuration manager loaded user", data.ToString());

				return data;
			}

			return null;
		}

		public bool CreateUser(UserData data)
		{
			bool result = false;
			if ((data == null) || (doc == null))
				return false;
			try
			{
				XElement newUser = new XElement("User",
												new XAttribute("name", data.UserName),
												new XElement("AppId", data.AppId),
												new XElement("AccessKey", data.AccessKey),
												new XElement("Email", data.Email),
												new XElement("Password", data.Password)
												);

				XElement main = doc.Element("Configuration");
				main.Add(newUser);
				doc.Save(config);
				result = true;

				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, data.ToString());
			}
			catch { }
			return result;
		}

		public bool RemoveData(UserData data)
		{
			bool result = false;
			if ((data == null) || (doc == null))
				return false;

			if (doc != null)
			{
				XElement element = (from c in doc.Element("Configuration").Elements("User")
									where c.Attribute("name").Value.ToLower() == data.UserName.ToLower()
									select c).FirstOrDefault();
				element.Remove();
				doc.Save(config);
				result = true;
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Warning, data.ToString());
			}

			return result;
		}

		public ProgramData GetProgramSettings()
		{
			if (setDoc != null)
			{
				ProgramData data = new ProgramData();
				data.ImageCacheMaxSize = double.Parse(setDoc.Element("ProgramSettings").Element("ImageCacheMaxSize").Value);
				data.IsClearImageCache = bool.Parse(setDoc.Element("ProgramSettings").Element("ClearImageCache").Value);
				data.DataCacheMaxSize = double.Parse(setDoc.Element("ProgramSettings").Element("DataCacheMaxSize").Value);
				data.DetermineNewMessages = setDoc.Element("ProgramSettings").Element("DetermineNewMessages").Value == "1" ? true : false;
				
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, "Configuration manager program settings", data.ToString());

				return data;
			}
			return null;
		}

		public bool SaveProgramSettings(ProgramData data)
		{
			bool result = false;
			try
			{
				if (setDoc != null && data != null)
				{
					XElement sets = setDoc.Element("ProgramSettings");
					sets.Element("ImageCacheMaxSize").Value = data.ImageCacheMaxSize.ToString();
					sets.Element("ClearImageCache").Value = data.IsClearImageCache.ToString();
					sets.Element("DataCacheMaxSize").Value = data.DataCacheMaxSize.ToString();
					sets.Element("DetermineNewMessages").Value = data.DetermineNewMessages == true ? "1" : "0";
					setDoc.Save(setting);
					LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, data.ToString());

					result = true;
				}
			}
			catch (Exception ex)
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Error, ex.Message);
			}

			return result;
		}


		#region IDisposable Members

		public void Dispose()
		{
			if (!disposed)
				disposed = true;

			if (doc != null)
				doc = null;

			if (setDoc != null)
				setDoc = null;

			if (!config.IsNullOrEmpty())
				config = null;

			if (!string.IsNullOrEmpty(setting))
				setting = null;

			GC.SuppressFinalize(this);
		}

		#endregion

		~ConfigurationManager()
		{
			this.Dispose();
		}
	}

	public class UserData : INotifyPropertyChanged
	{
		public UserData()
		{
			this.UserName = string.Empty;
			this.AppId = 0;
			this.Password = string.Empty;
			AccessKey = string.Empty;
			Email = string.Empty;
		}

		private string m_user;
		public string UserName
		{
			get { return m_user; }
			set
			{
				m_user = value;
				this.OnPropertyChanged("UserName");
			}
		}

		private string m_email;
		public string Email
		{
			get { return m_email; }
			set
			{
				m_email = value;
				OnPropertyChanged("Email");
			}
		}

		private long m_appid;
		public long AppId
		{
			get { return m_appid; }
			set
			{
				m_appid = value;
				OnPropertyChanged("AppId");
			}
		}

		private string m_key;
		public string AccessKey
		{
			get { return m_key; }
			set
			{
				m_key = value;
				OnPropertyChanged("AccessKey");
			}
		}

		private string m_pass;
		public string Password
		{
			get { return m_pass; }
			set
			{
				m_pass = value;
				OnPropertyChanged("Password");
			}
		}

		public override string ToString()
		{
			return string.Format("{0} \n\r {1} \r\n {2} \n\r {3} \n\r", UserName, Password, AppId, AccessKey);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string msg)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(msg));
			}
		}
	}

	public class ProgramData : INotifyPropertyChanged
	{
		private bool m_IsClearImageCache;
		/// <summary>
		/// Clear image cache after program shutdown
		/// </summary>
		public bool IsClearImageCache
		{
			get { return m_IsClearImageCache; }
			set
			{
				if (m_IsClearImageCache != value)
				{
					m_IsClearImageCache = value;
					this.OnPropChanged("IsClearImageCache");
				}
			}
		}

		private double m_ImageCacheMaxSize;
		/// <summary>
		/// Max image cache size in KB
		/// </summary>
		public double ImageCacheMaxSize
		{
			get { return m_ImageCacheMaxSize; }
			set
			{
				if (m_ImageCacheMaxSize != value)
				{
					m_ImageCacheMaxSize = value;
					this.OnPropChanged("ImageCacheMaxSize");
				}
			}
		}

		private double m_DataCacheMaxSize;
		/// <summary>
		/// Max data cache size in KB
		/// </summary>
		public double DataCacheMaxSize
		{
			get { return m_DataCacheMaxSize; }
			set
			{
				if (m_DataCacheMaxSize != value)
				{
					m_DataCacheMaxSize = value;
					this.OnPropChanged("DataCacheMaxSize");
				}
			}
		}

		private bool m_newMsg;
		public bool DetermineNewMessages
		{
			get { return m_newMsg; }
			set
			{
				m_newMsg = value;
				OnPropChanged("DetermineNewMessages");
			}
		}

		#region INtotifyProperty Cahnged
		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropChanged(string propName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propName));
			}
		}
		#endregion

		public override string ToString()
		{
			return string.Format("ImageCacheSize {0} \n\r DataCaheSize {1} \n\r IsClearImage {2}", ImageCacheMaxSize, DataCacheMaxSize, IsClearImageCache.ToString());
		}
	}
}
