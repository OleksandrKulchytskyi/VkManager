using System;
using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class UserDocument
	{
		public long Id { get; set; }

		public long OwnerId { get; set; }

		public string Title { get; set; }

		public int Size { get; set; }

		public string Extension { get; set; }

		public string Url { get; set; }

		public UserDocument(DataRow row)
		{
			if (row == null)
				throw new ArgumentNullException("row");

			long m_id;
			if (long.TryParse(row["did"].ToString(), out m_id))
				Id = m_id;

			long m_OwnId;
			if (long.TryParse(row["owner_id"].ToString(), out m_OwnId))
				OwnerId = m_OwnId;

			int m_size;
			if (Int32.TryParse(row["size"].ToString(), out m_size))
				Size = m_size;

			if (row.Table.Columns.Contains("title"))
				Title = row["title"].ToString();

			if (row.Table.Columns.Contains("ext"))
				Extension = row["ext"].ToString();

			if (row.Table.Columns.Contains("url"))
				Url = row["url"].ToString();
		}

		public UserDocument()
		{
		}
	}
}