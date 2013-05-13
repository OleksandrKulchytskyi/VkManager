using LogModule;
using System;
using System.Data.SQLite;
using System.IO;

namespace Utils
{
	public interface IImageCache
	{
		void SaveImage(string url, byte[] image);

		byte[] GetImage(string url);

		bool IsCached(string url);

		bool ClearAll();
	}

	public sealed class ImageCache : IImageCache, IDisposable
	{
		private bool isDisposed = false;
		private SQLiteConnection m_sqlCon;

		public ImageCache(string filename)
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

		~ImageCache()
		{
			Dispose();
		}

		private int CreateTables()
		{
			var command = m_sqlCon.CreateCommand();
			command.CommandText = "CREATE TABLE [ImageCache] ([Url] NVARCHAR(200) UNIQUE NOT NULL,[Image] BLOB NOT NULL)";
			int res = command.ExecuteNonQuery();
			command.Dispose();
			return res;
		}

		private int CreateIndexes()
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "CREATE UNIQUE INDEX url_indx ON [ImageCache] (Url)";
			int res = command.ExecuteNonQuery();
			command.Dispose();
			return res;
		}

		public static ImageCache OpenOrCreate(string filename)
		{
			if (File.Exists(filename))
				return new ImageCache(filename);
			else
			{
				ImageCache ret = new ImageCache(filename);
				ret.CreateTables();
				ret.CreateIndexes();
				return ret;
			}
		}

		public void SaveImage(string url, byte[] image)
		{
			if (this.IsCached(url))
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, string.Format("Specify url {0} \n\r was found in DB", url.ToUpper()),
													"It will be updated");
				this.Update(url, image);

				return;
			}

			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "INSERT INTO [ImageCache] VALUES (@url,@image)";
			SQLiteParameter p_url = new SQLiteParameter("@url");
			SQLiteParameter p_image = new SQLiteParameter("@image");
			command.Parameters.Add(p_url);
			command.Parameters.Add(p_image);
			p_url.Value = url;
			p_image.Value = image;
			command.ExecuteNonQuery();
			command.Dispose();
		}

		public byte[] GetImage(string url)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = string.Format("SELECT [Image] FROM [ImageCache] WHERE [Url]='{0}'", url);

			object o = command.ExecuteScalar();
			command.Dispose();
			return o as byte[];
		}

		public bool IsCached(string url)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = string.Format("SELECT COUNT(*) FROM [ImageCache] WHERE [Url]='{0}'", url);

			object o = command.ExecuteScalar();
			command.Dispose();
			return ((long)o) != 0;
		}

		public bool ClearAll()
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "DELETE FROM [ImageCache]";
			object o = command.ExecuteNonQuery();
			command.Dispose();
			return ((int)o) != 0;
		}

		public bool Delete(string url)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = string.Format("DELETE FROM [ImageCache] WHERE [Url]='{0}'", url);
			object o = command.ExecuteNonQuery();
			command.Dispose();

			LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, "Specify data url was delete", url);

			return ((int)o) != 0;
		}

		public bool Update(string url, byte[] data)
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "UPDATE [ImageCache] SET [Image]=@img WHERE [Url]=@url";
			SQLiteParameter imgParam = new SQLiteParameter("@img");
			command.Parameters.Add(imgParam);
			imgParam.Value = data;
			SQLiteParameter urlParam = new SQLiteParameter("@url");
			command.Parameters.Add(urlParam);
			urlParam.Value = url;

			object o = command.ExecuteNonQuery();
			command.Dispose();
			return ((int)o) != 0;
		}

		public int GetRecordsCount()
		{
			SQLiteCommand command = m_sqlCon.CreateCommand();
			command.CommandText = "SELECT COUNT(*) FROM [ImageCache]";
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