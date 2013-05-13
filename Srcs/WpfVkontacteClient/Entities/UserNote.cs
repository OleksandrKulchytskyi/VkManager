using System;

namespace WpfVkontacteClient.Entities
{
	public class UserNote
	{
		public long NoteId { get; set; }

		public long UserId { get; set; }

		public string Title { get; set; }

		public string Text { get; set; }

		public DateTime Date { get; set; }

		public int CommentsCount { get; set; }

		public Nullable<int> UserReadedComments { get; set; }

		public UserNote()
		{
		}

		public UserNote(System.Data.DataRow row)
		{
			NoteId = long.Parse(row["nid"].ToString());
			UserId = long.Parse(row["uid"].ToString());
			Title = row["title"].ToString();
			Text = row["text"].ToString();
			Date = Utils.DateTimeUtils.ConvertFromUnixTimestamp(double.Parse(row["date"].ToString()));
			CommentsCount = Int32.Parse(row["ncom"].ToString());

			if (row.Table.Columns.Contains("read_ncom"))
			{
				UserReadedComments = Int32.Parse(row["read_ncom"].ToString());
			}
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", Title, Text);
		}
	}
}