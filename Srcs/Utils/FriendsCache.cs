using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Utils
{
	public interface IFriendsCache
	{
		void AddToFriendsTable(int uid, string name, string lastname, string nickname, string country, string city, string photo);

		void AddToFriendsPhotoTable(long uid, byte[] photo, byte[] photoMedium);

		byte[] GetImage(int uid, int type);

		Tuple<string, string, string> GetUserInfo(long uid);

		bool IsFriendCached(long uid);

		bool IsFriendPhotoCached(long uid);

		bool IsEmpty();
	}

	public class FriendsCache : IFriendsCache, IDisposable
	{
		private bool m_isDisposed = false;
		private SQLiteConnection m_sqlCon;
		private static FriendsCache m_instance = null;

		public FriendsCache(string filename)
		{
			var builder = new SQLiteConnectionStringBuilder
			{
				BinaryGUID = true,
				DataSource = filename,
				Version = 3
			};
			string connectionString = builder.ToString();
			m_sqlCon = new SQLiteConnection(connectionString);
			m_sqlCon.Open();
			m_instance = this;
		}

		public static FriendsCache Instance
		{
			get
			{
				return m_instance;
			}
		}

		~FriendsCache()
		{
			Dispose();
		}

		private int CreateTables()
		{
			var command = m_sqlCon.CreateCommand();
			command.CommandText = "CREATE TABLE [Friends] ([Uid] int primary key not null,[Name] NVARCHAR(30) NOT NULL,[LastName] NVARCHAR(30) NOT NULL,[NickName] NVARCHAR(30) NULL,[Country] NVARCHAR(50) NULL,[City] NVARCHAR(50) NULL,[PhotoUrl] NVARCHAR(255) NULL)";
			int res = command.ExecuteNonQuery();
			command.CommandText = "CREATE TABLE [FriendsPhoto] ([Uid] int primary key not null,[Photo] BLOB NULL,[PhotoMedium] BLOB NULL)";
			res = command.ExecuteNonQuery();
			command.Dispose();
			return res;
		}

		private int CreateIndexes()
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "CREATE UNIQUE INDEX uidfriend_indx ON [Friends] (Uid)";
			int res = command.ExecuteNonQuery();

			command.CommandText = "CREATE UNIQUE INDEX uidphoto_indx ON [FriendsPhoto] (Uid)";
			res = command.ExecuteNonQuery();
			command.Dispose();
			return res;
		}

		public static FriendsCache OpenOrCreate(string filename)
		{
			if (System.IO.File.Exists(filename))
				return new FriendsCache(filename);
			else
			{
				FriendsCache ret = new FriendsCache(filename);
				ret.CreateTables();
				ret.CreateIndexes();
				return ret;
			}
		}

		public void AddToFriendsTable(int uid, string name, string lastname, string nickname, string country, string city, string photo)
		{
			if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lastname))
				return;

			if (CheckIfExist("Friends", (long)uid))
			{
				this.UpdateFriendsTable((long)uid, name, lastname, nickname, country, city, photo);
				return;
			}

			var friendCommand = m_sqlCon.CreateCommand();
			friendCommand.CommandText = string.Format("Insert into Friends values({0},'{1}','{2}','{3}','{4}','{5}','{6}')", uid, name, lastname,
									string.IsNullOrEmpty(nickname) == true ? null : nickname,
									string.IsNullOrEmpty(country) == true ? null : country,
									string.IsNullOrEmpty(city) == true ? null : city,
									string.IsNullOrEmpty(photo) == true ? null : photo);

			int rowCount = friendCommand.ExecuteNonQuery();
			friendCommand.Dispose();

			using (var photoCommand = m_sqlCon.CreateCommand())
			{
				photoCommand.CommandText = "INSERT INTO FriendsPhoto VALUES(@p1,@p2,@p3)";
				SQLiteParameter p_uid = new SQLiteParameter("@p1");
				SQLiteParameter p_photo = new SQLiteParameter("@p2");
				SQLiteParameter p_photo2 = new SQLiteParameter("@p3");
				photoCommand.Parameters.Add(p_uid);
				photoCommand.Parameters.Add(p_photo);
				photoCommand.Parameters.Add(p_photo2);
				p_uid.Value = uid;
				p_photo.Value = null;
				p_photo2.Value = null;
				rowCount = photoCommand.ExecuteNonQuery();
			}
		}

		public void AddToFriendsPhotoTable(long uid, byte[] photo, byte[] photoMedium)
		{
			if (CheckIfExist("FriendsPhoto", (long)uid))
			{
				this.UpdateFriendsPhotoTable(uid, photo, photoMedium);
			}

			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "INSERT INTO [FriendsPhoto] VALUES (@uid,@photo,@photo2)";
			SQLiteParameter p_uid = new SQLiteParameter("@uid");
			SQLiteParameter p_photo = new SQLiteParameter("@photo");
			SQLiteParameter p_photo2 = new SQLiteParameter("@photo2");
			command.Parameters.Add(p_uid);
			command.Parameters.Add(p_photo);
			command.Parameters.Add(p_photo2);
			p_uid.Value = uid;
			p_photo.Value = photo;
			p_photo2.Value = photoMedium;
			int rowCount = command.ExecuteNonQuery();
			command.Dispose();
		}

		public void UpdateFriendsPhotoTable(long uid, byte[] photo, byte[] photoMedium)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "UPDATE [FriendsPhoto] Set Photo=@photo,PhotoMedium=@photo2 WHERE Uid=@uid";
			SQLiteParameter p_uid = new SQLiteParameter("@uid");
			SQLiteParameter p_photo = new SQLiteParameter("@photo");
			SQLiteParameter p_photo2 = new SQLiteParameter("@photo2");
			command.Parameters.Add(p_uid);
			command.Parameters.Add(p_photo);
			command.Parameters.Add(p_photo2);
			p_uid.Value = uid;
			p_photo.Value = photo;
			p_photo2.Value = photoMedium;
			int rowCount = command.ExecuteNonQuery();
			command.Dispose();
		}

		public void UpdateFriendsTable(long uid, string name, string lastname, string nick, string country, string city, string photoUrl)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "UPDATE [Friends] Set Name=@name,LastName=@lastname,NickName=@nick,Country=@country,City=@city,PhotoUrl=@photo WHERE Uid=@uid";
			SQLiteParameter p_uid = new SQLiteParameter("@uid");
			SQLiteParameter p_name = new SQLiteParameter("@name");
			SQLiteParameter p_lname = new SQLiteParameter("@lastname");
			SQLiteParameter p_nick = new SQLiteParameter("@nick");
			SQLiteParameter p_country = new SQLiteParameter("@country");
			SQLiteParameter p_city = new SQLiteParameter("@city");
			SQLiteParameter p_photo = new SQLiteParameter("@photo");
			command.Parameters.Add(p_uid);
			command.Parameters.Add(p_name);
			command.Parameters.Add(p_lname);
			command.Parameters.Add(p_nick);
			command.Parameters.Add(p_country);
			command.Parameters.Add(p_city);
			command.Parameters.Add(p_photo);
			p_uid.Value = uid;
			p_name.Value = name;
			p_lname.Value = lastname;
			p_nick.Value = nick;
			p_city.Value = city;
			p_country.Value = country;
			p_photo.Value = photoUrl;
			int rowCount = command.ExecuteNonQuery();
			command.Dispose();
		}

		public List<SimpleUserInfo> GetAllDataFromFriends()
		{
			using (SQLiteCommand command = m_sqlCon.CreateCommand())
			{
				List<SimpleUserInfo> dataList = new List<SimpleUserInfo>();
				command.CommandText = "SELECT Uid,Name,LastName,NickName,Country,City,PhotoUrl FROM [Friends]";
				using (var dr = command.ExecuteReader())
				{
					while (dr.Read())
					{
						var simplUser = new SimpleUserInfo();
						simplUser.Uid = dr.GetInt64(0);
						simplUser.Name = dr.GetString(1);
						simplUser.LastName = dr.GetString(2);
						simplUser.NickName = dr.IsDBNull(3) == true ? string.Empty : dr.GetString(3);
						simplUser.Country = dr.IsDBNull(4) == true ? string.Empty : dr.GetString(4);
						simplUser.City = dr.IsDBNull(5) == true ? string.Empty : dr.GetString(5);
						simplUser.PhotoUrl = dr.IsDBNull(6) == true ? string.Empty : dr.GetString(6);

						dataList.Add(simplUser);
					}
					dr.Close();
				}
				return dataList;
			}
		}

		/// <summary>
		/// Get simple photo for user by it uid parameter
		/// </summary>
		/// <param name="uid"></param>
		/// <param name="type">0 - fefault, 1 -medium</param>
		/// <returns></returns>
		public byte[] GetImage(int uid, int type)
		{
			byte[] Photo = null;
			var command = m_sqlCon.CreateCommand();

			switch (type)
			{
				case 0:
					command.CommandText = string.Format("Select Photo from FriendsPhoto WHERE Uid={0}", uid);
					Photo = command.ExecuteScalar() as byte[];
					break;

				case 1:
					command.CommandText = string.Format("Select PhotoMedium from FriendsPhoto WHERE Uid={0}", uid);
					Photo = command.ExecuteScalar() as byte[];
					break;

				default:
					break;
			}

			command.Dispose();
			return Photo;
		}

		public bool IsFriendCached(long uid)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = string.Format("SELECT COUNT(*) FROM [Friends] WHERE [Uid]={0}", uid);

			object o = command.ExecuteScalar();
			command.Dispose();
			return ((long)o) != 0;
		}

		public bool IsFriendPhotoCached(long uid)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = string.Format("SELECT COUNT(*) FROM [FriendsPhoto] WHERE [Uid]={0}", uid);

			object o = command.ExecuteScalar();
			command.Dispose();
			return ((long)o) != 0;
		}

		public Tuple<string, string, string> GetUserInfo(long uid)
		{
			using (SQLiteCommand command = m_sqlCon.CreateCommand())
			{
				Tuple<string, string, string> tuple = null;

				command.CommandText = string.Format("SELECT Name,LastName,Nick FROM [Friends] WHERE [Uid]={0}", uid);
				using (var dr = command.ExecuteReader())
				{
					while (dr.Read())
					{
						tuple = new Tuple<string, string, string>(dr.GetString(0), dr.GetString(1), dr.GetString(2));
						break;
					}
					dr.Close();
				}
				return tuple;
			}
		}

		public string GetOnlyUserFullName(long uid)
		{
			using (SQLiteCommand command = m_sqlCon.CreateCommand())
			{
				command.CommandText = string.Format("SELECT Name,LastName FROM [Friends] WHERE [Uid]={0}", uid);
				string result = string.Empty;
				using (var dr = command.ExecuteReader())
				{
					while (dr.Read())
					{
						result = string.Format("{0} {1}", dr.GetString(0), dr.GetString(1));
						break;
					}
					dr.Close();
				}
				return result;
			}
		}

		/// <summary>
		/// Get records count from cache in table Friends
		/// </summary>
		/// <returns>recorsds count</returns>
		public int GetFriendsCount()
		{
			using (SQLiteCommand command = m_sqlCon.CreateCommand())
			{
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = "SELECT COUNT(*) from [Friends]";

				return ((int)command.ExecuteScalar());
			}
		}

		public List<long> GetFriendsWithoutDownloadedPhoto()
		{
			SQLiteCommand com = m_sqlCon.CreateCommand();
			com.CommandText = "SELECT Friends.Uid FROM Friends LEFT JOIN FriendsPhoto on Friends.Uid=FriendsPhoto.Uid where FriendsPhoto.Photo is NULL AND FriendsPhoto.PhotoMedium is NULL";
			using (SQLiteDataReader reader = com.ExecuteReader())
			{
				List<long> uids = new List<long>();
				while (reader.Read())
				{
					uids.Add(reader.GetInt64(0));
				}
				return uids;
			}
		}

		public Dictionary<long, string> GetFriendsWithoutDownloadedPhotoWithUrl()
		{
			SQLiteCommand com = m_sqlCon.CreateCommand();
			com.CommandText = "SELECT Friends.Uid,Friends.PhotoUrl FROM Friends LEFT JOIN FriendsPhoto on Friends.Uid=FriendsPhoto.Uid where FriendsPhoto.Photo is NULL AND FriendsPhoto.PhotoMedium is NULL";
			using (SQLiteDataReader reader = com.ExecuteReader())
			{
				Dictionary<long, string> uids = new Dictionary<long, string>();
				while (reader.Read())
				{
					if (!uids.ContainsKey(reader.GetInt64(0)))
					{
						string PhotoUrl = reader.IsDBNull(1) == true ? string.Empty : reader.GetString(1);
						uids.Add(reader.GetInt64(0), PhotoUrl);
					}
				}
				return uids;
			}
		}

		public bool IsEmpty()
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "SELECT COUNT(*) FROM [Friends]";

			object o = command.ExecuteScalar();
			command.Dispose();
			return ((long)o) == 0;
		}

		public bool CheckIfExist(string tableName, long uid)
		{
			if (string.IsNullOrEmpty(tableName))
				return false;
			using (SQLiteCommand command = m_sqlCon.CreateCommand())
			{
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = string.Format("SELECT COUNT(*) FROM [{0}] where Uid={1}", tableName, uid);
				return ((long)command.ExecuteScalar() == 1);
			}
		}

		public void Dispose()
		{
			if (!m_isDisposed && m_sqlCon != null)
			{
				m_sqlCon.Close();
				m_sqlCon = null;
				m_isDisposed = true;
				GC.SuppressFinalize(this);
			}
		}
	}

	public class SimpleUserInfo
	{
		public long Uid { get; set; }

		public string Name { get; set; }

		public string LastName { get; set; }

		public string NickName { get; set; }

		public string Country { get; set; }

		public string City { get; set; }

		public string PhotoUrl { get; set; }
	}
}