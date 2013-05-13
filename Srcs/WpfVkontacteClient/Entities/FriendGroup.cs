using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class FriendGroup
	{
		private long m_lid;

		public long Lid
		{
			get
			{
				return m_lid;
			}
		}

		private string m_name;

		public string Name
		{
			get
			{
				return m_name;
			}
		}

		public FriendGroup(DataRow row)
		{
			long.TryParse(row["lid"].ToString(), out m_lid);
			m_name = row["name"].ToString();
		}
	}
}