using System;
using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class UserComment
	{
		private long m_cid;

		public long CommentId
		{
			get
			{
				return m_cid;
			}
		}

		private long from_id;

		/// <summary>
		/// id пользователя который оставил коментарий
		/// </summary>
		public long FromUserId
		{
			get
			{
				return this.from_id;
			}
		}

		private string m_date;

		public DateTime Date
		{
			get
			{
				return Utils.DateTimeUtils.ConvertFromUnixTimestamp(double.Parse(m_date));
			}
		}

		private string m_message;

		public string Comment
		{
			get
			{
				return m_message;
			}
		}

		public UserComment(DataRow row)
		{
			if (row.Table.Columns.Contains("cid"))
				long.TryParse(row["cid"].ToString(), out this.m_cid);
			if (row.Table.Columns.Contains("id"))
				long.TryParse(row["id"].ToString(), out this.m_cid);

			if (row.Table.Columns.Contains("from_id"))
				long.TryParse(row["from_id"].ToString(), out this.from_id);
			if (row.Table.Columns.Contains("uid"))
				long.TryParse(row["uid"].ToString(), out this.from_id);

			if (row.Table.Columns.Contains("date"))
				this.m_date = row["date"].ToString();

			if (row.Table.Columns.Contains("message"))
				this.m_message = row["message"].ToString();
		}

		public override string ToString()
		{
			return m_message;
		}
	}
}