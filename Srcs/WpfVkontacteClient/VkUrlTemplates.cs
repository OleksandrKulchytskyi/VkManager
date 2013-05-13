using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfVkontacteClient
{
	public class VkUrlTemplates
	{
		//Login in OAuth 2.0
		public static string authorize = "http://api.vkontakte.ru/oauth/authorize?client_id=APP_ID&scope=SETTINGS&redirect_uri=REDIRECT_URI&display=DISPLAY&response_type=token";
		public const string Oauth2 = "http://api.vk.com/oauth/authorize?client_id=1&redirect_uri=http://api.vk.com/blank.html&scope=12&display=popup";

		//Old login urls
		public const string LoginUrl = "http://vkontakte.ru/login.php?app={0}&layout=popup&type=browser&settings={1}";
		public const string LoginSuccess = "http://vkontakte.ru/api/login_success.html";
		public const string LoginFailed = "http://vkontakte.ru/api/login_failure.html";
		public const string LongPollServer = "http://{0}?act=a_check&key={1}&ts={2}&wait=25";

		public static string CreateAuthorizationUsingOAuth2(int appId, string settings, string redirectUri = "http://api.vkontakte.ru/blank.html")
		{
			return string.Format("http://api.vkontakte.ru/oauth/authorize?client_id={0}&scope={1}&redirect_uri={2}&display=popup&response_type=token", appId, settings, redirectUri);
		}
	}
}
