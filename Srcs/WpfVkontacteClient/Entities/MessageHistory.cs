using System;
using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class MessageHistory
	{
		private string m_body;

		public string Body
		{
			get
			{
				return m_body;
			}
		}

		private long m_mid;

		public long MessageId
		{
			get
			{
				return m_mid;
			}
		}

		private long m_fromId;

		public long FromId
		{
			get
			{
				return m_fromId;
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

		private bool m_readState;

		public bool ReadState
		{
			get
			{
				return m_readState;
			}
		}

		public MessageHistory(DataRow row)
		{
			if (row.Table.Columns.Contains("mid"))
				long.TryParse(row["mid"].ToString(), out this.m_mid);

			if (row.Table.Columns.Contains("from_id"))
				long.TryParse(row["from_id"].ToString(), out this.m_fromId);

			if (row.Table.Columns.Contains("body"))
				m_body = row["body"].ToString();

			if (row.Table.Columns.Contains("date"))
				m_date = row["date"].ToString();

			if (row.Table.Columns.Contains("read_state"))
				m_readState = Extension.FromStringToBool(row["read_state"].ToString());
		}

		public override string ToString()
		{
			return this.m_body;
		}
	}
}