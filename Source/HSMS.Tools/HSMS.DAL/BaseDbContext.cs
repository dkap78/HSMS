using HSMS.DB.Models;
using HSMS.Security;
using HSMS.Utility;
using Npgsql;
using System.Data;
using System.Reflection;

namespace HSMS.DAL
{
    public abstract class BaseDbContext : IDisposable
    {
        #region Local Variables
        protected ConnectionSettings _connectionSettings;
        protected NpgsqlConnection _connection;
        protected NpgsqlTransaction _transaction;

        public IAppUserContext AppUser { get; }
        public AuditService Audit { get; }

        private HSMS.DB.Interface.ILogger _logger;
        #endregion

        #region Properties
        public NpgsqlConnection Connection
        {
            get
            {
                return _connection;
            }
        }
        public NpgsqlTransaction Transaction
        {
            get
            {
                return _transaction;
            }
        }
        #endregion

        #region Constructor
        /// Default constructor which uses the "DefaultConnection" connectionString
        /// </summary>
        public BaseDbContext(ConnectionSettings settings, IAppUserContext appUser)
        {
            _connectionSettings = settings;
            _connection = new NpgsqlConnection(_connectionSettings.DefaultConnection);

            this.AppUser = appUser;
            this.Audit = new AuditService(this, this.AppUser);
        }
        public BaseDbContext(NpgsqlConnection connection, NpgsqlTransaction? tran = null)
        {
            _connectionSettings = null;
            _connection = connection;
            _transaction = tran;
        }
        #endregion

        #region Connection Methods
        /// <summary>
        /// Closes a connection if open and also if no transaction is on.
        /// </summary>
        public void EnsureConnectionClosed()
        {
            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed && _transaction == null)
                {
                    _connection.Close();
                }
            }
        }

        /// <summary>
        /// Opens a connection if not open
        /// </summary>
        protected bool EnsureConnectionOpen()
        {
            bool blnConnOpen = false;
            var retries = 3;

            if (_connection.State == ConnectionState.Open)
            {
                blnConnOpen = true;
            } else
            {
                while (retries >= 0 && _connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                    retries--;
                    Thread.Sleep(30);
                }

                blnConnOpen = (_connection != null && _connection.State == ConnectionState.Open);
            }

            return blnConnOpen;
        }
        #endregion

        #region Transaction Methods
        public bool BeginTransaction()
        {
            bool blnRetValue = false;

            EnsureConnectionOpen();

            if (_transaction != null)
            {
                throw new InvalidOperationException("Cannot start multiple transaction. There is already transaction open, please either Commit or Rollback it first then start new transaction.");
            }

            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _transaction = _connection.BeginTransaction();
                blnRetValue = true;
            }

            return blnRetValue;
        }
        public bool CommitTransaction()
        {
            bool blnRetValue = false;

            if (_transaction != null)
            {
                _transaction.Commit();
                CloseTransaction();
                blnRetValue = true;
            } else
            {
                throw new InvalidOperationException("No transaction open.");
            }

            return blnRetValue;
        }
        public bool RollbackTransaction()
        {
            bool blnRetValue = false;

            if (_transaction != null)
            {
                _transaction.Rollback();
                CloseTransaction();
                blnRetValue = true;
            } else
            {
                throw new InvalidOperationException("No transaction open.");
            }

            return blnRetValue;
        }
        protected void CloseTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            EnsureConnectionClosed();
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            BeginTransaction();

            try
            {
                await action();
                CommitTransaction();
            } catch
            {
                RollbackTransaction();
                throw;
            }
        }
        #endregion

        #region Methods for executing query and return the result
        /// <summary>
        /// Executes a non-query sql statement
        /// </summary>
        /// <param name="commandText">The sql query to execute</param>
        /// <param name="parameters">Optional parameters to pass to the query</param>
        /// <returns>The count of records affected by the sql statement</returns>
        /// 
        public int Execute(string commandText, NpgsqlParameter[] parameters = null)
        {
            return Execute(commandText, CommandType.Text, parameters);
        }
        public int Execute(string commandText, CommandType cmdType, NpgsqlParameter[] parameters = null)
        {
            int result = 0;

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            try
            {
                if (EnsureConnectionOpen())
                {
                    var command = CreateCommand(commandText, cmdType, parameters);

                    result = command.ExecuteNonQuery();
                }
            } catch (Exception ex)
            {
                CustomDataException cde = new CustomDataException(ex);
                throw cde;
            } finally
            {
                EnsureConnectionClosed();
            }

            return result;
        }
        public async Task<int> ExecuteAsync(string commandText, CommandType cmdType = CommandType.StoredProcedure, NpgsqlParameter[] parameters = null)
        {
            int result = 0;

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            try
            {
                if (EnsureConnectionOpen())
                {
                    using var command = CreateCommand(commandText, cmdType, parameters);
                    result = await command.ExecuteNonQueryAsync();

                    await LogAuditAsync(commandText, parameters);
                } 
            } catch (Exception ex)
            {
                CustomDataException cde = new CustomDataException(ex);
                throw cde;
            } finally
            {
                EnsureConnectionClosed();
            }

            return result;
        }

        public NpgsqlDataReader ExecuteReader(string commandText, NpgsqlParameter[] parameters = null)
        {
            return ExecuteReader(commandText, CommandType.Text, parameters);
        }
        /// <summary>
        /// Executes a sql statement and returns as SqlDataReader object
        /// </summary>
        /// <param name="commandText">The sql query to execute</param>
        /// <param name="parameters">Optional parameters to pass to the query</param>
        /// <returns>Reference of SqlDataReader class returned by ExecuteReader method of Sql.<br>
        /// NOTE: As method returns SqlDataReader object, it does not close the connection. So
        /// make sure to close connection (by calling EnsureConnectionClose method) after data is
        /// read from SqlDataReader.
        /// </returns>
        /// 
        public NpgsqlDataReader ExecuteReader(string commandText, CommandType cmdType = CommandType.StoredProcedure, NpgsqlParameter[] parameters = null)
        {
            NpgsqlDataReader dataReader = null;

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            try
            {
                if (EnsureConnectionOpen())
                {
                    var command = CreateCommand(commandText, cmdType, parameters);
                    dataReader = command.ExecuteReader();
                }
            } finally
            {
            }

            return dataReader;
        }

        public object QueryValue(string commandText, NpgsqlParameter[] parameters = null)
        {
            return this.QueryValue(commandText, CommandType.Text, parameters);
        }
        /// <summary>
        /// Executes a sql query that returns a single scalar value as the result.
        /// </summary>
        /// <param name="commandText">The sql query to execute</param>
        /// <param name="parameters">Optional parameters to pass to the query</param>
        /// <returns></returns>
        public object QueryValue(string commandText, CommandType cmdType, NpgsqlParameter[] parameters = null)
        {
            object result = null;

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            try
            {
                EnsureConnectionOpen();
                var command = CreateCommand(commandText, cmdType, parameters);
                result = command.ExecuteScalar();
            } finally
            {
                EnsureConnectionClosed();
            }

            return result;
        }
        public async Task<object?> QueryValueAsync(string commandText, CommandType cmdType = CommandType.StoredProcedure, NpgsqlParameter[]? parameters = null)
        {
            EnsureConnectionOpen();

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            try
            {
                using var cmd = CreateCommand(commandText, cmdType, parameters);
                
                return await cmd.ExecuteScalarAsync();
            } finally
            {
                EnsureConnectionClosed();
            }
        }

        public List<TElement> SqlQuery<TElement>(string commandText, NpgsqlParameter[] parameters = null)
        {
            return this.SqlQuery<TElement>(commandText, CommandType.Text, parameters);
        }
        /// <summary>
        /// Executes a SQL query that returns a list of rows as the result.
        /// </summary>
        /// <param name="commandText">The sql query to execute</param>
        /// <param name="parameters">Parameters to pass to the sql query</param>
        /// <returns>A list of a Dictionary of Key, values pairs representing the 
        /// ColumnName and corresponding value</returns>
        public List<TElement> SqlQuery<TElement>(string commandText, CommandType cmdType, NpgsqlParameter[] parameters = null)
        {
            List<TElement> rows = new List<TElement>();
            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
            TElement element = default(TElement);
            Type t = typeof(TElement);

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            foreach (PropertyInfo p in t.GetProperties())
            {
                properties.Add(p.Name, p);
            }

            try
            {
                if (EnsureConnectionOpen())
                {
                    var command = CreateCommand(commandText, cmdType, parameters);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            element = Activator.CreateInstance<TElement>();
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                string strColumnName = reader.GetName(i);

                                if (properties.ContainsKey(strColumnName))
                                {
                                    PropertyInfo prop = properties[strColumnName];

                                    var columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i);

                                    if (prop.PropertyType != reader.GetFieldType(i) && columnValue != null)
                                    {
                                        Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                                        if (targetType.IsEnum)
                                        {
                                            columnValue = Enum.Parse(targetType, Enum.GetName(targetType, columnValue), false);
                                        } else
                                        {
                                            columnValue = Convert.ChangeType(columnValue, targetType);
                                        }
                                    }

                                    prop.SetValue(element, columnValue, null);
                                }
                            }

                            rows.Add(element);
                        }
                    }
                }
            } finally
            {
                EnsureConnectionClosed();
            }

            return rows;
        }
        public async Task<List<T>> QueryAsync<T>(string commandText, CommandType cmdType = CommandType.StoredProcedure, NpgsqlParameter[]? parameters = null)
        {
            var list = new List<T>();
            var props = typeof(T).GetProperties().ToDictionary(p => p.Name);

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            EnsureConnectionOpen();

            try
            {
                using var cmd = CreateCommand(commandText, cmdType, parameters);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var item = Activator.CreateInstance<T>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var col = reader.GetName(i);

                        if (!props.TryGetValue(col, out var prop))
                            continue;

                        var val = reader.IsDBNull(i) ? null : reader.GetValue(i);

                        if (val != null && prop.PropertyType != reader.GetFieldType(i))
                        {
                            var target = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                            if (target.IsEnum)
                                val = Enum.Parse(target, val.ToString()!);
                            else
                                val = Convert.ChangeType(val, target);
                        }

                        prop.SetValue(item, val);
                    }

                    list.Add(item);
                }
            } finally
            {
                EnsureConnectionClosed();
            }

            return list;
        }

        /// <summary>
        /// Helper method to return query a string value 
        /// </summary>
        /// <param name="commandText">The sql query to execute</param>
        /// <param name="parameters">Parameters to pass to the sql query</param>
        /// <returns>The string value resulting from the query</returns>
        public string GetStrValue(string commandText, NpgsqlParameter[] parameters = null)
        {
            string value = QueryValue(commandText, parameters) as string;
            return value;
        }
        #endregion

        #region Data Retrieval Methods
        public BaseModel PopulateBaseDataFromDataReader(NpgsqlDataReader dataReader, BaseModel model)
        {
            if (dataReader.HasColumn("id") && !(dataReader["id"] is System.DBNull))
            {
                model.Id = Guid.Parse(dataReader["id"].ToString());
            }

            if (dataReader.HasColumn("created_by") && !(dataReader["created_by"] is System.DBNull))
            {
                model.CreatedBy = Guid.Parse(dataReader["created_by"].ToString());
            }

            if (dataReader.HasColumn("created_date_time") && !(dataReader["created_date_time"] is System.DBNull))
            {
                model.CreatedAt = DateTime.Parse(dataReader["created_date_time"].ToString());
            }

            if (dataReader.HasColumn("last_updated_by") && !(dataReader["last_updated_by"] is System.DBNull))
            {
                model.UpdatedAt = Guid.Parse(dataReader["last_updated_by"].ToString());
            }

            if (dataReader.HasColumn("last_updated_date_time") && !(dataReader["last_updated_date_time"] is System.DBNull))
            {
                model.LastUpdationDateTime = DateTime.Parse(dataReader["last_updated_date_time"].ToString());
            }

            return model;
        }
        #endregion

        #region ILogger Implementation
        /*
        public void LogMailSendStatus(MailLog log)
        {
            NpgsqlParameter[] parameters = new NpgsqlParameter[10];
            parameters[0] = new NpgsqlParameter() { ParameterName = "p_MailFrom", NpgsqlDbType = NpgsqlDbType.Varchar, Value = (log.MailFrom ?? ""), Direction = ParameterDirection.Input };
            parameters[1] = new NpgsqlParameter() { ParameterName = "p_MailTo", NpgsqlDbType = NpgsqlDbType.Varchar, Value = (log.MailTo ?? ""), Direction = ParameterDirection.Input };
            parameters[2] = new NpgsqlParameter() { ParameterName = "p_MailCc", NpgsqlDbType = NpgsqlDbType.Varchar, Value = (log.MailCc ?? ""), Direction = ParameterDirection.Input };
            parameters[3] = new NpgsqlParameter() { ParameterName = "p_MailBcc", NpgsqlDbType = NpgsqlDbType.Varchar, Value = (log.MailBcc ?? ""), Direction = ParameterDirection.Input };
            parameters[4] = new NpgsqlParameter() { ParameterName = "p_MailSubject", NpgsqlDbType = NpgsqlDbType.Varchar, Value = (log.MailSubject ?? ""), Direction = ParameterDirection.Input };
            parameters[5] = new NpgsqlParameter() { ParameterName = "p_MailPlainBody", NpgsqlDbType = NpgsqlDbType.Varchar, Value = (log.MailPlainBody ?? ""), Direction = ParameterDirection.InputOutput };
            parameters[6] = new NpgsqlParameter() { ParameterName = "p_MailHtmlBody", NpgsqlDbType = NpgsqlDbType.Varchar, Value = (log.MailHtmlBody ?? ""), Direction = ParameterDirection.InputOutput };
            parameters[7] = new NpgsqlParameter() { ParameterName = "p_MailPriority", NpgsqlDbType = NpgsqlDbType.Varchar, Value = log.MailPriority, Direction = ParameterDirection.InputOutput };
            parameters[8] = new NpgsqlParameter() { ParameterName = "p_SendStatus", NpgsqlDbType = NpgsqlDbType.Integer, Value = log.SendStatus, Direction = ParameterDirection.InputOutput };
            parameters[9] = new NpgsqlParameter() { ParameterName = "p_StatusText", NpgsqlDbType = NpgsqlDbType.Varchar, Value = (log.StatusText ?? ""), Direction = ParameterDirection.InputOutput };

            try
            {
                this.Execute("sp_addmaillog", CommandType.StoredProcedure, parameters);
            } catch
            {
                //Ignore Error
            }

            this.EnsureConnectionClosed();
        }
        */

        public void LogInfo(string strMessage)
        {
            if (this._logger != null && this._logger.InfoEnabled && string.IsNullOrEmpty(strMessage) == false)
            {
                this._logger.Info(strMessage);
            }
        }
        public void LogDebug(string strMessage)
        {
            if (this._logger != null && this._logger.DebugEnabled && string.IsNullOrEmpty(strMessage) == false)
            {
                this._logger.Debug(strMessage);
            }
        }
        public void LogError(string strMessage)
        {
            if (this._logger != null && string.IsNullOrEmpty(strMessage) == false)
            {
                this._logger.Error(strMessage);
            }
        }

        private async Task LogAuditAsync(string sp, NpgsqlParameter[]? p)
        {
            if (_transaction == null)
                return;

            using var cmd = CreateCommand("sp_audit_insert", CommandType.StoredProcedure, new[]
            {
                new NpgsqlParameter("p_entity", sp),
                new NpgsqlParameter("p_changed_at", DateTime.UtcNow)
            });

            await cmd.ExecuteNonQueryAsync();
        }
        #endregion

        #region Utilities Methods
        /// <summary>
        /// Creates a sql Command with the given parameters
        /// </summary>
        /// <param name="strCommandText">The sql query to execute</param>
        /// <param name="parameters">Parameters to pass to the sql query</param>
        /// <returns></returns>
        protected NpgsqlCommand CreateCommand(string strCommandText, CommandType cmdType, NpgsqlParameter[] parameters)
        {
            NpgsqlCommand command = _connection.CreateCommand();
            command.CommandType = cmdType;
            command.CommandText = strCommandText;

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            } else
            {
                command.Parameters.Clear();
            }

            if (_transaction != null)
            {
                command.Transaction = _transaction;
            }

            return command;
        }
        #endregion

        #region Implementation of IDisposable Interface
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public virtual void Dispose(bool blnDisposing)
        {
            if (blnDisposing)
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }
        #endregion
    }
    public class ConnectionSettings
    {
        public string DefaultConnection { get; set; } = string.Empty;
    }
}
