using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;

namespace WpfVkontacteClient
{
	public static class VkHelpers
	{
		public static VKToken GetAccessToken(string appId, string appSecret)
		{
			string grant_type = "client_credentials";
			string authUrl = "https://oauth.vk.com/access_token?client_id={0}&client_secret={1}&grant_type={2}";
			//string reqStrTemplate = "http://api.vkontakte.ru/oauth/authorize?client_id={0}&scope=offline,wall";

			var responseStr = GetRequest(string.Format(authUrl, appId, appSecret, grant_type));
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return serializer.Deserialize<VKToken>(responseStr);
		}

		public static VKToken GetToken(String app_id, String app_secret, String code)
		{
			var url = GetAccessTokenUrl(app_id, app_secret, code);
			var responseStr = GetRequest(url);
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return serializer.Deserialize<VKToken>(responseStr);
		}

		public static string GetRequest(string url)
		{
			WebRequest wr = WebRequest.Create(url);
			using (Stream objStream = wr.GetResponse().GetResponseStream())
			{
				StreamReader objReader = new StreamReader(objStream);

				StringBuilder sb = new StringBuilder();
				string line = "";
				while (true)
				{
					line = objReader.ReadLine();
					if (line != null) sb.Append(line);

					else
						return sb.ToString();
				}
			}
		}

		public static string GetLoginUrl(this String app_id, VkAuthSettingsBuilder scope, string back_url, VkDisplay display, VkResponseType code = VkResponseType.code)
		{
			return String.Format(@"http://oauth.vk.com/authorize?client_id={0}&scope={1}&redirect_uri={2}&response_type={3}", app_id, scope, back_url, code, display);
		}

		public static string GetAccessTokenUrl(String app_id, String app_secret, String code)
		{
			return String.Format(@"https://oauth.vk.com/access_token?client_id={0}&client_secret={1}&code={2}", app_id, app_secret, code);
		}

		//public static string GetApiUrl(string app_id, string method, string sig, )
		//{
		//    return String.Format(@"http://api.vk.com/api.php?api_id={0}&method={1}&sig={2}&v={3}&format=json&sid={4}"
		//}
	}

	[DataContract]
	public class VKToken
	{
		private string _access_token;

		[DataMember]
		public string access_token
		{
			get { return _access_token; }
			set { _access_token = value; }
		}

		private int _expires_in;

		[DataMember]
		public int expires_in
		{
			get { return _expires_in; }
			set { _expires_in = value; }
		}

		private int _user_id;

		[DataMember]
		public int user_id
		{
			get { return _user_id; }
			set { _user_id = value; }
		}
	}

	public enum VkResponseType
	{
		code,
		token
	}

	public enum VkDisplay
	{
		page,// – форма авторизации в отдельном окне
		popup,// – всплывающее окно
		touch,// – авторизация для мобильных Touch-устройств
		wap// – авторизация для мобильных устройств с маленьким экраном или без поддержки Javascript
	}

	public enum VkAuthSetting
	{
		notify,//	Пользователь разрешил отправлять ему уведомления.
		friends,//	Доступ к друзьям.
		photos,//	Доступ к фотографиям.
		audio,//	Доступ к аудиозаписям.
		video,//	Доступ к видеозаписям.
		docs,//	Доступ к документам.
		notes,//	Доступ заметкам пользователя.
		pages,//	Доступ к wiki-страницам.
		offers,//	Доступ к предложениям (устаревшие методы).
		questions,//	Доступ к вопросам (устаревшие методы).
		wall,//	Доступ к обычным и расширенным методам работы со стеной.
		groups,//	Доступ к группам пользователя.
		messages,//	(для Standalone-приложений) Доступ к расширенным методам работы с сообщениями.
		notifications,//	Доступ к оповещениям об ответах пользователю.
		ads,//	Доступ к расширенным методам работы с рекламным API.
		offline,//	Доступ к API в любое время со стороннего сервера.
		nohttps//	Возможность осуществлять запросы к API без HTTPS.
	}

	public class VkAuthSettingsBuilder
	{
		private SortedSet<string> value = new SortedSet<string>();

		public override string ToString()
		{
			return value.Aggregate((current, next) => current.ToString() + ", " + next.ToString());
		}

		public VkAuthSettingsBuilder Add(VkAuthSetting set)
		{
			value.Add(set.ToString());
			return this;
		}

		public static VkAuthSettingsBuilder Common()//fix
		{
			return new VkAuthSettingsBuilder().Add(VkAuthSetting.friends).Add(VkAuthSetting.wall);
		}
	}
}