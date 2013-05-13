using System;
using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class UserGroup
	{
		private int m_groupId;

		public int GroupId
		{
			get
			{
				return m_groupId;
			}
		}

		private string m_Name;

		public string Name
		{
			get
			{
				return m_Name;
			}
		}

		private string m_photoUrl;

		public string FotoUrl
		{
			get { return m_photoUrl; }
		}

		private bool isClosed;

		public bool IsClosed
		{
			get
			{
				return isClosed;
			}
		}

		public UserGroup(DataRow row)
		{
			if (row.Table.Columns.Contains("gid"))
				Int32.TryParse(row["gid"].ToString(), out m_groupId);

			if (row.Table.Columns.Contains("name"))
				this.m_Name = row["name"].ToString();

			if (row.Table.Columns.Contains("photo"))
				this.m_photoUrl = row["photo"].ToString();

			if (row.Table.Columns.Contains("is_closed"))
				this.isClosed = Extension.FromStringToBool(row["is_closed"].ToString());
		}
	}
}