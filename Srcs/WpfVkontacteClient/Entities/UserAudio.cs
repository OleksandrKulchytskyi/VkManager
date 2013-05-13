using System;
using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class UserAudio : ItemToLoad
	{
		private long m_aid;

		public long AudioId
		{
			get
			{ return m_aid; }
		}

		private long owner_id;

		public long OwnerId
		{
			get
			{
				return owner_id;
			}
		}

		private string m_artist;

		public string Artist
		{
			get { return m_artist; }
		}

		private string m_title;

		public string Title
		{
			get { return m_title; }
		}

		private long m_duration;

		public long Duration
		{
			get
			{
				return m_duration;
			}
		}

		private long m_lyricsId;

		public long LyricId
		{
			get { return m_lyricsId; }
		}

		/// <summary>
		/// Отмечена ли запись в листе
		/// </summary>
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

		public UserAudio(DataRow row)
		{
			this.IsCheckedInList = false;
			Percentage = 0;
			m_duration = 0;

			long.TryParse(row["aid"].ToString(), out this.m_aid);
			long.TryParse(row["owner_id"].ToString(), out this.owner_id);

			if (row.Table.Columns.Contains("artist"))
				this.m_artist = row["artist"].ToString();
			if (row.Table.Columns.Contains("title"))
				this.m_title = row["title"].ToString();
			if (row.Table.Columns.Contains("url"))
				this.Url = row["url"].ToString();

			if (row.Table.Columns.Contains("lyrics_id") && !string.IsNullOrEmpty(row["lyrics_id"].ToString()))
				this.m_lyricsId = long.Parse(row["lyrics_id"].ToString());
		}

		public UserAudio(string url, long aid)
		{
			this.IsCheckedInList = false;
			Percentage = 0;
			m_duration = 0;
			this.Url = url;
			m_aid = aid;
		}

		public UserAudio(long aid, long ownerId, string title, long duration)
		{
			m_aid = aid;
			owner_id = ownerId;
			m_title = title;
			m_duration = duration;
		}

		public override string ToString()
		{
			return string.Format("Автор: {0} {1} Название: {2}", this.m_artist, Environment.NewLine, this.m_title);
		}
	}
}