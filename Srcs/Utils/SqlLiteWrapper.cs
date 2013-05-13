using LogModule;
using System;
using System.Collections;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace Utils
{
	public class SqlLiteWrapper : IDisposable
	{
		private bool disposing = false;

		public string DataSourceConncection
		{
			get;
			set;
		}

		public SQLiteConnectionStringBuilder ConnectionBuilder
		{
			get;
			set;
		}

		public SQLiteConnection SqlCon
		{
			get;
			private set;
		}

		public SqlLiteWrapper()
		{
			this.DataSourceConncection = string.Empty;
			this.SqlCon = null;
			this.ConnectionBuilder = null;
		}

		public SqlLiteWrapper(string conString)
		{
			if (!string.IsNullOrEmpty(conString))
			{
				this.DataSourceConncection = conString;
				this.SqlCon = new SQLiteConnection(this.DataSourceConncection);
				this.SqlCon.Open();
			}
		}

		public SqlLiteWrapper(string conString, bool isFile)
		{
			if (!string.IsNullOrEmpty(conString))
			{
				if (!isFile)
				{
					this.DataSourceConncection = conString;
					this.SqlCon = new SQLiteConnection(this.DataSourceConncection);
					this.SqlCon.Open();
				}
				else
				{
					this.ConnectionBuilder = new SQLiteConnectionStringBuilder
					{
						Version = 3,
						DataSource = conString,
						BinaryGUID = true,
						DefaultTimeout = 100
					};
					this.SqlCon = new SQLiteConnection(ConnectionBuilder.ConnectionString);
				}
			}
		}

		public SqlLiteWrapper(SQLiteConnectionStringBuilder builder)
		{
			if (builder != null)
			{
				this.ConnectionBuilder = builder;
				this.SqlCon = new SQLiteConnection(this.ConnectionBuilder.ConnectionString);
				this.SqlCon.Open();
			}
		}

		/// <summary>
		/// Execute non query to DB
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>row count processed</returns>
		public int ExecuteNonQuery(string sql)
		{
			SQLiteCommand mycommand = new SQLiteCommand(this.SqlCon);
			mycommand.CommandText = sql;
			int rowsUpdated = mycommand.ExecuteNonQuery();

			return rowsUpdated;
		}

		/// <summary>
		/// Execute scalar query to DB
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>Result of scalar query</returns>
		public string ExecuteScalar(string sql)
		{
			SQLiteCommand mycommand = new SQLiteCommand(SqlCon);
			mycommand.CommandText = sql;
			object value = mycommand.ExecuteScalar();
			if (value != null)
			{
				return value.ToString();
			}

			return "";
		}

		/// <summary>
		/// Fetch all data from table using specify criteria
		/// </summary>
		/// <param name="Table"></param>
		/// <param name="where"></param>
		/// <param name="etc"></param>
		/// <returns></returns>
		public DataTable FetchAll(string Table, string where, string etc)
		{
			DataTable dt = new DataTable();
			//create sql query string
			string sql = string.Format("SELECT * FROM {0} {1} {2}", Table, where, etc);
			ConnectionState previousConnectionState = ConnectionState.Closed;

			try
			{
				previousConnectionState = this.SqlCon.State;
				if (SqlCon.State == ConnectionState.Closed)
				{
					SqlCon.Open();
				}
				SQLiteCommand command = new SQLiteCommand(sql, SqlCon);
				SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
				//Fill table
				adapter.Fill(dt);
			}
			catch (Exception ex)
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Error, ex.Message);
				dt = null;
			}

			//возвращаем таблицу
			return dt;
		}

		public DataTable FetchAll(string table)
		{
			return FetchAll(table, "", "");
		}

		public DataTable FetchAll(string table, string where)
		{
			return FetchAll(table, where, "");
		}

		/// <summary>
		/// Fetch all data by column table using specify criteria
		/// </summary>
		/// <param name="table"></param>
		/// <param name="columns"></param>
		/// <param name="where"></param>
		/// <param name="etc"></param>
		/// <returns></returns>
		public DataTable FetchByColumn(string table, string[] columns, string where, string etc)
		{
			DataTable dt = new DataTable();
			StringBuilder columnsBuilder = new StringBuilder();
			if (columns == null || columns.Length == 0)
				columnsBuilder.Append("*");
			else
			{
				bool ifFirst = true;
				//Adding all colmns to one string
				foreach (string col in columns)
				{
					if (ifFirst)
					{
						columnsBuilder.Append(col);
						ifFirst = false;
					}
					else
					{
						columnsBuilder.Append(string.Format(",{0}", col));
					}
				}
			}
			string sql = string.Format("SELECT {0} FROM {1} {2} {3}", columnsBuilder.ToString(), table, where, etc);
			ConnectionState previousConnectionState = ConnectionState.Closed;

			try
			{
				previousConnectionState = SqlCon.State;
				if (SqlCon.State == ConnectionState.Closed)
				{
					SqlCon.Open();
				}
				SQLiteCommand command = new SQLiteCommand(sql, SqlCon);
				SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
				adapter.Fill(dt);
			}
			catch (Exception ex)
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Error, ex.Message);
				dt = null;
			}

			return dt;
		}

		public DataTable FetchByColumn(string table, string[] columns, string where)
		{
			return FetchByColumn(table, columns, where, string.Empty);
		}

		public bool Insert(string table, ParametersCollection parameters)
		{
			ConnectionState previousConnectionState = ConnectionState.Closed;
			bool result = false;
			try
			{
				previousConnectionState = SqlCon.State;
				if (SqlCon.State == ConnectionState.Closed)
				{
					SqlCon.Open();
				}
				SQLiteCommand command = new SQLiteCommand(SqlCon);
				bool ifFirst = true;
				StringBuilder queryColumns = new StringBuilder();
				StringBuilder queryValues = new StringBuilder();
				queryColumns.Append("("); //список полей, в которые вставляются новые значения
				queryValues.Append("(");  //список значений для этих полей
				foreach (Parameter iparam in parameters)
				{
					//Add new parameter
					command.Parameters.Add("@" + iparam.ColumnName, iparam.DbType).Value = iparam.Value;
					//merge columns and values into one string
					if (ifFirst)
					{
						queryColumns.Append(iparam.ColumnName);
						queryValues.Append(string.Format("@{0}", iparam.ColumnName));
						ifFirst = false;
					}
					else
					{
						queryColumns.Append(string.Format(",{0}", iparam.ColumnName));
						queryValues.Append(string.Format(",@{0}", iparam.ColumnName));
					}
				}
				queryColumns.Append(")");
				queryValues.Append(")");

				//create new query string
				string sql = string.Format("INSERT INTO {0} {1} VALUES {2}", table, queryColumns.ToString(), queryValues.ToString());
				command.CommandText = sql;
				int count = command.ExecuteNonQuery();
				command.Dispose();
				result = true;
			}
			catch (Exception ex)
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Error, ex.Message);
				result = false;
			}

			return result;
		}

		public bool Update(string tablename, ParametersCollection collection, object[] whereparams, string whereseparator)
		{
			ConnectionState previousConnectionState = ConnectionState.Closed;
			bool result = false;
			try
			{
				//проверяем переданные аргументы
				if (whereparams.Length == 0) throw (new SqlLiteWarningException("Ошибка! Не указано ни одно условие"));
				if (whereparams.Length > 0 && whereseparator.Trim().Length == 0) throw (new SqlLiteWarningException("При использовании нескольких условий, требуется указать разделитель OR или AND"));

				previousConnectionState = SqlCon.State;
				if (SqlCon.State == ConnectionState.Closed)
				{
					SqlCon.Open();
				}

				int i = 0;
				//готовим переменную для сбора полей и их значений
				string sql_params = string.Empty;
				bool ifFirst = true;
				SQLiteCommand command = new SQLiteCommand(SqlCon);
				//в цикле создаем строку запроса
				foreach (Parameter param in collection)
				{
					if (ifFirst)
					{
						sql_params = param.ColumnName + " = @param" + i;
						ifFirst = false;
					}
					else
					{
						sql_params += "," + param.ColumnName + " = @param" + i;
					}
					//и добавляем параметры с таким же названием
					command.Parameters.Add("@param" + i, param.DbType).Value = param.Value;
					i++;
				}

				//условия для запроса
				string sql_where = string.Empty;
				ifFirst = true;
				//собираем строку с условиями
				foreach (object item in whereparams)
				{
					if (ifFirst)
					{
						sql_where = item.ToString();
						ifFirst = false;
					}
					else
					{
						sql_where += " " + whereseparator + " " + item;
					}
				}
				sql_where = "WHERE " + sql_where;
				//собираем запрос воедино
				command.CommandText = string.Format("UPDATE {0} SET {1} {2}", tablename, sql_params, sql_where);
				//выполняем запрос
				command.ExecuteNonQuery();
			}
			catch (SqlLiteWarningException message)
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Error, message.Message);
				result = false;
			}
			catch (Exception error)
			{
				LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Error, error.Message);
				result = false;
			}
			return result;
		}

		public void Dispose()
		{
			if (!disposing)
			{
				disposing = true;
				if (SqlCon != null && SqlCon.State == ConnectionState.Open)
				{
					SqlCon.Close();
				}

				if (this.ConnectionBuilder != null)
					ConnectionBuilder = null;

				if (!string.IsNullOrEmpty(this.DataSourceConncection))
					this.DataSourceConncection = null;

				GC.SuppressFinalize(this);
			}
		}
	}

	#region For Parameters

	public class Parameter
	{
		#region Поля

		private string _columnName;
		private object _value;
		private DbType _dbType;

		#endregion Поля

		/// <summary>
		/// Значение
		/// </summary>
		public object Value
		{
			get { return _value; }
			set { _value = value; }
		}

		/// <summary>
		/// Название поля в базе данных
		/// </summary>
		public string ColumnName
		{
			get { return _columnName; }
			set { _columnName = value; }
		}

		/// <summary>
		/// Тип передаваемого значения
		/// </summary>
		public DbType DbType
		{
			get { return _dbType; }
			set { _dbType = value; }
		}
	}

	public class ParametersCollection : CollectionBase
	{
		/// <summary>
		/// Добавить параметр в коллекцию
		/// </summary>
		/// <param name="iparam">Новый параметр</param>
		public virtual void Add(Parameter iparam)
		{
			//добавляем в общую коллекцию
			this.List.Add(iparam);
		}

		/// <summary>
		/// Добавить параметр в коллекцию
		/// </summary>
		/// <param name="columnName">Имя поля/колонки</param>
		/// <param name="value">Значине</param>
		/// <param name="dbType">Тип значения</param>
		public virtual void Add(string columnName, object value, DbType dbType)
		{
			//Инициализируем объект с параметром
			Parameter iparam = new Parameter();
			//присваиваем название поля
			iparam.ColumnName = columnName;
			//присваиваем значение
			iparam.Value = value;
			//присваиваем тип значения
			iparam.DbType = dbType;
			//добавляем в общую коллекцию
			//List описан в "родителе"
			this.List.Add(iparam);
		}

		/// <summary>
		/// Получить элемент по индексу
		/// </summary>
		/// <param name="Index">Индекс</param>
		/// <returns>Параметр</returns>
		public virtual Parameter this[int Index]
		{
			get
			{
				//возвращает элемент по индексу
				//используется в конструкции foreach
				return (Parameter)this.List[Index];
			}
		}
	}

	#endregion For Parameters

	#region SQLite Exceptions

	public class SqlLiteWarningException : Exception
	{
		private string _messageText;

		public string MessageText
		{
			get { return _messageText; }
		}

		public SqlLiteWarningException(string messagetext)
			: base()
		{
			_messageText = messagetext;
		}
	}

	#endregion SQLite Exceptions
}