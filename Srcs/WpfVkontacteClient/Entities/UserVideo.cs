using System;
using System.Data;
using Utils;

namespace WpfVkontacteClient.Entities
{
	public class UserVideo : ItemToLoad
	{
		private long m_videoId;

		public long VideoId
		{
			get
			{
				return m_videoId;
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

		private long m_ownerId;

		public long OwnerId
		{
			get
			{
				return m_ownerId;
			}
		}

		private int m_duration;

		public int Duration
		{
			get
			{
				return m_duration;
			}
		}

		private string m_date;

		public DateTime Date
		{
			get
			{
				return DateTimeUtils.ConvertFromUnixTimestamp(double.Parse(m_date));
			}
		}

		private string m_thumb;

		public string Thumb
		{
			get
			{
				return m_thumb;
			}
		}

		private string m_image;

		public string Image
		{
			get
			{
				return m_image;
			}
		}

		private string m_link;

		public string Link
		{
			get { return string.Format(@"http://vkontakte.ru/{0}", m_link); }
		}

		protected bool m_checked;

		public bool IsCheckedInList
		{
			get { return m_checked; }
			set
			{
				m_checked = value;
				base.OnPropertyChanged("IsCheckedInList");
			}
		}

		private string m_player;

		public string PlayerUrl
		{
			get { return m_player; }
		}

		public UserVideo(DataRow row)
		{
			this.State = LoadState.None;
			this.IsCheckedInList = false;
			this.Percentage = 0;

			if (row.Table.Columns.Contains("vid"))
				long.TryParse(row["vid"].ToString(), out m_videoId);

			if (row.Table.Columns.Contains("id"))
				long.TryParse(row["id"].ToString(), out m_videoId);

			if (row.Table.Columns.Contains("title"))
				this.m_title = row["title"].ToString();

			if (row.Table.Columns.Contains("description"))
				this.m_description = row["description"].ToString();

			if (row.Table.Columns.Contains("owner_id"))
				long.TryParse(row["owner_id"].ToString(), out m_ownerId);

			if (row.Table.Columns.Contains("duration"))
				Int32.TryParse(row["duration"].ToString(), out m_duration);

			if (row.Table.Columns.Contains("date"))
				this.m_date = row["date"].ToString();

			if (row.Table.Columns.Contains("thumb"))
				this.m_thumb = row["thumb"].ToString();

			if (row.Table.Columns.Contains("link"))
				this.m_link = row["link"].ToString();

			if (row.Table.Columns.Contains("image"))
				this.m_image = row["image"].ToString();

			if (row.Table.Columns.Contains("player"))
				this.m_player = row["player"].ToString();
		}

		public UserVideo(long vid, long ownerId, string title, long duration, string image)
		{
			this.State = LoadState.None;
			m_videoId = vid;
			m_ownerId = ownerId;
			m_title = title;
			m_duration = (int)duration;
			m_image = image;
		}

		public override string ToString()
		{
			return string.Format("{0} \r\n {1}", m_title, m_description);
		}
	}
}