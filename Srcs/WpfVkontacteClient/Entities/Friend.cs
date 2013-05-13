using System;
using System.Data;
using System.Linq;

namespace WpfVkontacteClient.Entities
{
	public class Friend
	{
		private int m_uid;

		public int UserId
		{
			get { return m_uid; }
		}

		private string m_firstName;

		public string FirstName
		{
			get { return m_firstName; }
		}

		private string m_lastName;

		public string LastName
		{
			get { return m_lastName; }
		}

		private string m_nick;

		public string Nick
		{
			get { return m_nick; }
		}

		private string m_pictureMedium;

		public string PictureMedium
		{
			get { return m_pictureMedium; }
		}

		private bool m_Online;

		public bool Online
		{
			get
			{
				return m_Online;
			}
		}

		private string m_picture;

		public string Picture
		{
			get { return m_picture; }
			set
			{
				m_picture = value;
			}
		}

		private DateTime m_birthDay;

		public DateTime BirthDay
		{
			get { return m_birthDay; }
		}

		private string m_city;

		public string City
		{
			get { return m_city; }
		}

		private string m_country;

		public string Country
		{
			get { return WpfVkontacteClient.Extension.NumberToCountry(m_country); }
		}

		public Friend(DataRow row)
		{
			if (!Int32.TryParse(row["uid"].ToString(), out m_uid))
			{
				LogModule.LoggingModule.Instance.WriteMessage(LogModule.LoggingModule.Severity.Warning, "error in conversion user", row["uid"].ToString());
			}

			if (row.Table.Columns.Contains("first_name"))
				m_firstName = row["first_name"].ToString();

			if (row.Table.Columns.Contains("last_name"))
				m_lastName = row["last_name"].ToString();

			if (row.Table.Columns.Contains("nickname"))
				m_nick = row["nickname"].ToString();

			if (row.Table.Columns.Contains("photo_medium"))
				m_pictureMedium = row["photo_medium"].ToString();

			if (row.Table.Columns.Contains("photo"))
				m_picture = row["photo"].ToString();

			if (row.Table.Columns.Contains("online"))
			{
				Boolean.TryParse(row["online"].ToString(), out m_Online);
			}

			if (row.Table.Columns.Contains("bdate") && row["bdate"].ToString().Length > 3)
			{
				string[] dates = row["bdate"].ToString().Split('.');
				try
				{
					if (dates.Length == 2)
						m_birthDay = new DateTime(1500, Convert.ToInt32(dates[1]), Convert.ToInt32(dates[0]));
					if (dates.Length == 3 && !dates.Any(data => data.Contains('-')))
						m_birthDay = new DateTime(Convert.ToInt32(dates[2]), Convert.ToInt32(dates[1]), Convert.ToInt32(dates[0]));
				}
				catch { }
				//TODO: check year and date [0] [2]
			}

			if (row.Table.Columns.Contains("city"))
			{
				m_city = row["city"].ToString();
			}

			if (row.Table.Columns.Contains("country"))
			{
				m_country = row["country"].ToString();
			}
		}

		public Friend(long uid, string name, string lastname, string nick, string country, string city, string photo)
		{
			this.m_uid = (int)uid;
			this.m_firstName = name;
			this.m_lastName = lastname;
			this.m_nick = string.IsNullOrEmpty(nick) == true ? string.Empty : nick;
			this.m_country = string.IsNullOrEmpty(country) == true ? string.Empty : country;
			this.m_city = string.IsNullOrEmpty(city) == true ? string.Empty : city;
			this.m_picture = string.IsNullOrEmpty(photo) == true ? string.Empty : photo;
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", m_firstName, m_lastName);
		}
	}
}