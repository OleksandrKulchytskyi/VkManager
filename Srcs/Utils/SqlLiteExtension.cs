using System.Data;
using System.Data.SQLite;

namespace Utils
{
	public static class SqlLiteExtension
	{
		public static DataTable ExecuteQuery(string dbName, string sql)
		{
			// Validate SQL
			if (string.IsNullOrWhiteSpace(sql))
			{
				return null;
			}
			else
			{
				if (!sql.EndsWith(";"))
				{
					sql += ";";
				}

				SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", dbName));
				connection.Open();
				SQLiteCommand cmd = new SQLiteCommand(connection);
				cmd.CommandText = sql;
				DataTable dt = new DataTable();
				SQLiteDataReader reader = cmd.ExecuteReader();
				dt.Load(reader);
				reader.Close();
				connection.Close();
				return dt;
			}
		}
	}
}