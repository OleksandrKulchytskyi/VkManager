using System;
using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class OnlineFriends
	{
		private int m_uid;

		public int UId
		{
			get { return m_uid; }
		}

		public OnlineFriends(DataRow dr)
		{
			Int32.TryParse(dr["uid_Text"].ToString(), out m_uid);
		}
	}
}