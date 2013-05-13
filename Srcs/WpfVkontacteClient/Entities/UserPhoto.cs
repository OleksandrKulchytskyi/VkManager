using System;
using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class UserPhoto
	{
		private long pid;

		public long PhotoId
		{
			get { return pid; }
		}

		private long aid;

		public long AlbumId
		{
			get
			{
				return aid;
			}
		}

		private long owner_id;

		public long OwnerId
		{
			get
			{
				return owner_id;
			}
		}

		private string m_src;

		public string Source
		{
			get
			{
				return m_src;
			}
		}

		private string m_srcBig;

		public string SourceBig
		{
			get
			{
				return m_srcBig;
			}
		}

		private string m_srcSmall;

		public string SourceSmall
		{
			get
			{
				return m_srcSmall;
			}
		}

		private string m_created;

		public DateTime Created
		{
			get
			{
				return Utils.DateTimeUtils.ConvertFromUnixTimestamp(double.Parse(m_created));
			}
		}

		public UserPhoto(DataRow row)
		{
			if (row.Table.Columns.Contains("aid"))
				long.TryParse(row["aid"].ToString(), out this.aid);

			if (row.Table.Columns.Contains("pid"))
				long.TryParse(row["pid"].ToString(), out this.pid);

			if (row.Table.Columns.Contains("owner_id"))
				long.TryParse(row["owner_id"].ToString(), out this.owner_id);

			if (row.Table.Columns.Contains("created"))
				this.m_created = row["created"].ToString();

			if (row.Table.Columns.Contains("src"))
				this.m_src = row["src"].ToString();

			if (row.Table.Columns.Contains("src_big"))
				this.m_srcBig = row["src_big"].ToString();

			if (row.Table.Columns.Contains("src_small"))
				this.m_srcSmall = row["src_small"].ToString();
		}

		public UserPhoto(long pid, long ownerId, string src, string srcBig)
		{
			this.pid = pid;
			this.owner_id = ownerId;
			this.m_src = src;
			this.m_srcBig = srcBig;
		}
	}
}