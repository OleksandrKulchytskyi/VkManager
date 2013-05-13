using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WpfVkontacteClient;

namespace WpfVkClient.Test
{
	[TestClass]
	public class VkHelperTest
	{
		string appId = "2376559";
		string appSecret = "25LQOW9VtboLGpQwUQwi";

		[TestMethod]
		public void TestMethod1()
		{

			try
			{
				VKToken access = VkHelpers.GetAccessToken(appId, appSecret);
				Assert.IsNotNull(access.access_token);
			}
			catch (Exception ex)
			{
				if (ex.Message != null)
				{
					Debug.WriteLine(ex.Message);
					if (ex.InnerException != null)
						Debug.WriteLine(ex.InnerException.Message);
				}
				throw;
			}
		}

		[TestMethod]
		public void MyTestMethod2()
		{
			VKontakteApiWrapper wrapper = new VKontakteApiWrapper(long.Parse(appId), appSecret,(int)VKontakteApiWrapper.All);
			bool connected = wrapper.CoonectBasedOnServer(appId, appSecret);
			Assert.IsTrue(connected);

			Assert.IsTrue(object.ReferenceEquals(wrapper, VKontakteApiWrapper.Instance));
			var doc= VKontakteApiWrapper.Instance.ExecuteMethodByToken("audio.get",new List<VKParameter>() { new VKParameter("uid", "81523827") });
			Assert.IsNotNull(doc);
		}
	}
}
