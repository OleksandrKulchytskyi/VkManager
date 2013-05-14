using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using WpfVkontacteClient.Entities;

namespace WpfVkontacteClient
{
	/// <summary>
	/// Вспомогательный класс - параметры для методов ВКонтакте
	/// </summary>
	public struct VKParameter
	{
		public VKParameter(string name, string value)
		{
			m_name = name.Trim();
			m_value = value.Trim();
		}

		/// <summary>
		/// Для хеш кеширования
		/// </summary>
		public string ForHash
		{
			get
			{
				return this.ToString();
			}
		}

		/// <summary>
		/// Для Url (исп. метод EscapeDataString для значения переменной value)
		/// </summary>
		public string ForUrl
		{
			get
			{
				//метод EscapeDataString  преобразует все символы,за исключением зарезервированных и незарезервированных знаков 
				//по стандарту RFC 2396, в их шестнадцатеричное представление
				return string.Format("{0}={1}", m_name, Uri.EscapeDataString(m_value));
			}
		}

		/// <summary>
		/// Для method format string
		/// </summary>
		public string ForFormat
		{
			get
			{
				return string.Concat(".", m_value);
			}
		}

		/// <summary>
		/// Имя параметра
		/// </summary>
		private string m_name;
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		/// <summary>
		/// Значение параметра
		/// </summary>
		private string m_value;
		public string Value
		{
			get { return m_value; }
			set { m_value = value; }
		}

		/// <summary>
		/// Перегруженый метод
		/// </summary>
		/// <returns>Строка типа имя=значение</returns>
		public override string ToString()
		{
			return string.Format("{0}={1}", m_name, m_value);
		}
	}

	/// <summary>
	/// Флаги для определения установок приложения
	/// </summary>
	[Flags]
	public enum VKSettings : int
	{
		ApplicationNotify = 1, //Извещения
		FreindsList = 2, //Работа со списком друзей
		Photo = 4, //Работа с фотографиями
		Audio = 8, //Аудиозаписи
		Video = 16, //Видеозаписи
		Offer = 32, //Предложения
		Questions = 64, //Вопросы
		Wiki = 128, //Вики
		LeftLink = 256, //Ссылка на приложение
		SpeedWallPublish = 512, //Стена пользователя
		Status = 1024, //Текущий статус (онлайн/оффлайн)
		Notes = 2048, //Заметки
		ExMessages = 4096, //Раширенная работа с сообщениями
		ExWall = 8192, //Расширенная работа со стеной
		Documents = 131072 //Работа с документами
	}

	/// <summary>
	/// Фильтр сообщений
	/// </summary>
	public enum VkMessageFilter : int
	{
		/// <summary>
		/// Непрочитаные
		/// </summary>
		NeverReaded = 1,
		/// <summary>
		/// Не из чата
		/// </summary>
		NotChat = 2,
		/// <summary>
		/// Только от друзей
		/// </summary>
		FromFriends = 4,
		/// <summary>
		/// нет фильтра
		/// </summary>
		None = 0
	}

	/// <summary>
	/// Обертка для методов ВКонтакте API.
	/// </summary>
	public sealed class VKontakteApiWrapper
	{
		public static VKSettings All = VKSettings.Documents | VKSettings.FreindsList | VKSettings.ExMessages |
						VKSettings.Audio | VKSettings.Video | VKSettings.ApplicationNotify |
						VKSettings.ExWall | VKSettings.Status;

		public static List<string> appRightsList = new List<string>()
		{
			"friends","photos","audio", "video", "docs", "status","wall","groups","messages","notifications"
		};

		#region private members
		private static VKontakteApiWrapper m_instance;

		private string m_sessionData;
		private long m_appId;
		private string m_appSecret;

		private long m_userId;
		private string m_secret;
		private string m_sid;

		private bool m_connected;

		private int m_settings;

		private string _accToken;

		private readonly string _execMethodTmpl = "https://api.vk.com/method/";
		#endregion

		/// <summary>
		/// Настройки wrappera
		/// </summary>
		public int Settings
		{
			get { return m_settings; }
		}

		/// <summary>
		/// Id подсоединенного пользователя в контакте
		/// </summary>
		public long UserId
		{
			get { return m_userId; }
		}

		/// <summary>
		/// Состояние подключения
		/// </summary>
		public bool IsConnected
		{
			get { return m_connected; }
		}

		public static VKontakteApiWrapper Instance
		{
			get
			{
				return m_instance;
			}
		}

		public VKontakteApiWrapper(long appId, string appSecret, int settings)
		{
			m_appId = appId;
			m_appSecret = appSecret;
			m_settings = settings;
			m_instance = this;
		}

		private static string _appRightsScope;
		public static string GenerateAppRightsScope
		{
			get
			{
				if (_appRightsScope == null)
					_appRightsScope = string.Join(",", appRightsList);
				return _appRightsScope;
			}
		}

		/// <summary>
		/// Соединяемся с сервером ВКонтакте и получаем настройки
		/// </summary>
		/// <returns>true если подсоединился</returns>
		public bool Connect(Window owner)
		{
			m_connected = false;
			//Получим данные сессии
			LoginWindow login = new LoginWindow(m_appId, m_settings);
			if (owner != null) login.Owner = owner;
			login.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			if (login.ShowDialog() == true)
			{
				this.m_sessionData = login.SessionData;//get the code value based on user login
				VKToken token = VkHelpers.GetToken(this.m_appId.ToString(), this.m_appSecret, m_sessionData);
				m_userId = token.user_id;
				_accToken = token.access_token;

				//string temp = m_sessionData.Replace("{", "").Replace("}", "");
				//string[] values = temp.Split(",".ToCharArray());
				try
				{
					//foreach (string str in values)
					//{
					//	string[] parts = str.Split(":".ToCharArray());
					//	parts[0] = parts[0].Replace("\"", "");
					//	parts[1] = parts[1].Replace("\"", "");
					//	if (parts[0].Equals("mid")) m_userId = Int32.Parse(parts[1]); //ID пользователя в контакте
					//	if (parts[0].Equals("sid")) m_sid = parts[1].Trim(); //Номер сессии
					//	if (parts[0].Equals("secret")) m_secret = parts[1].Trim(); //Секрет для подписи
					//}

					//Получим текущие установки
					XmlDocument res = this.ExecuteMethodByToken("getUserSettings", null);
					if (!this.HasError(res))
					{
						m_connected = true;
						Int32.TryParse(this.ParseSimpleResponse(res), out m_settings);
					}
					else
					{
						VKErrorInfo errorInfo = this.GetErrorContent(res);
						LogModule.LoggingModule.Instance.WriteMessage(LogModule.LoggingModule.Severity.Error, errorInfo.ErrorMessage);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(owner, ex.Message);
				}
			}
			if (login != null)
				login = null;
			return m_connected;
		}

		public bool CoonectBasedOnServer(string appId, string appSecret)
		{
			try
			{
				VKToken token = VkHelpers.GetAccessToken(appId, appSecret);
				_accToken = token.access_token;
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		/// <summary>
		/// Проверка есть ли в ответе ошибки
		/// </summary>
		/// <param name="doc">ХМЛ документ</param>
		/// <returns>true если тег "error" присутствует</returns>
		public bool HasError(XmlDocument doc)
		{
			foreach (XmlNode node in doc.ChildNodes)
			{
				if (node.Name == "error")
					return true;
			}
			return false;
		}

		/// <summary>
		/// Получаем контент сообщения об ошибке
		/// </summary>
		/// <param name="doc"></param>
		/// <returns>Информацию об ошибке</returns>
		public VKErrorInfo GetErrorContent(XmlDocument doc)
		{
			XDocument xDoc = doc.ToXDocument();
			if (xDoc != null)
			{
				VKErrorInfo errors = new VKErrorInfo();
				errors.ErrorCode = Convert.ToInt32(xDoc.Element("error").Element("error_code").Value);
				errors.ErrorMessage = xDoc.Element("error").Element("error_msg").Value;
				errors.ErrorParams = (from item in xDoc.Element("error").Element("request_params").Elements()
									  where item.Name == "param"
									  select new VKErrorParams()
									  {
										  Key = item.Element("key").Value,
										  Value = item.Element("value").Value
									  }).ToList();
				return errors;
			}
			return null;
		}

		/// <summary>
		/// Получение значения тега "response"
		/// </summary>
		/// <param name="doc">XML документ</param>
		/// <returns>значение тега @response@</returns>
		public string ParseSimpleResponse(XmlDocument doc)
		{
			foreach (XmlNode node in doc.ChildNodes)
			{
				if (node.Name == "response")
					return node.InnerText;
			}
			return string.Empty;
		}

		/// <summary>
		/// Создание подписи (Использование  MD5CryptoServiceProvider)
		/// </summary>
		/// <param name="values">список параметров</param>
		/// <returns>сигнатуру</returns>
		[Obsolete("Due to VK API changes this method is deprecated.")]
		private string CreateSignature(List<VKParameter> values)
		{
			using (MD5 md5 = MD5CryptoServiceProvider.Create())
			{
				//Сортируем ключи
				values.Sort((x, y) => { return String.Compare(x.Name, y.Name); });
				StringBuilder sb = new StringBuilder();
				sb.Append(m_userId.ToString());
				foreach (VKParameter par in values)
				{
					sb.Append(par.ForHash);
				}
				sb.Append(m_secret);
				//Вычесляем HashCode для составленой сигнатуры
				byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
				//Формируем строку сигнатуры
				StringBuilder sBuilder = new StringBuilder();
				for (int i = 0; i < data.Length; i++)
				{
					sBuilder.Append(data[i].ToString("x2"));
				}
				return sBuilder.ToString();
			}
		}

		[Obsolete("This method is deprecated. Please use ExecuteMethodByToken instead.")]
		public XmlDocument ExecuteMethod(string methodName, List<VKParameter> methodParams)
		{
			List<VKParameter> temp = new List<VKParameter>();
			temp.Add(new VKParameter("api_id", m_appId.ToString()));
			temp.Add(new VKParameter("method", methodName));
			temp.Add(new VKParameter("format", "XML"));
			temp.Add(new VKParameter("v", "3.0"));

			if (methodParams != null && methodParams.Count > 0)
				temp.AddRange(methodParams);

			string sig = this.CreateSignature(temp);
			temp.Add(new VKParameter("sid", m_sid));
			temp.Add(new VKParameter("sig", sig));

			StringBuilder sb = new StringBuilder();
			sb.Append("http://api.vkontakte.ru/api.php?");
			for (int i = 0; i < temp.Count - 1; i++)
			{
				sb.Append(temp[i].ForUrl);
				sb.Append("&");
			}
			sb.Append(temp[temp.Count - 1].ForUrl);

			WebClient wc = new WebClient();
			byte[] buff = wc.DownloadData(sb.ToString());
			string ret_string = Encoding.UTF8.GetString(buff);
			XmlDocument ret = new XmlDocument();
			ret.LoadXml(ret_string);
			return ret;
		}

		/// <summary>
		/// Исполнение Api метода из API Вконтакте
		/// </summary>
		/// <param name="methodName">Название метода</param>
		/// <param name="accessToken">User access_token</param>
		/// <param name="methodParams">Список параметров</param>
		/// <returns>XML document</returns>
		public XmlDocument ExecuteMethodByToken(string methodName, string accessToken, List<VKParameter> methodParams)
		{
			string accToken = string.Concat("&access_token=", accessToken);

			StringBuilder sb = new StringBuilder();
			sb.Append(_execMethodTmpl);

			List<VKParameter> parameters = new List<VKParameter>();
			parameters.Add(new VKParameter("method", methodName));
			parameters.Add(new VKParameter("format", "xml"));

			sb.Append(parameters[0].Value);
			sb.Append(parameters[1].ForFormat);
			sb.Append("?");

			if (methodParams != null && methodParams.Count > 0)
			{
				for (int i = 0; i < methodParams.Count; i++)
				{
					if (i != 0)
						sb.Append("&");
					sb.Append(methodParams[i].ForUrl);
				}
			}

			sb.Append(accToken);

			WebClient wc = new WebClient();
			byte[] buff = wc.DownloadData(sb.ToString());
			string ret_string = Encoding.UTF8.GetString(buff);
			XmlDocument ret = new XmlDocument();
			ret.LoadXml(ret_string);
			return ret;
		}

		/// <summary>
		/// Исполнение Api метода из API Вконтакте
		/// </summary>
		/// <param name="methodName">Название метода</param>
		/// <param name="methodParams">Список параметров</param>
		/// <returns>XML document</returns>
		public XmlDocument ExecuteMethodByToken(string methodName, List<VKParameter> methodParams)
		{
			return ExecuteMethodByToken(methodName, this._accToken, methodParams);
		}

		#region Friends region

		/// <summary>
		/// Получаем uid для онлайн пользователей текущего юзера
		/// </summary>
		/// <returns>список обьэктов OnlineFriends</returns>
		public List<OnlineFriends> GetUidOnlineFriends()
		{
			//Разрешили работать с друзьями?
			if (this.IsConnected && (this.Settings & (int)VKSettings.FreindsList) != 0)
			{
				//Получаем список со всеми параметрами
				XmlDocument doc = this.ExecuteMethodByToken("friends.getOnline", null);

				if (!this.HasError(doc))
				{
					DataSet ds = new DataSet();
					System.IO.StringReader xmlSR = new System.IO.StringReader(doc.InnerXml);
					ds.ReadXml(xmlSR, XmlReadMode.InferSchema);
					List<OnlineFriends> friends = new List<OnlineFriends>();

					foreach (DataRow dr in ds.Tables["uid"].Rows)
					{
						friends.Add(new OnlineFriends(dr));
					}

					return friends;
				}
				else
				{
					VKErrorInfo errorInfo = this.GetErrorContent(doc);
					MessageBox.Show(string.Format("{0} - {1}", errorInfo.ErrorCode, errorInfo.ErrorMessage));
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Получаем инфомацию об онлайн пользователях
		/// </summary>
		/// <param name="online">список id пользователей который онлайн</param>
		/// <returns></returns>
		public List<Friend> GetInfoForOnlineFriends(List<OnlineFriends> online)
		{
			if (this.IsConnected && (Settings & (int)VKSettings.FreindsList) != 0)
			{
				List<VKParameter> parameters = new List<VKParameter>();
				string uids = this.GetUidsString(online);
				if (string.IsNullOrEmpty(uids))
					return null;
				parameters.Add(new VKParameter("uids", uids));
				parameters.Add(new VKParameter("fields",
				"uid, first_name, last_name, nickname, sex, bdate, city, country, timezone, photo, photo_medium, photo_big, lists, domain, has_mobile, rate, contacts, education"));
				XmlDocument doc = ExecuteMethodByToken("getProfiles", parameters);
				if (!HasError(doc))
				{
					List<Friend> friends = new List<Friend>();
					DataSet ds = new DataSet();
					System.IO.StringReader xmlSR = new System.IO.StringReader(doc.InnerXml);
					ds.ReadXml(xmlSR, XmlReadMode.InferSchema);
					if (ds.Tables["user"] != null && ds.Tables["user"].Rows.Count > 0)
					{
						foreach (DataRow dr in ds.Tables["user"].Rows)
						{
							friends.Add(new Friend(dr));
						}
					}
					return friends;
				}
				else
				{
					VKErrorInfo errorInfo = this.GetErrorContent(doc);
					MessageBox.Show(string.Format("{0} - {1}", errorInfo.ErrorCode, errorInfo.ErrorMessage));
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает список идентификаторов друзей пользователя или расширенную информацию о друзьях пользователя (при использовании параметра fields). 
		/// </summary>
		/// <param name="userId">идентификатор пользователя, для которого необходимо получить список друзей. Если параметр не задан, то считается, что он равен идентификатору текущего пользователя.</param>
		///<param name="count">количество друзей, которое нужно вернуть. (по умолчанию – все друзья) </param>
		/// <returns></returns>
		public List<Friend> GetAllFriends(string userId, int? count, int? offset)
		{
			//Разрешили работать с друзьями?
			if (IsConnected && (Settings & (int)VKSettings.FreindsList) != 0)
			{
				//Получаем список со всеми параметрами
				List<VKParameter> list = new List<VKParameter>();
				if (!userId.IsNullOrEmpty())
					list.Add(new VKParameter("uid", userId));

				list.Add(new VKParameter("fields", "uid,first_name,last_name,nickname,sex,bdate,city,country,timezone,photo,photo_medium,photo_big,online,lists,domain,has_mobile,rate,contacts,education"));

				if (count.HasValue)
					list.Add(new VKParameter("count", count.Value.ToString()));

				if (offset.HasValue)
					list.Add(new VKParameter("offset", offset.Value.ToString()));

				XmlDocument doc = ExecuteMethodByToken("friends.get", list);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					System.IO.StringReader xmlSR = new System.IO.StringReader(doc.InnerXml);
					ds.ReadXml(xmlSR, XmlReadMode.InferSchema);
					List<Friend> friends = new List<Friend>();
					foreach (DataRow dr in ds.Tables["user"].Rows)
					{
						friends.Add(new Friend(dr));
					}
					return friends;
				}
				else
				{
					VKErrorInfo errorInfo = GetErrorContent(doc);
					MessageBox.Show(string.Format("{0} - {1}", errorInfo.ErrorCode, errorInfo.ErrorMessage));
					return null;
				}
			}
			return null;
		}

		public Nullable<int> FriendsCount(string userId)
		{
			//Разрешили работать с друзьями?
			if (IsConnected && (Settings & (int)VKSettings.FreindsList) != 0)
			{
				//Получаем список со всеми параметрами
				List<VKParameter> list = new List<VKParameter>();
				if (!userId.IsNullOrEmpty())
					list.Add(new VKParameter("uid", userId));

				XmlDocument doc = ExecuteMethodByToken("friends.get", list);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response").ChildNodes.Count;
				}
				else
				{
					VKErrorInfo errorInfo = GetErrorContent(doc);
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает список меток друзей текущего пользователя.(Группы в которые входят друзья)
		/// </summary>
		/// <returns>Cписок меток друзей</returns>
		public List<FriendGroup> GetFriendGroups()
		{
			if (IsConnected && (Settings & (int)VKSettings.FreindsList) != 0)
			{
				XmlDocument doc = ExecuteMethodByToken("friends.getLists", null);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("friendGroupDS");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<FriendGroup> groups = new List<FriendGroup>();
					foreach (DataRow dr in ds.Tables["list"].Rows)
					{
						groups.Add(new FriendGroup(dr));
					}
					return groups;
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Возвращает список идентификаторов общих друзей между парой пользователей
		/// </summary>
		/// <param name="target_id">идентификатор пользователя с которым необходимо искать общих друзей. (обьязательный)</param>
		/// <param name="source_id">идентификатор пользователя, чьи друзья пересекаются с друзьями пользователя с идентификатором target_uid. Если параметр не задан, то считается, что source_uid равен идентификатору текущего пользователя.</param>
		/// <returns></returns>
		public List<string> GetMutalFriends(string target_id, string source_id)
		{
			if (IsConnected && (Settings & (int)(VKSettings.FreindsList)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("target_id", target_id));
				if (!source_id.IsNullOrEmpty())
					param.Add(new VKParameter("source_id", source_id));
				XmlDocument doc = ExecuteMethodByToken("friends.getMutual", param);
				if (!HasError(doc))
				{
					return (from item in doc.ToXDocument().Element("response").Elements()
							where item.Name.ToString() == "uid"
							select item.Value.ToString()).ToList();
				}
				return null;
			}
			return null;
		}

		#endregion

		#region User region
		/// <summary>
		/// Получаем информацию о пользователе
		/// </summary>
		/// <param name="userId">id пользователя (обьязательный параметр)</param>
		/// <returns>Информацию о пользователе</returns>
		public UserInfos GetUserInfo(string userId)
		{
			if (IsConnected && (Settings & ((int)VKSettings.FreindsList)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("uids", userId));
				param.Add(new VKParameter("fields",
					"uid, first_name, last_name, nickname, domain, sex, bdate, city, country, timezone, photo, photo_medium, photo_big, has_mobile, rate, contacts, education"));
				XmlDocument doc = ExecuteMethodByToken("getProfiles", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					return new UserInfos(ds.Tables["user"].Rows[0]);
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Получаем текущий баланс пользователя
		/// </summary>
		/// <returns>сумма баланса</returns>
		public string GetUserBalance()
		{
			if (this.IsConnected && (this.Settings & (int)VKSettings.FreindsList) != 0)
			{
				//Получаем список со всеми параметрами
				XmlDocument doc = this.ExecuteMethodByToken("getUserBalance", null);
				if (!this.HasError(doc))
				{
					DataSet ds = new DataSet();
					System.IO.StringReader xmlSR = new System.IO.StringReader(doc.InnerXml);
					ds.ReadXml(xmlSR, XmlReadMode.InferSchema);
					Entities.BalanceInfo balance = new Entities.BalanceInfo(ds.Tables["uid"].Rows[0]);
					return balance.Balance;
				}
				else
				{
					VKErrorInfo info = this.GetErrorContent(doc);
					return string.Empty;
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// Получаем информаицю о том или установил тпользователь данное приложение
		/// </summary>
		/// <param name="uid">id пользователя</param>
		/// <returns>true елси установлено</returns>
		public Nullable<bool> IsAppUser(string uid)
		{
			if (this.IsConnected && (this.Settings & (int)VKSettings.FreindsList) != 0)
			{
				//Получаем список со всеми параметрами
				XmlDocument doc = null;
				if (string.IsNullOrEmpty(uid))
				{
					doc = this.ExecuteMethodByToken("isAppUser", null);
				}
				else
				{
					var data = new List<VKParameter>();
					data.Add(new VKParameter("uid", uid));
					doc = this.ExecuteMethodByToken("isAppUser", data);
				}
				if (!this.HasError(doc))
				{
					string value = doc.ToXDocument().Element("response").Value;
					return value == "1" ? true : false;
				}
				else
				{
					VKErrorInfo info = this.GetErrorContent(doc);
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвраает список групп в которых состоит пользователь
		/// </summary>
		/// <returns>Список групп</returns>
		public List<UserGroup> GetUserGroups()
		{
			if (this.IsConnected && (this.Settings & (int)VKSettings.FreindsList) != 0)
			{
				XmlDocument doc = this.ExecuteMethodByToken("getGroupsFull", null);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("tempDS");
					System.IO.StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<Entities.UserGroup> groups = new List<UserGroup>();
					foreach (DataRow dr in ds.Tables["group"].Rows)
					{
						groups.Add(new UserGroup(dr));
					}
					return groups;
				}
				else
				{
					VKErrorInfo info = GetErrorContent(doc);
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Плучаем список uid друзей которые установили данное приложение 
		/// </summary>
		/// <returns>список uid</returns>
		public List<string> GetAppUsers()
		{
			if (this.IsConnected && (Settings & (int)VKSettings.FreindsList) != 0)
			{
				XmlDocument doc = ExecuteMethodByToken("friends.getAppUsers", null);
				if (!HasError(doc))
				{
					return (from item in doc.ToXDocument().Element("response").Elements()
							where item.Name.ToString().ToLower() == "uid"
							select item.Value.ToString()).ToList();
				}
				else
				{
					return null;
				}
			}
			return null;
		}
		#endregion

		#region Activity
		/// <summary>
		/// Возвращает последнюю запись пользователя, которую он опубликовал на своей стене.
		/// </summary>
		/// <param name="uid">идентификатор пользователя (по умолчанию - текущий пользователь).</param>
		/// <returns></returns>
		public ActivityInfo ActivityGet(string uid)
		{
			if (this.IsConnected && (this.Settings & (int)VKSettings.Status) != 0)
			{
				//Получаем список со всеми параметрами
				var data = new List<VKParameter>();
				data.Add(new VKParameter("uid", uid));
				XmlDocument doc = this.ExecuteMethodByToken("activity.get", data);
				if (!this.HasError(doc))
				{
					ActivityInfo activity = new ActivityInfo();
					activity.Activity = doc.SelectSingleNode("response/activity/text()").Value;
					activity.Id = long.Parse(doc.SelectSingleNode("response/id/text()").Value);
					activity.Time = Utils.DateTimeUtils.ConvertFromUnixTimestamp(double.Parse(doc.SelectSingleNode("response/time/text()").Value));
					return activity;
				}
				else
				{
					VKErrorInfo info = this.GetErrorContent(doc);
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Добавляет текстовое сообщение на стену текущего пользователя. 
		/// </summary>
		/// <param name="text">текст сообщения.</param>
		/// <returns></returns>
		public Nullable<long> ActivitySet(string text)
		{
			if (this.IsConnected && (this.Settings & (int)VKSettings.Status) != 0)
			{
				//Получаем список со всеми параметрами
				var param = new List<VKParameter>();
				param.Add(new VKParameter("text", text));
				XmlDocument doc = this.ExecuteMethodByToken("activity.set", param);
				if (!this.HasError(doc))
				{
					return long.Parse(doc.SelectSingleNode("response/text()").Value);
				}
				else
				{
					VKErrorInfo info = this.GetErrorContent(doc);
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает записи со стены указанного пользователя, которые были написаны им самим.
		/// </summary>
		/// <param name="uid">идентификатор пользователя (по умолчанию - текущий пользователь).</param>
		/// <returns></returns>
		public List<ActivityInfo> ActivityGetHistory(string uid)
		{
			if (this.IsConnected && (this.Settings & (int)VKSettings.Status) != 0)
			{
				//Получаем список со всеми параметрами
				var data = new List<VKParameter>();
				data.Add(new VKParameter("uid", uid));
				XmlDocument doc = this.ExecuteMethodByToken("activity.getHistory", data);
				if (!this.HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<ActivityInfo> list = new List<ActivityInfo>();
					foreach (DataRow row in ds.Tables["activity"].Rows)
					{
						ActivityInfo activity = new ActivityInfo();
						activity.Activity = row["text"].ToString();
						activity.Id = long.Parse(row["id"].ToString());
						activity.Time = Utils.DateTimeUtils.ConvertFromUnixTimestamp(double.Parse(row["created"].ToString()));
						list.Add(activity);
					}
					return list;
				}
				else
				{
					VKErrorInfo info = this.GetErrorContent(doc);
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает обновления записей пользователей, опубликованных ими на собственных стенах.
		/// </summary>
		/// <param name="time">будут возвращены записи, которые были созданы не раньше этого времени (unixtime). Если этот параметр не указан, то используются параметры offset и count.</param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public List<ActivityInfo> ActivityGetNews(Nullable<DateTime> time, int offset = 0, int count = 100)
		{
			if (this.IsConnected && (this.Settings & (int)VKSettings.Status) != 0)
			{
				//Получаем список со всеми параметрами
				var param = new List<VKParameter>();
				if (time.HasValue)
				{
					param.Add(new VKParameter("timestamp", Utils.DateTimeUtils.ConvertToUnixTimestamp(time.Value).ToString()));
				}
				else
				{
					param.Add(new VKParameter("offset", offset.ToString()));
					param.Add(new VKParameter("count", count.ToString()));
				}

				XmlDocument doc = this.ExecuteMethodByToken("activity.getNews", param);
				if (!this.HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<ActivityInfo> list = new List<ActivityInfo>();
					foreach (DataRow row in ds.Tables["activity"].Rows)
					{
						ActivityInfo activity = new ActivityInfo();
						activity.Activity = row["text"].ToString();
						activity.Id = long.Parse(row["uid"].ToString());
						activity.Time = Utils.DateTimeUtils.ConvertFromUnixTimestamp(double.Parse(row["timestamp"].ToString()));
						list.Add(activity);
					}
					return list;
				}
				else
				{
					VKErrorInfo info = this.GetErrorContent(doc);
					return null;
				}
			}
			return null;
		}

		#endregion

		#region Work with photos
		/// <summary>
		/// Получаем список альюомов для пользователя
		/// </summary>
		/// <param name="uid">ИД пользователя</param>
		/// <param name="aids">ид альбомов</param>
		/// <returns>список альбомов</returns>
		public List<UserAlbum> GetUserAlbum(string uid, List<string> aids)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("uid", uid));
				if (aids != null && aids.Count > 0)
					param.Add(new VKParameter("aids", GetStringFromList(aids)));

				XmlDocument doc = ExecuteMethodByToken("photos.getAlbums", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<UserAlbum> albums = new List<UserAlbum>();
					if (ds.Tables["album"] != null && ds.Tables["album"].Rows.Count > 0)
					{
						foreach (DataRow dr in ds.Tables["album"].Rows)
						{
							albums.Add(new UserAlbum(dr));
						}
					}
					return albums;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает количество доступных альбомов пользователя
		/// </summary>
		/// <param name="uid"></param>
		/// <param name="gid">ID группы, которой принадлежат альбомы.</param>
		/// <returns></returns>
		public Nullable<int> GetUserAlbumCount(string uid, Nullable<long> gid)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("uid", uid));
				if (gid.HasValue)
					param.Add(new VKParameter("gid", gid.Value.ToString()));

				XmlDocument doc = ExecuteMethodByToken("photos.getAlbumsCount", param);
				if (!HasError(doc))
				{
					return Int32.Parse(doc.SelectSingleNode("response/text()").Value);
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает все фотографии пользователя в антихронологическом порядке.
		/// </summary>
		/// <param name="owner_id">идентификатор пользователя (по-умолчанию - текущий пользователь). </param>
		/// <param name="offset">смещение, необходимое для выборки определенного подмножества фотографий</param>
		/// <param name="count">количество фотографий, которое необходимо получить (но не более 100)</param>
		/// <returns>Список фотографий</returns>
		public List<UserPhoto> GetAllUserPhoto(string owner_id, string offset, string count)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> parameters = new List<VKParameter>();
				if (!string.IsNullOrEmpty(owner_id))
					parameters.Add(new VKParameter("owner_id", owner_id));
				if (!string.IsNullOrEmpty(owner_id))
					parameters.Add(new VKParameter("offset", offset));
				if (!string.IsNullOrEmpty(count))
					parameters.Add(new VKParameter("count", count));

				XmlDocument doc = ExecuteMethodByToken("photos.getAll", parameters);
				if (!HasError(doc))
				{
					var ds = new DataSet("tempDS");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<UserPhoto> photos = new List<UserPhoto>();
					foreach (DataRow row in ds.Tables["photo"].Rows)
					{
						photos.Add(new UserPhoto(row));
					}
					return photos;
				}
				else
				{
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Получаем список комментраев к фотографии
		/// </summary>
		/// <param name="photoId">id фотографии (обьязательный параметр)</param>
		/// <param name="ownerId">идентификатор пользователя (по-умолчанию - текущий пользователь). </param>
		/// <param name="offset">смещение, необходимое для выборки определенного подмножества комментариев</param>
		/// <param name="count">количество комментариев, которое необходимо получить (но не более 100).</param>
		/// <returns></returns>
		public List<UserComment> GetPhotoComments(string photoId, string ownerId, string offset, string count)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("pid", photoId));
				if (!string.IsNullOrEmpty(ownerId))
					param.Add(new VKParameter("owner_id", ownerId));
				if (!string.IsNullOrEmpty(offset))
					param.Add(new VKParameter("offset", offset));
				if (!string.IsNullOrEmpty(count))
					param.Add(new VKParameter("count", count));

				XmlDocument doc = ExecuteMethodByToken("photos.getComments", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("photoComents");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					var data = new List<UserComment>();
					if (!ds.Tables["comment"].IsNull() && ds.Tables["comment"].Rows.Count > 0)
					{
						foreach (DataRow row in ds.Tables["comment"].Rows)
						{
							data.Add(new UserComment(row));
						}
					}
					return data;
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Возвращает список фотографий, на которых отмечен пользователь
		/// </summary>
		/// <param name="photoId">id пользователя (по-умолчанию - текущий пользователь)</param>
		/// <param name="offset">смещение, необходимое для выборки определенного подмножества комментариев</param>
		/// <param name="count">количество комментариев, которое необходимо получить (но не более 100).</param>
		/// <returns></returns>
		public List<UserPhoto> GetUserPhotos(string userId, string offset, string count)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(userId))
					param.Add(new VKParameter("uid", userId));

				if (!string.IsNullOrEmpty(offset))
					param.Add(new VKParameter("offset", offset));
				if (!string.IsNullOrEmpty(count))
					param.Add(new VKParameter("count", count));

				XmlDocument doc = ExecuteMethodByToken("photos.getUserPhotos", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("userPhoto");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					var data = new List<UserPhoto>();
					if (ds.Tables["photo"].Rows.Count == 0)
						return data;
					foreach (DataRow row in ds.Tables["photo"].Rows)
					{
						data.Add(new UserPhoto(row));
					}
					return data;
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Возвращает информацию о фотографиях по их идентификаторам. 
		/// </summary>
		/// <param name="photos">перечисленные через запятую идентификаторы, которые представляют собой идущие через знак подчеркивания id пользователей, разместивших фотографии, и id самих фотографий. (1_129207899,6492_135055734 )</param>
		/// <returns>список фотографий</returns>
		public List<PhotoExteded> GetPhotosById(List<string> photos)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (photos != null && photos.Count > 0)
				{
					param.Add(new VKParameter("photos", this.GetStringFromList(photos)));
				}
				XmlDocument doc = ExecuteMethodByToken("photos.getById", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("userPhoto");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					var data = new List<PhotoExteded>();
					if (ds.Tables["photo"].Rows.Count == 0)
						return data;
					foreach (DataRow row in ds.Tables["photo"].Rows)
					{
						data.Add(new PhotoExteded(row));
					}
					return data;
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Возвращает список фотографий в альбоме
		/// </summary>
		/// <param name="userId">ID пользователя, которому принадлежит альбом с фотографиями</param>
		/// <param name="aid">ID альбома с фотографиями</param>
		/// <param name="pids">перечисленные через запятую ID фотографий</param>
		/// <returns></returns>
		public List<PhotoExteded> GetPhotosFromAlbum(string userId, string aid, List<string> pids)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("uid", userId));
				param.Add(new VKParameter("aid", aid));
				if (pids != null && pids.Count > 0)
				{
					param.Add(new VKParameter("pids", this.GetStringFromList(pids)));
				}
				XmlDocument doc = ExecuteMethodByToken("photos.get", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("photoDS");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<PhotoExteded> photos = new List<PhotoExteded>();
					if (ds.Tables["photo"] != null && ds.Tables["photo"].Rows.Count > 0)
					{
						foreach (DataRow dr in ds.Tables["photo"].Rows)
						{
							photos.Add(new PhotoExteded(dr));
						}
					}
					return photos;
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Делает фотографию обложкой альбома. 
		/// </summary>
		/// <param name="pid">	id фотографии, которая должна стать обложкой альбома. </param>
		/// <param name="aid">id альбома. </param>
		/// <param name="uid"></param>
		/// <returns></returns>
		public bool AlbumMakePhotoCover(long pid, long aid, string uid)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("pid", pid.ToString()));
				param.Add(new VKParameter("aid", aid.ToString()));
				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("oid", uid));

				XmlDocument doc = ExecuteMethodByToken("photos.makeCover", param);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text()").Value == "1";
				}
			}
			return false;
		}

		/// <summary>
		/// Возвращает адрес сервера для загрузки фотографий.
		/// </summary>
		/// <param name="aid"></param>
		/// <param name="gid"></param>
		/// <returns></returns>
		public string GetUploadServerForPhoto(long aid, Nullable<long> gid)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("aid", aid.ToString()));
				if (gid.HasValue)
					param.Add(new VKParameter("gid", gid.Value.ToString()));

				XmlDocument doc = ExecuteMethodByToken("photos.getUploadServer", param);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/upload_url/text()").Value;
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// Возвращает адрес сервера для загрузки фотографии на страницу пользователя. После удачной загрузки Вы можете воспользоваться методом photos.saveProfilePhoto. 
		/// </summary>
		/// <returns></returns>
		public string GetProfileUploadServerForPhoto()
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				XmlDocument doc = ExecuteMethodByToken("photos.getProfileUploadServer", null);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/upload_url/text()").Value;
				}
				return null;
			}
			return string.Empty;
		}

		/// <summary>
		/// Создает пустой альбом для фотографий.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="privacy">уровень доступа к альбому. Значения: 0 – все пользователи, 1 – только друзья, 2 – друзья и друзья друзей, 3 - только я.</param>
		/// <param name="commentPrivacy"></param>
		/// <param name="description"></param>
		/// <returns></returns>
		public UserAlbum PhotoCreateAlbum(string title, string description, int privacy = 1, int commentPrivacy = 1)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("title", title));
				param.Add(new VKParameter("privacy", privacy.ToString()));
				param.Add(new VKParameter("comment_privacy", commentPrivacy.ToString()));
				if (!string.IsNullOrEmpty(description))
					param.Add(new VKParameter("description", description));

				XmlDocument doc = ExecuteMethodByToken("photos.createAlbum", param);
				if (!HasError(doc))
				{
					var ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					if (ds.Tables["album"].Rows[0] != null)
						return new UserAlbum(ds.Tables["album"].Rows[0]);
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Редактирует данные альбома для фотографий пользователя.
		/// </summary>
		/// <param name="aid">идентификатор редактируемого альбома</param>
		/// <param name="title"></param>
		/// <param name="privacy">новый уровень доступа к альбому. Значения: 0 – все пользователи, 1 – только друзья, 2 – друзья и друзья друзей, 3 - только я. </param>
		/// <param name="comPrivacy"></param>
		/// <param name="description"></param>
		/// <returns></returns>
		public bool PhotoAlbumEdit(long aid, string title, string description, int privacy = 1, int comPrivacy = 1)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("aid", aid.ToString()));
				param.Add(new VKParameter("title", title));
				param.Add(new VKParameter("privacy", privacy.ToString()));
				param.Add(new VKParameter("comment_privacy", comPrivacy.ToString()));
				if (!string.IsNullOrEmpty(description))
					param.Add(new VKParameter("description", description));

				XmlDocument doc = ExecuteMethodByToken("photos.editAlbum", param);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text()").Value == "1";
				}
				return false;
			}
			return false;
		}

		/// <summary>
		/// Переносит фотографию из одного альбома в другой.
		/// </summary>
		/// <param name="pid">id переносимой фотографии.</param>
		/// <param name="targetAid">id альбома, куда переносится фотография. </param>
		/// <param name="oid"></param>
		/// <returns></returns>
		public bool PhotoMovePhoto(long pid, long targetAid, string oid)
		{
			if (IsConnected && (Settings & (int)VKSettings.Photo) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("pid", pid.ToString()));
				param.Add(new VKParameter("target_aid", targetAid.ToString()));
				if (!string.IsNullOrEmpty(oid))
					param.Add(new VKParameter("oid", oid));

				XmlDocument doc = ExecuteMethodByToken("photos.move", param);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text()").Value == "1";
				}
				return false;
			}
			return false;
		}

		#endregion

		#region Work with Video

		/// <summary>
		/// Возвращает список видеозаписей, на которых отмечен пользователь
		/// </summary>
		/// <param name="userId">идентификатор пользователя (по-умолчанию - текущий пользователь). </param>
		/// <param name="count">количество видеозаписей, которое необходимо получить (но не более 100).</param>
		/// <param name="offset">смещение, необходимое для выборки определенного подмножества видеозаписей.</param>
		/// <returns></returns>
		public List<UserVideo> GetUserVideo(string userId, string offset, string count)
		{
			if (IsConnected && (Settings & (int)VKSettings.Video) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!userId.IsNullOrEmpty())
					param.Add(new VKParameter("uid", userId));
				if (!offset.IsNullOrEmpty())
					param.Add(new VKParameter("offset", offset));
				if (!count.IsNullOrEmpty())
					param.Add(new VKParameter("count", count));
				XmlDocument doc = ExecuteMethodByToken("video.getUserVideos", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("userVideo");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					var data = new List<UserVideo>();
					if (ds.Tables["video"] != null && ds.Tables["video"].Rows.Count > 0)
					{
						foreach (DataRow row in ds.Tables["video"].Rows)
						{
							data.Add(new UserVideo(row));
						}
					}
					return data;
				}
				return null;

			}
			return null;
		}

		/// <summary>
		/// Возвращает информацию о видеозаписях
		/// </summary>
		/// <param name="videos">перечисленные через запятую идентификаторы, которые представляют собой идущие через знак подчеркивания id пользователей, которым принадлежат видеозаписи, и id самих видеозаписей. Если видеозапись принадлежит группе, то в качестве первого параметра используется -id группы</param>
		/// <param name="userId">id пользователя, видеозаписи которого нужно вернуть. Если указан параметр videos, uid игнорируется. </param>
		/// <param name="width">требуемая ширина изображений видеозаписей в пикселах. Возможные значения - 130, 160 (по умолчанию), 320. </param>
		/// <param name="count">количество возвращаемых видеозаписей (максимум 200). </param>
		/// <param name="offset">смещение относительно первой найденной видеозаписи для выборки определенного подмножества. </param>
		/// <returns></returns>
		public List<UserVideo> GetVideo(List<string> videos, string userId, string width, string count, string offset)
		{
			if (IsConnected && (Settings & (int)VKSettings.Video) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (videos != null && videos.Count > 0)
					param.Add(new VKParameter("videos", this.GetStringFromList(videos)));
				if (!userId.IsNullOrEmpty())
					param.Add(new VKParameter("uid", userId));
				if (!width.IsNullOrEmpty())
					param.Add(new VKParameter("width", offset));
				else
				{
					param.Add(new VKParameter("width", "160"));
				}

				if (!offset.IsNullOrEmpty())
					param.Add(new VKParameter("offset", offset));
				if (!count.IsNullOrEmpty())
					param.Add(new VKParameter("count", count));

				XmlDocument doc = ExecuteMethodByToken("video.get", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("userVideo");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					var data = new List<UserVideo>();
					if (ds.Tables["video"] != null && ds.Tables["video"].Rows.Count > 0)
					{
						foreach (DataRow row in ds.Tables["video"].Rows)
						{
							data.Add(new UserVideo(row));
						}
					}
					return data;
				}
				return null;

			}
			return null;
		}

		/// <summary>
		/// Возвращает список видеозаписей в соответствии с заданным критерием поиска.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="sort">Вид сортировки. 1 - по длительности видеозаписи, 0 - по дате добавления. </param>
		/// <param name="onlyHD"></param>
		/// <param name="count"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public List<UserVideo> VideoSearch(string query, int sort = 0, bool onlyHD = false, int count = 100, int offset = 0)
		{
			if (IsConnected && (Settings & (int)VKSettings.Video) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (string.IsNullOrEmpty(query))
					return new List<UserVideo>();

				param.Add(new VKParameter("q", query));
				param.Add(new VKParameter("sort", sort.ToString()));
				param.Add(new VKParameter("hd", onlyHD == true ? "0" : "1"));
				param.Add(new VKParameter("count", count.ToString()));
				param.Add(new VKParameter("offset", offset.ToString()));

				XmlDocument doc = ExecuteMethodByToken("video.search", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("userVideo");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					var data = new List<UserVideo>();
					if (ds.Tables["video"] != null && ds.Tables["video"].Rows.Count > 0)
					{
						foreach (DataRow row in ds.Tables["video"].Rows)
						{
							data.Add(new UserVideo(row));
						}
					}
					return data;
				}
				return null;

			}
			return null;
		}

		/// <summary>
		/// Удаляет видеозапись со страницы пользователя. 
		/// </summary>
		/// <param name="vid"></param>
		/// <param name="oid">id владельца видеозаписи</param>
		/// <returns></returns>
		public bool VideoDelete(long vid, long oid)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Audio)) != 0)
			{
				XmlDocument doc = ExecuteMethodByToken("video.delete", new List<VKParameter>() { new VKParameter("vid", vid.ToString()),
				new VKParameter("oid",oid.ToString())});
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text()").Value == "1";
				}
				return false;
			}
			return false;
		}

		/// <summary>
		/// Возвращает информацию о видеозаписях
		/// </summary>
		/// <param name="videos">перечисленные через запятую идентификаторы, которые представляют собой идущие через знак подчеркивания id пользователей, которым принадлежат видеозаписи, и id самих видеозаписей. Если видеозапись принадлежит группе, то в качестве первого параметра используется -id группы</param>
		/// <param name="userId">id пользователя, видеозаписи которого нужно вернуть. Если указан параметр videos, uid игнорируется. </param>
		/// <param name="width">требуемая ширина изображений видеозаписей в пикселах. Возможные значения - 130, 160 (по умолчанию), 320. </param>
		/// <param name="count">количество возвращаемых видеозаписей (максимум 200). </param>
		/// <param name="offset">смещение относительно первой найденной видеозаписи для выборки определенного подмножества. </param>
		/// <returns></returns>
		public List<UserVideo> VideoGetByIds(List<string> videos, int width = 160)
		{
			if (IsConnected && (Settings & (int)VKSettings.Video) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (videos != null && videos.Count > 0)
					param.Add(new VKParameter("videos", this.GetStringFromList(videos)));

				param.Add(new VKParameter("width", width.ToString()));

				XmlDocument doc = ExecuteMethodByToken("video.get", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("userVideo");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					var data = new List<UserVideo>();
					if (ds.Tables["video"] != null && ds.Tables["video"].Rows.Count > 0)
					{
						foreach (DataRow row in ds.Tables["video"].Rows)
						{
							data.Add(new UserVideo(row));
						}
					}
					return data;
				}
				return null;

			}
			return null;
		}

		#endregion

		#region Work with Audio

		/// <summary>
		/// Получаем список песен пользователя
		/// </summary>
		/// <param name="uid">id пользователя (если null , то текущий)</param>
		/// <param name="gid">id группы</param>
		/// <param name="aids">id песен</param>
		/// <param name="need_user">получать данные о пользователе которму принадлежит песня(1 или 0)</param>
		/// <returns>список песен</returns>
		public List<UserAudio> GetUserAudio(string uid, string gid, List<string> aids, string need_user)
		{
			if (IsConnected && (Settings & (int)VKSettings.Audio) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(gid))
					param.Add(new VKParameter("gid", gid));
				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("uid", uid));
				if (aids != null && aids.Count > 0)
					param.Add(new VKParameter("aids", GetStringFromList(aids)));

				if (!string.IsNullOrEmpty(need_user))
					param.Add(new VKParameter("need_user", need_user));
				XmlDocument doc = ExecuteMethodByToken("audio.get", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<UserAudio> audio = new List<UserAudio>();
					if (ds.Tables["audio"] != null && ds.Tables["audio"].Rows.Count > 0)
					{
						foreach (DataRow dr in ds.Tables["audio"].Rows)
						{
							audio.Add(new UserAudio(dr));
						}
					}
					return audio;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает список аудиозаписей в соответствии с заданным критерием поиска. 
		/// </summary>
		/// <param name="songText"></param>
		/// <param name="sort"></param>
		/// <param name="withWords"></param>
		/// <param name="count">max 200</param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public List<UserAudio> AudioSearch(string songText, int sort = 1, bool withWords = false, int count = 100, int offset = 0)
		{
			if (IsConnected && (Settings & (int)VKSettings.Audio) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (string.IsNullOrEmpty(songText))
					return new List<UserAudio>();

				param.Add(new VKParameter("q", songText));
				param.Add(new VKParameter("sort", sort.ToString()));
				param.Add(new VKParameter("lyrics", withWords == true ? "1" : "0"));
				param.Add(new VKParameter("count", count.ToString()));
				param.Add(new VKParameter("offset", offset.ToString()));

				XmlDocument doc = ExecuteMethodByToken("audio.search", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<UserAudio> audio = new List<UserAudio>();
					if (ds.Tables["audio"] != null && ds.Tables["audio"].Rows.Count > 0)
					{
						foreach (DataRow dr in ds.Tables["audio"].Rows)
						{
							audio.Add(new UserAudio(dr));
						}
					}
					return audio;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает адрес сервера для загрузки аудиозаписей. 
		/// </summary>
		/// <returns></returns>
		public string AudioGetUploadServer()
		{
			if (IsConnected && (Settings & ((int)VKSettings.Audio)) != 0)
			{
				XmlDocument doc = ExecuteMethodByToken("audio.getUploadServer", null);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/upload_url/text()").Value.Trim();
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Возвращает информацию об аудиозаписях. 
		/// </summary>
		/// <param name="audios">перечисленные через запятую идентификаторы – идущие через знак подчеркивания id пользователей, которым принадлежат аудиозаписи, и id самих аудиозаписей. Если аудиозапись принадлежит группе, то в качестве первого параметра используется -id группы.</param>
		/// <returns></returns>
		public List<UserAudio> AudioGetById(List<string> audios)
		{
			if (IsConnected && (Settings & (int)VKSettings.Audio) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();

				param.Add(new VKParameter("audios", GetStringFromList(audios)));

				XmlDocument doc = ExecuteMethodByToken("audio.getById", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<UserAudio> audio = new List<UserAudio>();
					if (ds.Tables["audio"] != null && ds.Tables["audio"].Rows.Count > 0)
					{
						foreach (DataRow dr in ds.Tables["audio"].Rows)
						{
							audio.Add(new UserAudio(dr));
						}
					}
					return audio;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает текст аудиозаписи по параметру lyrics_id
		/// </summary>
		/// <param name="lyricsId">id текста аудиозаписи</param>
		/// <returns></returns>
		public string AudioGetLyrics(long lyricsId)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Audio)) != 0)
			{
				XmlDocument doc = ExecuteMethodByToken("audio.getLyrics", new List<VKParameter>() { new VKParameter("lyrics_id", lyricsId.ToString()) });
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text/text()").Value;
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Удаляет аудиозапись со страницы пользователя или группы. 
		/// </summary>
		/// <param name="aid"></param>
		/// <param name="oid">id владельца аудиозаписи. Если удаляемая аудиозапись находится на странице группы, в этом параметре должно стоять значение, равное -id группы. </param>
		/// <returns></returns>
		public bool AudioDelete(long aid, long oid)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Audio)) != 0)
			{
				XmlDocument doc = ExecuteMethodByToken("audio.delete", new List<VKParameter>() { new VKParameter("aid", aid.ToString()),
				new VKParameter("oid",oid.ToString())});
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text()").Value == "1";
				}
				return false;
			}
			return false;
		}

		#endregion

		#region Work with messages

		/// <summary>
		/// Посылает сообщение пользователю
		/// </summary>
		/// <param name="uid">Ид пользователя</param>
		/// <param name="message">текст сообщения</param>
		/// <param name="title">Заголовок</param>
		/// <param name="isFromChat">Является ли данное сообщение сообщением из чата?</param>
		/// <returns>Возвращает id сообщения</returns>
		public long SendUserMessage(string uid, string message, string title, bool isFromChat)
		{
			if (IsConnected && (Settings & (int)VKSettings.ExMessages) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("uid", uid));
				if (!string.IsNullOrEmpty(message))
					param.Add(new VKParameter("message", message));
				if (!string.IsNullOrEmpty(message))
					param.Add(new VKParameter("title", title));
				if (isFromChat)
				{
					param.Add(new VKParameter("type", "1"));
				}
				else
				{
					param.Add(new VKParameter("type", "0"));
				}
				XmlDocument doc = ExecuteMethodByToken("messages.send", param);
				if (!HasError(doc))
				{
					return Int64.Parse(doc.ToXDocument().Element("response").Value);
				}
				else
				{
					return 0;
				}
			}
			return 0;
		}

		/// <summary>
		/// Возвращает список найденных личных сообщений текущего пользователя по введенной строке поиска.
		/// </summary>
		/// <param name="findtext">подстрока, по которой будет производиться поиск.</param>
		/// <param name="offset">смещение, необходимое для выборки определенного подмножества сообщений из списка найденных. </param>
		/// <param name="count">количество сообщений, которое необходимо получить (но не более 100). </param>
		/// <returns></returns>
		public List<UserMessage> SearchMessage(string findtext, string offset, string count)
		{
			if (IsConnected && (Settings & (int)VKSettings.ExMessages) != 0)
			{
				List<VKParameter> parameters = new List<VKParameter>();
				if (!string.IsNullOrEmpty(findtext))
					parameters.Add(new VKParameter("q", findtext));
				if (!string.IsNullOrEmpty(offset))
					parameters.Add(new VKParameter("offset", offset));

				if (!string.IsNullOrEmpty(count))
					parameters.Add(new VKParameter("count", count));

				XmlDocument doc = ExecuteMethodByToken("messages.search", parameters);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("tempDS");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<UserMessage> messages = new List<UserMessage>();
					if (ds.Tables["message"] != null && ds.Tables["message"].Rows.Count > 0)
					{
						foreach (DataRow dr in ds.Tables["message"].Rows)
						{
							messages.Add(new UserMessage(dr));
						}
					}
					return messages;
				}
				else
				{
					return null;
				}

			}
			return null;
		}

		/// <summary>
		/// Получение списка сообщений
		/// </summary>
		/// <param name="mout">если этот параметр равен 1, сервер вернет исходящие сообщения</param>
		/// <param name="offset">смещение, необходимое для выборки определенного подмножества сообщений</param>
		/// <param name="count">количество сообщений, которое необходимо получить (но не более 100). </param>
		/// <param name="filter">Фильтр сообщений</param>
		/// <param name="previewLength">Количество симоволов по которому нужно обрезать сообщение.(по умолчанию 90).</param>
		/// <param name="timeoffset">Макисмальное время прошедшее с момента отправки сообщения до текущего момента в секундах. 0, если Вы хотите получить сообщения любой давности.</param>
		/// <returns>Список сообщений</returns>
		public List<UserMessage> GetUserMessages(string mout, string offset, string count, VkMessageFilter filter, string previewLength, string timeoffset)
		{
			if (IsConnected && (Settings & (int)VKSettings.ExMessages) != 0)
			{
				List<VKParameter> parameters = new List<VKParameter>();
				if (!string.IsNullOrEmpty(mout))
					parameters.Add(new VKParameter("out", mout));
				if (!string.IsNullOrEmpty(offset))
					parameters.Add(new VKParameter("offset", offset));

				if (!string.IsNullOrEmpty(count))
					parameters.Add(new VKParameter("count", count));

				if (!string.IsNullOrEmpty(ProcessMessageFilter(filter)))
					parameters.Add(new VKParameter("filter", ProcessMessageFilter(filter)));

				if (!string.IsNullOrEmpty(previewLength))
					parameters.Add(new VKParameter("preview_length", previewLength));

				if (!string.IsNullOrEmpty(timeoffset))
					parameters.Add(new VKParameter("time_offset", timeoffset));

				XmlDocument doc = ExecuteMethodByToken("messages.get", parameters);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("tempDS");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<UserMessage> messages = null;
					if (ds.Tables["message"] != null && ds.Tables["message"].Rows.Count > 0)
					{
						messages = new List<UserMessage>(ds.Tables["message"].Rows.Count);
						foreach (DataRow dr in ds.Tables["message"].Rows)
						{
							if (ds.Relations.Contains("message_attachment"))
							{
								DataRow[] rows = dr.GetChildRows(ds.Relations["message_attachment"]);
								if (rows != null && rows.Length > 0)
								{
									messages.Add(new UserMessage(dr, rows[0]));
								}
							}
							messages.Add(new UserMessage(dr));
						}
					}
					return messages;
				}
				else return null;
			}
			return null;
		}

		/// <summary>
		/// Помечает сообщения, как прочитанные. 
		/// </summary>
		/// <param name="mids">список идентификаторов сообщений</param>
		/// <returns>true если помечено</returns>
		public bool MarkMessagesAsRead(List<string> mids)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExMessages)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("mids", this.GetStringFromList(mids)));
				XmlDocument doc = ExecuteMethodByToken("messages.markAsRead", param);
				if (!HasError(doc))
				{
					return Extension.FromStringToBool(doc.SelectSingleNode("response/text()").Value);
				}
				return false;
			}
			return false;
		}

		/// <summary>
		/// Помечает сообщение, как прочитанное. 
		/// </summary>
		/// <param name="mids">идентификатор сообщения</param>
		/// <returns>true если помечено</returns>
		public bool MarkMessageAsRead(string mid)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExMessages)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("mids", mid));
				XmlDocument doc = ExecuteMethodByToken("messages.markAsRead", param);
				if (!HasError(doc))
				{
					return Extension.FromStringToBool(doc.SelectSingleNode("response/text()").Value);
				}
				return false;
			}
			return false;
		}

		/// <summary>
		/// Помечает сообщения, как непрочитанные. 
		/// </summary>
		/// <param name="mids">список идентификаторов сообщений</param>
		/// <returns>true если помечено</returns>
		public bool MarkMessagesAsNew(List<string> mids)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExMessages)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("mids", this.GetStringFromList(mids)));
				XmlDocument doc = ExecuteMethodByToken("messages.markAsNew", param);
				if (!HasError(doc))
				{
					return Extension.FromStringToBool(doc.SelectSingleNode("response/text()").Value);
				}
				return false;
			}
			return false;
		}

		/// <summary>
		/// ППомечает сообщениe, как непрочитанное.  
		/// </summary>
		/// <param name="mids">идентификатор сообщения</param>
		/// <returns>true если помечено</returns>
		public bool MarkMessageAsNew(string mid)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExMessages)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("mids", mid));
				XmlDocument doc = ExecuteMethodByToken("messages.markAsNew", param);
				if (!HasError(doc))
				{
					return Extension.FromStringToBool(doc.SelectSingleNode("response/text()").Value);
				}
				return false;
			}
			return false;
		}

		/// <summary>
		/// Удаляет сообщение. 
		/// </summary>
		/// <param name="messageId">идентификатор сообщения.</param>
		/// <returns>Возвращает true в случае успешного удаления</returns>
		public bool DeleteMessage(string messageId)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExMessages)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("mid", messageId));
				XmlDocument doc = ExecuteMethodByToken("messages.delete", param);
				if (!HasError(doc))
				{
					return Extension.FromStringToBool(doc.SelectSingleNode("response/text()").Value);
				}
				return false;
			}
			return false;
		}

		/// <summary>
		/// Возвращает историю сообщений для данного пользователя. 
		/// </summary>
		/// <param name="userId">идентификатор пользователя, историю переписки с которым необходимо вернуть</param>
		/// <param name="offset">смещение, необходимое для выборки определенного подмножества сообщений. </param>
		/// <param name="count">количество сообщений, которое необходимо получить (но не более 100). </param>
		/// <returns></returns>
		public List<MessageHistory> GetMessageHistory(string userId, string offset, string count)
		{
			if (IsConnected && (Settings & (int)VKSettings.ExMessages) != 0)
			{
				List<VKParameter> parameters = new List<VKParameter>();
				if (!userId.IsNullOrEmpty())
					parameters.Add(new VKParameter("uid", userId));
				if (!string.IsNullOrEmpty(offset))
					parameters.Add(new VKParameter("offset", offset));

				if (!string.IsNullOrEmpty(count))
					parameters.Add(new VKParameter("count", count));

				XmlDocument doc = ExecuteMethodByToken("messages.getHistory", parameters);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("tempDS");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<MessageHistory> messages = new List<MessageHistory>();
					if (ds.Tables["message"] != null && (ds.Tables["message"].Rows.Count > 0))
					{
						foreach (DataRow dr in ds.Tables["message"].Rows)
						{
							messages.Add(new MessageHistory(dr));
						}
					}
					return messages;
				}
				else
				{
					return null;
				}

			}
			return null;
		}

		public List<UserMessage> GetMessagesDialogs(int offset = 0, int count = 100, int previewLength = 0)
		{
			if (IsConnected && (Settings & (int)VKSettings.ExMessages) != 0)
			{
				List<VKParameter> parameters = new List<VKParameter>();
				parameters.Add(new VKParameter("offset", offset.ToString()));
				parameters.Add(new VKParameter("count", count.ToString()));
				parameters.Add(new VKParameter("preview_length", previewLength.ToString()));

				XmlDocument doc = ExecuteMethodByToken("messages.getDialogs", parameters);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet("tempDS");
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);
					List<UserMessage> messages = new List<UserMessage>();
					if (ds.Tables["message"] != null && (ds.Tables["message"].Rows.Count > 0))
					{
						foreach (DataRow dr in ds.Tables["message"].Rows)
						{
							messages.Add(new UserMessage(dr));
						}
					}
					return messages;
				}
				else
				{
					return null;
				}

			}
			return null;
		}

		#endregion

		#region Work with status
		/// <summary>
		/// Получаем статус пользователя
		/// </summary>
		/// <param name="uid">id пользователя (если не указан,то текущий)</param>
		/// <returns>Текст статуса</returns>
		public string GetUserStatus(string uid)
		{
			if (IsConnected && (Settings & (int)VKSettings.Status) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("uid", uid));

				XmlDocument doc = ExecuteMethodByToken("status.get", param);
				if (!HasError(doc))
				{
					return doc.ToXDocument().Element("response").Element("text").Value.ToString();
				}
				else
				{
					return string.Empty;
				}
			}
			return null;
		}

		/// <summary>
		/// Устанавливает статус пользователя
		/// </summary>
		///<param name="text">Текст статуса</param>
		/// <returns>Текст статуса</returns>
		public bool SetUserStatus(string text)
		{
			if (IsConnected && (Settings & (int)VKSettings.Status) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(text))
					param.Add(new VKParameter("text", text));

				XmlDocument doc = ExecuteMethodByToken("status.set", param);
				if (!HasError(doc))
				{
					return Extension.FromStringToBool(doc.ToXDocument().Element("response").Value.ToString());
				}
				else
				{
					return false;
				}
			}
			return false;
		}
		#endregion

		#region Subscription

		/// <summary>
		/// Возвращает список идентификаторов пользователей, которые являются подписчиками текущего пользователя.
		/// </summary>
		/// <param name="uid">Если параметр не задан, то считается, что он равен идентификатору текущего пользователя</param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public List<string> GetSubscriptionFollowers(string uid, int offset = 0, int count = 100)
		{
			if (this.IsConnected && (Settings & (int)VKSettings.FreindsList) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();

				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("uid", uid));

				param.Add(new VKParameter("offset", offset.ToString()));
				param.Add(new VKParameter("count", count.ToString()));


				XmlDocument doc = ExecuteMethodByToken("subscriptions.getFollowers", param);
				if (!HasError(doc))
				{
					if (doc.SelectSingleNode("response/users").Attributes["list"].Value.ToLower() != "true")
						return null;
					return (from item in doc.ToXDocument().Element("response").Element("users").Elements()
							where item.Name.ToString().ToLower() == "uid"
							select item.Value.ToString()).ToList();
				}
				else
				{
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Возвращает список идентификаторов пользователей, которые входят в список подписок текущего пользователя
		/// </summary>
		/// <param name="uid">Если параметр не задан, то считается, что он равен идентификатору текущего пользователя.</param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public List<string> GetSubscriptions(string uid, int offset = 0, int count = 100)
		{
			if (this.IsConnected && (Settings & (int)VKSettings.FreindsList) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();

				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("uid", uid));

				param.Add(new VKParameter("offset", offset.ToString()));
				param.Add(new VKParameter("count", count.ToString()));


				XmlDocument doc = ExecuteMethodByToken("subscriptions.get", param);
				if (!HasError(doc))
				{
					return (from item in doc.ToXDocument().Element("response").Element("users").Elements()
							where item.Name.ToString().ToLower() == "uid"
							select item.Value.ToString()).ToList();
				}
				else
				{
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Добавляет указанного пользователя в список подписок текущего пользователя
		/// </summary>
		/// <param name="uid"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public bool SubscriptionsFollow(string uid, int offset = 0, int count = 100)
		{
			if (this.IsConnected && (Settings & (int)VKSettings.FreindsList) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();

				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("uid", uid));

				XmlDocument doc = ExecuteMethodByToken("subscriptions.follow", param);
				if (!HasError(doc))
				{
					return doc.ToXDocument().Element("response").Value == "1" ? true : false;
				}
				else
				{
					return false;
				}
			}
			return false;
		}

		/// <summary>
		/// Удаляет указанного пользователя из списка подписок текущего пользователя
		/// </summary>
		/// <param name="uid"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public bool SubscriptionsUnfollow(string uid)
		{
			if (this.IsConnected && (Settings & (int)VKSettings.FreindsList) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();

				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("uid", uid));

				XmlDocument doc = ExecuteMethodByToken("subscriptions.unfollow", param);
				if (!HasError(doc))
				{
					return doc.ToXDocument().Element("response").Value == "1" ? true : false;
				}
				else
				{
					return false;
				}
			}
			return false;
		}

		#endregion

		#region LongPoolServer

		/// <summary>
		/// Возвращает данные, необходимые для подключения к Long Poll серверу
		/// <remarks>Long Poll подключение позволит Вам моментально узнавать о приходе новых сообщений и других событий. </remarks>
		/// </summary>
		/// <returns></returns>
		public LongPollServerInfo GetLongPollServerConnetInfo()
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExMessages)) != 0)
			{
				XmlDocument doc = ExecuteMethodByToken("messages.getLongPollServer", null);
				if (!HasError(doc))
				{
					LongPollServerInfo server = new LongPollServerInfo();
					server.Key = doc.SelectSingleNode("response/key/text()").Value;
					server.Server = doc.SelectSingleNode("response/server/text()").Value;
					long ts;
					if (long.TryParse(doc.SelectSingleNode("response/ts/text()").Value, out ts))
						server.Ts = ts;
					return server;
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// подключиться к Long Poll серверу
		/// </summary>
		/// <param name="serverData"></param>
		/// <returns>При каждом ответе сервер будет возвращать новый ts</returns>
		public string ConnectToLongPoolServer(LongPollServerInfo serverData)
		{
			WebClient wc = new WebClient();
			string connection = string.Format("http://{0}?act=a_check&key={1}&ts={2}&wait=25", serverData.Server,
											serverData.Key, serverData.Ts);

			byte[] buff = wc.DownloadData(connection);
			string ret_string = Encoding.UTF8.GetString(buff);
			return ret_string;
		}

		public string ReconnectToLongPoolServer(LongPollServerInfo serverData, long newTs)
		{
			WebClient wc = new WebClient();
			string connection = string.Format("http://{0}?act=a_check&key={1}&ts={2}&wait=25", serverData.Server,
											serverData.Key, newTs);

			byte[] buff = wc.DownloadData(connection);
			string ret_string = Encoding.UTF8.GetString(buff);
			return ret_string;
		}

		public LongPollServerError ParseError(string responseError)
		{
			int message = Int32.Parse(responseError.Replace('{', ' ').Replace('}', ' ').Trim().Split(':')[0]);
			return (LongPollServerError)message;
		}

		#endregion

		#region Wall region

		public List<PostInfo> WallGetPosts(string ownerId, int offset = 0, int count = 100, string filter = "all")
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExWall)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(ownerId))
					param.Add(new VKParameter("owner_id", ownerId));

				param.Add(new VKParameter("offset", offset.ToString()));
				param.Add(new VKParameter("count", count.ToString()));
				param.Add(new VKParameter("filter", filter));

				XmlDocument doc = ExecuteMethodByToken("wall.get", param);
				if (!HasError(doc))
				{
					XmlNodeList postList = doc.SelectNodes("response/post");
					if (postList != null)
					{
						List<PostInfo> postsInfo = new List<PostInfo>();
						foreach (XmlNode postNode in postList)
						{
							PostInfo post = new PostInfo();
							post.ParseXml(postNode);
							postsInfo.Add(post);
						}
						return postsInfo;
					}
					return null;
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Публикует новую запись на своей или чужой стене
		/// </summary>
		/// <param name="ownerId"></param>
		/// <param name="message"></param>
		/// <param name="attachment"> "<type><owner_id>_<media_id>" photo100172_166443618</param>
		/// <param name="services"></param>
		/// <returns>post_id</returns>
		public Nullable<int> WallPost(string ownerId, string message, string attachment = null, string services = null)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExWall)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();

				if (!string.IsNullOrEmpty(ownerId))
					param.Add(new VKParameter("owner_id", ownerId));
				if (!string.IsNullOrEmpty(message))
					param.Add(new VKParameter("message", message));
				if (!string.IsNullOrEmpty(attachment))
					param.Add(new VKParameter("attachment", attachment));
				if (!string.IsNullOrEmpty(services))
					param.Add(new VKParameter("services", services));

				XmlDocument doc = ExecuteMethodByToken("wall.post", param);
				if (!HasError(doc))
				{
					return Int32.Parse(doc.SelectSingleNode("response/post_id/text()").Value.Trim());
				}
				return null;
			}
			return null;
		}

		public List<UserComment> GetWallComments(string ownerId, int postId, string sort = "asc", int offset = 0, int count = 100)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExWall)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(ownerId))
					param.Add(new VKParameter("owner_id", ownerId));
				param.Add(new VKParameter("post_id", postId.ToString()));
				param.Add(new VKParameter("sort", sort));
				param.Add(new VKParameter("offset", offset.ToString()));
				param.Add(new VKParameter("count", count.ToString()));

				XmlDocument doc = ExecuteMethodByToken("wall.getComments", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);

					List<UserComment> comments = new List<UserComment>();
					foreach (DataRow dr in ds.Tables["comment"].Rows)
					{
						comments.Add(new UserComment(dr));
					}
					return comments;
				}
				return null;
			}
			return null;
		}

		public Nullable<int> WallAddComment(string ownerId, int postId, string text, int? reply_to_cid)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExWall)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(ownerId))
					param.Add(new VKParameter("owner_id", ownerId));
				param.Add(new VKParameter("post_id", postId.ToString()));
				param.Add(new VKParameter("text", text));
				if (reply_to_cid.HasValue)
					param.Add(new VKParameter("count", reply_to_cid.Value.ToString()));

				XmlDocument doc = ExecuteMethodByToken("wall.addComment", param);
				if (!HasError(doc))
				{
					return Int32.Parse(doc.SelectSingleNode("response/cid/text()").Value.Trim());
				}
				return null;
			}
			return null;
		}

		public bool WallDeleteComment(string ownerId, int cid)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExWall)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(ownerId))
					param.Add(new VKParameter("owner_id", ownerId));
				param.Add(new VKParameter("cid", cid.ToString()));

				XmlDocument doc = ExecuteMethodByToken("wall.deleteComment", param);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text()").Value == "1";
				}
				return false;
			}
			return false;
		}

		public bool WallRestoreComment(string ownerId, int cid)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExWall)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(ownerId))
					param.Add(new VKParameter("owner_id", ownerId));
				param.Add(new VKParameter("cid", cid.ToString()));

				XmlDocument doc = ExecuteMethodByToken("wall.restoreComment", param);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text()").Value == "1";
				}
				return false;
			}
			return false;
		}

		public Nullable<int> WallDeleteLike(string ownerId, int postId)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExWall)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(ownerId))
					param.Add(new VKParameter("owner_id", ownerId));
				param.Add(new VKParameter("post_id", postId.ToString()));

				XmlDocument doc = ExecuteMethodByToken("wall.deleteLike", param);
				if (!HasError(doc))
				{
					return Int32.Parse(doc.SelectSingleNode("response/likes/text()").Value.Trim());
				}
				return null;
			}
			return null;
		}

		public Nullable<int> WallAddLike(string ownerId, int postId, bool? needPublish)
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExWall)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(ownerId))
					param.Add(new VKParameter("owner_id", ownerId));
				param.Add(new VKParameter("post_id", postId.ToString()));
				if (needPublish.HasValue)
					param.Add(new VKParameter("need_publish", needPublish == true ? "1" : "0"));

				XmlDocument doc = ExecuteMethodByToken("wall.deleteLike", param);
				if (!HasError(doc))
				{
					return Int32.Parse(doc.SelectSingleNode("response/likes/text()").Value.Trim());
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Возвращает адрес сервера для загрузки фотографии на стену пользователя.
		/// </summary>
		/// <returns>Возвращает объект с единственным полем upload_url.</returns>
		public string WallGetPhotoUploadServer()
		{
			if (IsConnected && (Settings & ((int)VKSettings.ExWall)) != 0)
			{
				XmlDocument doc = ExecuteMethodByToken("wall.getPhotoUploadServer", null);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/upload_url/text()").Value.Trim();
				}
				return null;
			}
			return null;
		}

		#endregion

		#region Notes

		public List<UserNote> NotesGet(string Uid, List<string> nids, int sort = 0, int count = 30, int offset = 0)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Notes)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (!string.IsNullOrEmpty(Uid))
					param.Add(new VKParameter("uid", Uid));

				if (nids != null && nids.Count > 0)
					param.Add(new VKParameter("nids", GetStringFromList(nids)));

				param.Add(new VKParameter("sort", sort.ToString()));
				param.Add(new VKParameter("count", count.ToString()));
				param.Add(new VKParameter("offset", offset.ToString()));


				XmlDocument doc = ExecuteMethodByToken("notes.get", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);

					List<UserNote> comments = new List<UserNote>();
					foreach (DataRow dr in ds.Tables["note"].Rows)
					{
						comments.Add(new UserNote(dr));
					}
					return comments;
				}
				return null;
			}
			return null;
		}

		public bool NotesDelete(long noteId)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Notes)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("nid", noteId.ToString()));

				XmlDocument doc = ExecuteMethodByToken("notes.delete", param);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text()").Value == "1";
				}
				return false;
			}
			return false;
		}

		/// <summary>
		/// Создает новую заметку у текущего пользователя.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="text"></param>
		/// <param name="privacy">уровень доступа к заметке. Значения: 0 – все пользователи, 1 – только друзья, 2 – друзья и друзья друзей, 3 - только пользователь.</param>
		/// <param name="commentPrivacy"></param>
		/// <returns></returns>
		public Nullable<long> NotesAdd(string title, string text, int privacy = 1, int commentPrivacy = 1)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Notes)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("title", title));
				param.Add(new VKParameter("text", text));
				param.Add(new VKParameter("privacy", privacy.ToString()));
				param.Add(new VKParameter("comment_privacy", commentPrivacy.ToString()));

				XmlDocument doc = ExecuteMethodByToken("notes.add", param);
				if (!HasError(doc))
				{
					return long.Parse(doc.SelectSingleNode("response/nid/text()").Value);
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Редактирует заметку текущего пользователя. 
		/// </summary>
		/// <param name="noteId"></param>
		/// <param name="title"></param>
		/// <param name="text"></param>
		/// <param name="privacy">уровень доступа к заметке. Значения: 0 – все пользователи, 1 – только друзья, 2 – друзья и друзья друзей, 3 - только пользователь.</param>
		/// <param name="commentPrivacy"></param>
		/// <returns></returns>
		public bool NotesEdit(long noteId, string title, string text, Nullable<int> privacy, Nullable<int> commentPrivacy)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Notes)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("nid", noteId.ToString()));
				param.Add(new VKParameter("title", title));
				param.Add(new VKParameter("text", text));
				if (privacy.HasValue)
					param.Add(new VKParameter("privacy", privacy.Value.ToString()));
				if (commentPrivacy.HasValue)
					param.Add(new VKParameter("comment_privacy", commentPrivacy.Value.ToString()));

				XmlDocument doc = ExecuteMethodByToken("notes.edit", param);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text()").Value == "1";
				}
				return false;
			}
			return false;
		}

		/// <summary>
		/// Возвращает список заметок, друзей пользователя.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public List<UserNote> NotesGetFriends(int count = 60, int offset = 0)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Notes)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("count", count.ToString()));
				param.Add(new VKParameter("offset", offset.ToString()));

				XmlDocument doc = ExecuteMethodByToken("notes.getFriendsNotes", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);

					List<UserNote> comments = new List<UserNote>();
					foreach (DataRow dr in ds.Tables["note"].Rows)
					{
						comments.Add(new UserNote(dr));
					}
					return comments;
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Возращает список комментариев к заметке. 
		/// </summary>
		/// <param name="nid">id заметки, комментарии которой нужно вернуть</param>
		/// <param name="uid">идентификатор пользователя (по умолчанию - текущий пользователь). </param>
		/// <param name="sort">сортировка результатов (0 - по дате добавления в порядке возрастания, 1 - по дате добавления в порядке убывания).</param>
		/// <param name="count"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public List<UserComment> NotesGetComments(long nid, string uid, int sort = 0, int count = 40, int offset = 0)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Notes)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("nid", nid.ToString()));
				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("uid", uid));
				param.Add(new VKParameter("sort", sort.ToString()));
				param.Add(new VKParameter("count", count.ToString()));
				param.Add(new VKParameter("offset", offset.ToString()));

				XmlDocument doc = ExecuteMethodByToken("notes.getFriendsNotes", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);

					List<UserComment> comments = new List<UserComment>();
					foreach (DataRow dr in ds.Tables["comment"].Rows)
					{
						comments.Add(new UserComment(dr));
					}
					return comments;
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Добавляет новый комментарий к заметке. 
		/// </summary>
		/// <param name="nid"></param>
		/// <param name="uid">идентификатор пользователя, владельца заметки (по умолчанию - текущий пользователь).</param>
		/// <param name="repToId">id пользователя, ответом на комментарий которого является добавляемый комментарий (не передаётся если комментарий не является ответом).</param>
		/// <param name="message">(минимальная длина - 2 символа). </param>
		/// <returns></returns>
		public Nullable<long> NotesAddComment(long nid, string uid, Nullable<long> repToId, string message)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Notes)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("nid", nid.ToString()));
				if (!string.IsNullOrEmpty(uid))
					param.Add(new VKParameter("owner_id", uid));
				if (repToId.HasValue)
					param.Add(new VKParameter("reply_to", repToId.Value.ToString()));

				param.Add(new VKParameter("message", message));

				XmlDocument doc = ExecuteMethodByToken("notes.createComment", param);
				if (!HasError(doc))
				{
					return long.Parse(doc.SelectSingleNode("response/text()").Value);
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Удаляет комментарий. 
		/// </summary>
		/// <param name="cid">id комментария, котороый нужно удалить</param>
		/// <param name="ownerId"></param>
		/// <returns></returns>
		public bool NotesDeleteComment(long cid, string ownerId)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Notes)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("cid", cid.ToString()));
				if (!string.IsNullOrEmpty(ownerId))
					param.Add(new VKParameter("owner_id", ownerId));

				XmlDocument doc = ExecuteMethodByToken("notes.deleteComment", param);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text()").Value == "1";
				}
				return false;
			}
			return false;
		}

		#endregion

		#region Documents
		/// <summary>
		/// Возвращает расширенную информацию о документах текущего пользователя
		/// </summary>
		/// <param name="uid">id пользователя или группы, документы которого нужно вернуть. По умолчанию – id текущего пользователя. Если необходимо получить документы группы, в этом параметре должно стоять значение, равное -id группы. </param>
		/// <param name="count"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public List<UserDocument> DocumentsGet(long? uid, int? count, int offset = 0)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Documents)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				if (uid.HasValue)
					param.Add(new VKParameter("oid", uid.Value.ToString()));
				if (count.HasValue)
					param.Add(new VKParameter("count", count.Value.ToString()));

				param.Add(new VKParameter("offset", offset.ToString()));

				XmlDocument doc = ExecuteMethodByToken("docs.get", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);

					List<UserDocument> docs = new List<UserDocument>();
					foreach (DataRow dr in ds.Tables["doc"].Rows)
					{
						docs.Add(new UserDocument(dr));
					}
					return docs;
				}
				return null;
			}
			return null;
		}

		/// <summary>
		/// Удаляет документ пользователя или группы
		/// </summary>
		/// <param name="did"></param>
		/// <param name="oid"></param>
		/// <returns></returns>
		public bool DocumentDelete(long did, long oid, bool isGroup = false)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Documents)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("did", did.ToString()));
				if (isGroup)
					param.Add(new VKParameter("oid", string.Format("-{0}", oid)));
				else
					param.Add(new VKParameter("oid", oid.ToString()));


				XmlDocument doc = ExecuteMethodByToken("docs.delete", param);
				if (!HasError(doc))
				{
					return doc.SelectSingleNode("response/text()").Value == "1";
				}
				return false;
			}
			return false;
		}

		/// <summary>
		/// Возвращает информацию о документах. 
		/// </summary>
		/// <param name="docs">перечисленные через запятую идентификаторы – идущие через знак подчеркивания id пользователей, которым принадлежат документы, и id самих документов</param>
		/// <returns></returns>
		public List<UserDocument> DocumentsGetyId(List<string> docs)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Documents)) != 0 && docs != null)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("docs", this.GetStringFromList(docs)));

				XmlDocument doc = ExecuteMethodByToken("docs.getById", param);
				if (!HasError(doc))
				{
					DataSet ds = new DataSet();
					StringReader sr = new StringReader(doc.InnerXml);
					ds.ReadXml(sr, XmlReadMode.InferSchema);

					List<UserDocument> docList = new List<UserDocument>();
					foreach (DataRow dr in ds.Tables["doc"].Rows)
					{
						docList.Add(new UserDocument(dr));
					}
					return docList;
				}
				return null;
			}
			return null;
		}
		#endregion

		#region Places

		public UserPlace PlacesGetCountryById(int id)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Notes)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("cids", id.ToString()));

				XmlDocument doc = ExecuteMethodByToken("places.getCountryById", param);
				if (!HasError(doc))
				{
					UserPlace place = new UserPlace();
					place.Id = Int32.Parse(doc.SelectSingleNode("response/country/cid()").Value);
					place.Name = doc.SelectSingleNode("response/country/name()").Value;
					return place;
				}
				return null;
			}
			return null;
		}

		public UserPlace PlacesGetCityById(int id)
		{
			if (IsConnected && (Settings & ((int)VKSettings.Notes)) != 0)
			{
				List<VKParameter> param = new List<VKParameter>();
				param.Add(new VKParameter("cids", id.ToString()));

				XmlDocument doc = ExecuteMethodByToken("places.getCityById", param);
				if (!HasError(doc))
				{
					UserPlace place = new UserPlace();
					place.Id = Int32.Parse(doc.SelectSingleNode("response/city/cid()").Value);
					place.Name = doc.SelectSingleNode("response/city/name()").Value;
					return place;
				}
				return null;
			}
			return null;
		}
		#endregion

		#region Helper methods
		/// <summary>
		/// Формирует строку параметров
		/// </summary>
		/// <param name="online"></param>
		/// <returns></returns>
		private string GetUidsString(List<OnlineFriends> online)
		{
			if (online != null && online.Count > 0)
			{
				bool isFirstIter = true;
				StringBuilder sb = new StringBuilder();
				foreach (OnlineFriends friend in online)
				{
					if (isFirstIter)
					{
						sb.Append(friend.UId.ToString());
					}
					else
					{
						sb.Append(string.Format(",{0}", friend.UId.ToString()));
					}
					isFirstIter = false;
				}
				return sb.ToString();
			}
			else
				return string.Empty;
		}

		/// <summary>
		/// Формируем строку из списка
		/// </summary>
		/// <param name="parameters">список параметров</param>
		/// <returns></returns>
		private string GetStringFromList(List<string> parameters)
		{
			bool first = true;
			StringBuilder sb = new StringBuilder();
			foreach (var item in parameters)
			{
				if (first)
				{
					sb.Append(item);
				}
				else
				{
					sb.Append(string.Format(",{0}", item));
				}
				first = false;
			}
			return sb.ToString();
		}

		private string ProcessMessageFilter(VkMessageFilter filter)
		{
			string response = string.Empty;
			switch (filter)
			{
				case VkMessageFilter.NotChat:
					response = "2";
					break;
				case VkMessageFilter.NeverReaded:
					response = "1";
					break;

				case VkMessageFilter.FromFriends:
					response = "4";
					break;

				default:
					return string.Empty;
			}
			return response;
		}
		#endregion
	}

	public enum LongPollServerError
	{
		None = 0,
		/// <summary>
		/// Сообщение не прочитано
		/// </summary>
		Unread = 1,
		/// <summary>
		/// Исходящее сообщение
		/// </summary>
		Outbox = 2,
		/// <summary>
		/// на сообщение был создан ответ
		/// </summary>
		Replied = 4,
		/// <summary>
		/// помеченное сообщение
		/// </summary>
		Important = 8,
		/// <summary>
		/// сообщение отправлено через чат
		/// </summary>
		Chat = 16,
		/// <summary>
		/// сообщение отправлено другом 
		/// </summary>
		Friends = 32,
		/// <summary>
		/// сообщение помечено как "Спам" 
		/// </summary>
		Spam = 64,
		/// <summary>
		/// сообщение удалено (в корзине) 
		/// </summary>
		Deleted = 128,
		/// <summary>
		/// сообщение проверено пользователем на спам 
		/// </summary>
		Fixed = 256,
		/// <summary>
		/// сообщение содержит медиаконтент 
		/// </summary>
		Media = 512
	}
}