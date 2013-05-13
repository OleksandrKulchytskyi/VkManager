using System;
using System.Data;
using Utils;

namespace WpfVkontacteClient.Entities
{
	public class UserMessage
	{
		private string m_body;

		public string MessageBody
		{
			get
			{
				return m_body;
			}
		}

		private string m_title;

		public string MessageTitle
		{
			get
			{
				return m_title;
			}
		}

		private long m_messageId;

		public long MessageId
		{
			get
			{
				return m_messageId;
			}
		}

		private long m_uid;

		public long UserId
		{
			get
			{
				return m_uid;
			}
		}

		private bool m_state;

		public bool ReadState
		{
			get
			{
				return m_state;
			}
			set { m_state = value; }
		}

		private string m_date;

		public DateTime Date
		{
			get
			{
				return Utils.DateTimeUtils.ConvertFromUnixTimestamp(double.Parse(m_date));
			}
		}

		private string m_out;

		public string Out
		{
			get
			{
				return m_out;
			}
		}

		public UserMessage(DataRow row)
		{
			if (row.Table.Columns.Contains("mid"))
				long.TryParse(row["mid"].ToString(), out this.m_messageId);

			if (row.Table.Columns.Contains("uid"))
				long.TryParse(row["uid"].ToString(), out this.m_uid);

			if (row.Table.Columns.Contains("body"))
				this.m_body = row["body"].ToString().EscapeXmlString();

			if (row.Table.Columns.Contains("title"))
				this.m_title = row["title"].ToString();

			if (row.Table.Columns.Contains("date"))
				this.m_date = row["date"].ToString();

			if (row.Table.Columns.Contains("read_state"))
				this.m_state = Extension.FromStringToBool(row["read_state"].ToString());

			if (row.Table.Columns.Contains("out"))
			{
				this.m_out = row["out"].ToString();
			}
		}

		public override string ToString()
		{
			return string.Format("{0} \r\n {1}", m_title, m_body);
		}
	}
}