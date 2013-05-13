using System;
using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class UserInfos
	{
		private int m_id;

		public int Id
		{
			get { return m_id; }
		}

		private string m_first_name;

		public string First_name
		{
			get { return m_first_name; }
		}

		private string m_last_name;

		public string Last_name
		{
			get { return m_last_name; }
		}

		private string m_sex;

		public string Sex
		{
			get
			{
				return m_sex;
			}
		}

		private string m_Online;

		public string Online
		{
			get { return m_Online; }
		}

		private string m_domain;

		public string Domain
		{
			get { return m_domain; }
		}

		private string m_city;

		public string City
		{
			get { return m_city; }
		}

		private string m_country;

		public string Country
		{
			get { return m_country; }
		}

		private string m_rate;

		public string Rate
		{
			get { return m_rate; }
		}

		private string m_photoBig;

		public string PhotoBigUrl
		{
			get { return m_photoBig; }
		}

		private string m_photoMedium;

		public string PhotoMediumUrl
		{
			get { return m_photoMedium; }
		}

		public UserInfos(DataRow row)
		{
			m_Online = string.Empty;

			Int32.TryParse(row["uid"].ToString(), out m_id);

			m_first_name = row["first_name"].ToString();
			m_last_name = row["last_name"].ToString();

			if (row.Table.Columns.Contains("online"))
				m_Online = row["online"].ToString();

			if (row.Table.Columns.Contains("city"))
				m_city = row["city"].ToString();

			string picture_uri = string.Empty;
			if (row.Table.Columns.Contains("photo"))
				picture_uri = row["photo"].ToString();

			if (row.Table.Columns.Contains("rate"))
				m_rate = row["rate"].ToString();

			if (row.Table.Columns.Contains("country"))
				m_country = row["country"].ToString();

			if (row.Table.Columns.Contains("sex"))
				m_sex = row["sex"].ToString();

			m_domain = row["domain"].ToString();

			if (row.Table.Columns.Contains("photo_medium"))
				m_photoMedium = row["photo_medium"].ToString();

			if (row.Table.Columns.Contains("photo_big"))
				m_photoBig = row["photo_big"].ToString();
		}

		public override string ToString()
		{
			return string.Format("{0} {1} Online:{2} ", m_first_name, m_last_name, m_Online);
		}
	}
}