using LogModule;
using System;
using System.Data.SQLite;
using System.IO;

namespace Utils
{
	public interface IByteCache
	{
		void SaveData(string url, byte[] image);

		byte[] GetData(string url);

		bool IsCached(string url);

		bool ClearAll();
	}

	public sealed class ByteCache : IByteCache, IDisposable
	{
		private bool isDisposed = false;
		private SQLiteConnection m_sqlCon;

		public ByteCache(string filename)
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
		}

		~ByteCache()
		{
			Dispose();
		}

		private int CreateTables()
		{
			var command = m_sqlCon.CreateCommand();
			command.CommandText = "CREATE TABLE [ByteCache] ([Url] NVARCHAR(200) UNIQUE NOT NULL,[Data] BLOB NOT NULL)";
			int res = command.ExecuteNonQuery();
			command.Dispose();
			return res;
		}

		private int CreateIndexes()
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "CREATE UNIQUE INDEX url_indx ON [ByteCache] (Url)";
			int res = command.ExecuteNonQuery();
			command.Dispose();
			return res;
		}

		public static ByteCache OpenOrCreate(string filename)
		{
			if (File.Exists(filename))
				return new ByteCache(filename);
			else
			{
				ByteCache ret = new ByteCache(filename);
				ret.CreateTables();
				ret.CreateIndexes();
				return ret;
			}
		}

		public void SaveData(string url, byte[] data)
		{
			if (this.IsCached(url))
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, string.Format("Specify url {0} \n\r was found in DB", url.ToUpper()),
													"It will be updated");
				this.Update(url, data);
				return;
			}

			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "INSERT INTO [ByteCache] VALUES (@url,@data)";
			SQLiteParameter p_url = new SQLiteParameter("@url");
			SQLiteParameter p_image = new SQLiteParameter("@data");
			command.Parameters.Add(p_url);
			command.Parameters.Add(p_image);
			p_url.Value = url;
			p_image.Value = data;
			command.ExecuteNonQuery();
			command.Dispose();
		}

		public byte[] GetData(string url)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = string.Format("SELECT [Data] FROM [ByteCache] WHERE [Url]='{0}'", url);

			object o = command.ExecuteScalar();
			command.Dispose();
			return o as byte[];
		}

		public bool IsCached(string url)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = string.Format("SELECT COUNT(*) FROM [ByteCache] WHERE [Url]='{0}'", url);

			object o = command.ExecuteScalar();
			command.Dispose();
			return ((long)o) != 0;
		}

		public bool Delete(string url)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = string.Format("DELETE FROM [ByteCache] WHERE [Url]='{0}'", url);

			object o = command.ExecuteNonQuery();
			command.Dispose();

			LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, "Specify data url was delete", url);

			return ((int)o) != 0;
		}

		public bool Update(string url, byte[] data)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "UPDATE [ByteCache] SET [Data]=@data WHERE [Url]=@url";
			SQLiteParameter dataParam = new SQLiteParameter("@data");
			command.Parameters.Add(dataParam);
			dataParam.Value = data;
			SQLiteParameter urlParam = new SQLiteParameter("@url");
			command.Parameters.Add(urlParam);
			urlParam.Value = url;

			object o = command.ExecuteNonQuery();
			command.Dispose();
			return ((int)o) != 0;
		}

		public bool ClearAll()
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "DELETE FROM [ByteCache]";

			object o = command.ExecuteNonQuery();
			command.Dispose();
			return ((int)o) != 0;
		}

		public int GetRecordsCount()
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "SELECT COUNT(*) FROM [ByteCache]";
			object o = command.ExecuteNonQuery();
			command.Dispose();
			return (int)o;
		}

		public void Dispose()
		{
			if (!isDisposed && m_sqlCon != null)
			{
				m_sqlCon.Close();
				m_sqlCon = null;
				isDisposed = true;
				GC.SuppressFinalize(this);
			}
		}
	}
}