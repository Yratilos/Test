using Kdbndp;
using KdbndpTypes;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace Test.KingBase
{
    /// <summary>
    /// DbHelperKingBase
    /// </summary>
    public class DbHelperKingBase
    {
        private string connectionString;
        public DbHelperKingBase(string connectionStr)
        {
            if (ConfigurationManager.ConnectionStrings[connectionStr] is null)
            {
                connectionString = GetConnection(connectionStr);
            }
            else
            {
                connectionString = ConfigurationManager.ConnectionStrings[connectionStr].ToString();
            }
        }

        public void AddInParameter(DbCommand command, string name, DbType dbType, object value)
        {
            value = value == null ? DBNull.Value : value;
            KdbndpParameter sqlParameter = new KdbndpParameter(name, value);
            sqlParameter.Direction = ParameterDirection.Input;
            sqlParameter.DbType = dbType;
            command.Parameters.Add(sqlParameter);
        }

        public void AddOutParameter(DbCommand command, string name, DbType dbType, int size)
        {
            KdbndpParameter sqlParameter = new KdbndpParameter();
            sqlParameter.ParameterName = name;
            sqlParameter.Direction = ParameterDirection.Output;
            sqlParameter.DbType = dbType;
            sqlParameter.Size = size;
            command.Parameters.Add(sqlParameter);
        }

        public void AddParameter(DbCommand command, string name, DbType dbType, ParameterDirection direction, object value)
        {
            value = value == null ? DBNull.Value : value;
            KdbndpParameter sqlParameter = new KdbndpParameter(name, value);
            sqlParameter.Direction = direction;
            sqlParameter.DbType = dbType;
            command.Parameters.Add(sqlParameter);
        }

        public virtual DbConnection CreateConnection()
        {
            KdbndpConnection connection = new KdbndpConnection(connectionString);
            return connection;
        }

        public int Delete(List<object> listobj)
        {
            Hashtable KdbndpList = new Hashtable();
            foreach (object obj in listobj)
            {
                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                string where = "";
                List<KdbndpParameter> list = new List<KdbndpParameter>();
                foreach (PropertyInfo p in properties)
                {
                    if (p.PropertyType == typeof(int) ||
                        p.PropertyType == typeof(long) ||
                        p.PropertyType == typeof(string) ||
                        p.PropertyType == typeof(bool) ||
                        p.PropertyType == typeof(float) ||
                        p.PropertyType == typeof(double) ||
                        p.PropertyType == typeof(DateTime))
                    {
                        object value = p.GetValue(obj);
                        if (value != null)
                        {
                            where += p.Name + "=" + ":" + p.Name + " and ";
                            KdbndpParameter sqlPara = new KdbndpParameter(p.Name, value);
                            list.Add(sqlPara);
                        }
                    }
                }
                where = where.Substring(0, where.Length - 4);
                string sql = "delete from dbo." + type.Name + " where " + where;

                KdbndpParameter[] sqlParameters = list.ToArray();
                KdbndpList.Add(sqlParameters, sql);
            }
            return ExecuteSqlTran(KdbndpList);
        }

        public DataTable Execute<T>(string value, T model)
        {
            KdbndpCommand cmd = (KdbndpCommand)GetStoredProcCommand(value);
            List<string> lst = new List<string>();
            DataTable dataTable = new DataTable();
            foreach (var item in model.GetType().GetProperties())
            {
                try
                {
                    lst.Add($"'{item.GetValue(model)}'");

                    AddInParameter(cmd, item.Name, item.GetValue(model));
                }
                catch
                {
                    throw new Exception($"参数添加时出错,参数名{item.Name}");
                }
            }
            var sql = $"select {value}({string.Join(",", lst)})";
            try
            {
                DataSet dataSet = new DataSet();
                using (KdbndpConnection connection = new KdbndpConnection(connectionString))
                {
                    connection.Open();
                    KdbndpDataAdapter sqlDataAdapter = new KdbndpDataAdapter();
                    cmd.Connection = connection;
                    sqlDataAdapter.SelectCommand = cmd;
                    sqlDataAdapter.Fill(dataSet);
                }
                DataSet ds = ExecuteDataSet(cmd);
                if (ds is null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                {
                    return dataTable;
                }
                dataTable = ds.Tables[0];
                return dataTable;
            }
            catch
            {
                throw new Exception($"执行数据库操作出错:{sql}");
            }
        }

        public virtual DataSet ExecuteDataSet(DbCommand command)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                connection.Open();
                using (KdbndpTransaction transaction = connection.BeginTransaction())
                {
                    DataSet dataSet = new DataSet();
                    try
                    {
                        KdbndpDataAdapter sqlDataAdapter = new KdbndpDataAdapter();
                        command.Connection = connection;
                        command.Transaction = transaction;
                        sqlDataAdapter.SelectCommand = (KdbndpCommand)command;
                        sqlDataAdapter.Fill(dataSet);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        connection.Close();
                    }
                    return dataSet;
                }
            }
        }

        public virtual DataSet ExecuteDataSet(DbCommand command, DbTransaction Transaction)
        {
            DataSet dataSet = new DataSet();
            KdbndpDataAdapter sqlDataAdapter = new KdbndpDataAdapter();
            command.Connection = Transaction.Connection;
            command.Transaction = Transaction;
            sqlDataAdapter.SelectCommand = (KdbndpCommand)command;
            sqlDataAdapter.Fill(dataSet);
            return dataSet;
        }

        public virtual int ExecuteNonQuery(DbCommand command)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                connection.Open();
                using (KdbndpTransaction transaction = connection.BeginTransaction())
                {
                    int rowsAffected = 0;
                    try
                    {
                        command.Connection = connection;
                        command.Transaction = transaction;
                        rowsAffected = command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        connection.Close();
                    }
                    return rowsAffected;
                }
            }
        }

        public virtual int ExecuteNonQuery(DbCommand command, DbTransaction Transaction)
        {
            command.Connection = Transaction.Connection;
            command.Transaction = Transaction;
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected;
        }

        public int ExecuteNonQuery(string sql, IDataParameter[] parameters)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                KdbndpCommand command = (KdbndpCommand)GetSQLStringCommand(sql);
                command.Parameters.AddRange(parameters);
                connection.Open();
                using (KdbndpTransaction transaction = connection.BeginTransaction())
                {
                    int rowsAffected = 0;
                    try
                    {
                        command.Connection = connection;
                        command.Transaction = transaction;
                        rowsAffected = command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        connection.Close();
                    }
                    return rowsAffected;
                }
            }
        }

        public object ExecuteScalar(string sql, params IDataParameter[] param)
        {
            using (KdbndpConnection conn = new KdbndpConnection(connectionString))
            {
                conn.Open();
                using (KdbndpCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddRange(param);
                    return cmd.ExecuteScalar();
                }
            }
        }

        public virtual object ExecuteScalar(DbCommand command, DbTransaction Transaction)
        {
            command.Connection = Transaction.Connection;
            command.Transaction = Transaction;
            object returnValue = command.ExecuteScalar();
            return returnValue;
        }

        public int ExecuteSql(string sql)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                KdbndpCommand command = (KdbndpCommand)GetSQLStringCommand(sql);
                connection.Open();
                command.Connection = connection;
                int rowsAffected = command.ExecuteNonQuery();
                connection.Close();
                return rowsAffected;
            }
        }

        public int ExecuteSqlTran(List<string> SQLStringList)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                connection.Open();
                KdbndpCommand cmd = new KdbndpCommand();
                cmd.Connection = connection;
                KdbndpTransaction tx = connection.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    int count = 0;
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n];
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return count;
                }
                catch
                {
                    tx.Rollback();
                    return 0;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        public int ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (KdbndpConnection conn = new KdbndpConnection(connectionString))
            {
                conn.Open();
                using (KdbndpTransaction trans = conn.BeginTransaction())
                {
                    KdbndpCommand cmd = new KdbndpCommand();
                    try
                    {
                        int count = 0;
                        //ѭ��
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Value.ToString();
                            KdbndpParameter[] cmdParms = (KdbndpParameter[])myDE.Key;
                            cmd = (KdbndpCommand)GetSQLStringCommand(cmdText);
                            cmd.Parameters.AddRange(cmdParms);
                            count += ExecuteNonQuery(cmd, trans);
                            //PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            //count += cmd.ExecuteNonQuery();
                            //cmd.Parameters.Clear();
                        }
                        trans.Commit();
                        return count;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public int Exists(string strSql)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                using (KdbndpCommand cmd = new KdbndpCommand(strSql, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if (Equals(obj, null) || Equals(obj, DBNull.Value))
                        {
                            return -1;
                        }
                        else
                        {
                            int rows = Convert.ToInt32(cmd.ExecuteScalar());
                            return rows;
                        }
                    }
                    catch (Exception e)
                    {
                        connection.Close();
                        throw e;
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        public object GetParameterValue(DbCommand command, string name)
        {
            return command.Parameters[name].Value;
        }

        public virtual DbCommand GetSQLStringCommand(string sql)
        {
            KdbndpCommand command = new KdbndpCommand(sql);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 180;
            return command;
        }

        public virtual DbCommand GetStoredProcCommand(string storedProcedureName)
        {
            KdbndpCommand command = new KdbndpCommand(storedProcedureName);
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 180;
            return command;
        }

        public string GetValue(string SQLString, params IDataParameter[] cmdParms)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                using (KdbndpCommand cmd = new KdbndpCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.Parameters.AddRange(cmdParms);
                        var obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        return obj.ToString();
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        public int Insert(List<object> listobj)
        {
            Hashtable KdbndpList = new Hashtable();
            foreach (object obj in listobj)
            {
                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();
                string values = "";
                string columns = "";
                List<KdbndpParameter> list = new List<KdbndpParameter>();
                foreach (PropertyInfo p in properties)
                {
                    if (p.PropertyType == typeof(int) ||
                        p.PropertyType == typeof(long) ||
                        p.PropertyType == typeof(string) ||
                        p.PropertyType == typeof(bool) ||
                        p.PropertyType == typeof(float) ||
                        p.PropertyType == typeof(double) ||
                        p.PropertyType == typeof(DateTime))
                    {
                        object value = p.GetValue(obj);
                        if (value != null)
                        {
                            columns += p.Name + ",";
                            values += ":" + p.Name + ",";
                            KdbndpParameter sqlPara = new KdbndpParameter(p.Name, value);
                            list.Add(sqlPara);
                        }
                    }
                }
                columns = columns.Substring(0, columns.Length - 1);
                values = values.Substring(0, values.Length - 1);
                string sql = "insert into dbo." + type.Name + "(" + columns + ")values(" + values + ")";

                KdbndpParameter[] sqlParameters = list.ToArray();
                KdbndpList.Add(sqlParameters, sql);
            }
            return ExecuteSqlTran(KdbndpList);
        }

        public DataSet Query(string sql)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                KdbndpCommand command = (KdbndpCommand)GetSQLStringCommand(sql);
                DataSet dataSet = new DataSet();
                connection.Open();
                KdbndpDataAdapter sqlDataAdapter = new KdbndpDataAdapter();
                command.Connection = connection;
                sqlDataAdapter.SelectCommand = command;
                sqlDataAdapter.Fill(dataSet);
                connection.Close();
                return dataSet;
            }
        }

        public DataSet Query(string sql, IDataParameter[] parameters)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                KdbndpCommand command = (KdbndpCommand)GetSQLStringCommand(sql);
                command.Parameters.AddRange(parameters);
                DataSet dataSet = new DataSet();
                connection.Open();
                KdbndpDataAdapter sqlDataAdapter = new KdbndpDataAdapter();
                command.Connection = connection;
                sqlDataAdapter.SelectCommand = command;
                sqlDataAdapter.Fill(dataSet);
                connection.Close();
                return dataSet;
            }
        }

        /// <summary>
        /// 执行函数
        /// </summary>
        /// <param name="procedurename">函数名</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public DataSet RunProcedure(string procedurename,params IDataParameter[] parameters)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                KdbndpCommand command = (KdbndpCommand)GetStoredProcCommand(procedurename);
                command.Parameters.AddRange(parameters);
                DataSet dataSet = new DataSet();
                connection.Open();
                KdbndpDataAdapter sqlDataAdapter = new KdbndpDataAdapter();
                command.Connection = connection;
                sqlDataAdapter.SelectCommand = command;
                sqlDataAdapter.Fill(dataSet);
                connection.Close();
                return dataSet;
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="storedProcName">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="rowsAffected">影响行数</param>
        /// <returns>影响行数</returns>
        public int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            rowsAffected = ExecuteNonQuery(storedProcName, parameters);
            return rowsAffected;
        }

        private static KdbndpCommand BuildIntCommand(KdbndpConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            KdbndpCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new KdbndpParameter("ReturnValue",
                KdbndpDbType.Integer, 4, null,
                ParameterDirection.ReturnValue, false, 0, 0, DataRowVersion.Default, null));
            return command;
        }

        private static KdbndpCommand BuildQueryCommand(KdbndpConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            KdbndpCommand command = new KdbndpCommand(storedProcName, connection);
            //command.CommandType = CommandType.StoredProcedure;
            foreach (KdbndpParameter parameter in parameters)
            {
                if (parameter != null)
                {
                    // 检查未分配值的输出参数,将其分配以DBNull.Value.
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        parameter.Value == null)
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);
                }
            }
            return command;
        }

        private void AddInParameter(KdbndpCommand command, string name, object value)
        {
            value = value == null ? DBNull.Value : value;
            KdbndpParameter sqlParameter = new KdbndpParameter(name, value);
            sqlParameter.Direction = ParameterDirection.Input;
            command.Parameters.Add(sqlParameter);
        }

        private string GetConnection(string connectionStr)
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = "App.config" };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            var connectionString = config.ConnectionStrings.ConnectionStrings[connectionStr].ToString();
            return connectionString;
        }
    }
}
