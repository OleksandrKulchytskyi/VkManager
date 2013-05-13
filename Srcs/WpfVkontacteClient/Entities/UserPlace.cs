using System;
using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class UserPlace
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public UserPlace(DataRow row)
		{
			if (row == null)
				throw new ArgumentNullException("row");

			int m_id;
			if (Int32.TryParse(row["cid"].ToString(), out m_id))
			{
				Id = m_id;
			}

			Name = row["name"].ToString();
		}

		public UserPlace()
		{
			Name = string.Empty;
		}
	}
}