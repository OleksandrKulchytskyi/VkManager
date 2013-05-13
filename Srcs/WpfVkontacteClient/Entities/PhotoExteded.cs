using System;
using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class PhotoExteded
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

		public Uri UriSourceBig
		{
			get
			{
				return new Uri(m_srcBig, UriKind.RelativeOrAbsolute);
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

		private string m_srcxBig;

		public string SourceXBig
		{
			get
			{
				return m_srcxBig;
			}
		}

		private string m_srcxxBig;

		public string SourceXXBig
		{
			get
			{
				return m_srcxxBig;
			}
		}

		private string m_text;

		public string Text
		{
			get
			{
				return m_text;
			}
		}

		public PhotoExteded(DataRow row)
		{
			if (row.Table.Columns.Contains("aid"))
				long.TryParse(row["aid"].ToString(), out this.aid);

			if (row.Table.Columns.Contains("pid"))
				long.TryParse(row["pid"].ToString(), out this.pid);

			if (row.Table.Columns.Contains("owner_id"))
				long.TryParse(row["owner_id"].ToString(), out this.owner_id);

			if (row.Table.Columns.Contains("src"))
				this.m_src = row["src"].ToString();

			if (row.Table.Columns.Contains("src_big"))
				this.m_srcBig = row["src_big"].ToString();

			if (row.Table.Columns.Contains("src_xbig"))
				this.m_srcxBig = row["src_xbig"].ToString();

			if (row.Table.Columns.Contains("src_xxbig"))
				this.m_srcxxBig = row["src_xxbig"].ToString();

			if (row.Table.Columns.Contains("src_small"))
				this.m_srcSmall = row["src_small"].ToString();

			if (row.Table.Columns.Contains("text"))
				m_text = row["text"].ToString();
		}
	}
}