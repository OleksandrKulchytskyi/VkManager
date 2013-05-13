using System;
using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class UserAlbum
	{
		private long m_aid;

		public long AlbumId
		{
			get { return m_aid; }
		}

		private long thumb_id;

		public long ThumbId
		{
			get
			{
				return thumb_id;
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

		private string m_title;

		public string Title
		{
			get
			{
				return m_title;
			}
		}

		private string m_description;

		public string Description
		{
			get
			{
				return m_description;
			}
		}

		private int m_size;

		public int Size
		{
			get
			{
				return m_size;
			}
		}

		private int m_privacy;

		public int Privacy
		{
			get
			{
				return m_privacy;
			}
		}

		public UserAlbum(DataRow row)
		{
			long.TryParse(row["aid"].ToString(), out this.m_aid);
			long.TryParse(row["thumb_id"].ToString(), out this.thumb_id);
			long.TryParse(row["owner_id"].ToString(), out this.owner_id);
			this.m_title = row["title"].ToString();
			this.m_description = row["description"].ToString();
			Int32.TryParse(row["size"].ToString(), out m_size);
			if (row.Table.Columns.Contains("privacy"))
			{
				Int32.TryParse(row["privacy"].ToString(), out m_privacy);
			}
		}

		public UserAlbum(long aid, long thumbId, long ownerId, string title, string description, int size, int privacy)
		{
			m_aid = aid; owner_id = ownerId; thumb_id = thumbId;
			m_title = title;
			m_description = description; m_size = size; m_privacy = privacy;
		}

		public override string ToString()
		{
			return string.Format("{0} \r\n {1} \r\n {2}", m_title, m_description, m_size);
		}
	}
}