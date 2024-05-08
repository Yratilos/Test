using Kdbndp;
using KdbndpTypes;
using Microsoft.Data.SqlClient;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Reflection;

namespace SimCloudDAL
{
    /// <summary>
    /// 1
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

        private string GetConnection(string connectionStr)
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = "App.config" };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            var connectionString = config.ConnectionStrings.ConnectionStrings[connectionStr].ToString();
            return connectionString;
        }

        public virtual KdbndpConnection CreateConnection()
        {
            KdbndpConnection connection = new KdbndpConnection(connectionString);
            return connection;
        }

        public virtual KdbndpCommand GetStoredProcCommand(string storedProcedureName)
        {
            KdbndpCommand command = new KdbndpCommand(storedProcedureName);
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 180;
            return command;
        }

        public virtual KdbndpCommand GetSQLStringCommand(string sql)
        {
            KdbndpCommand command = new KdbndpCommand(sql);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 180;
            return command;
        }

        public void AddInParameter(KdbndpCommand command, string name, KdbndpDbType dbType, object value)
        {
            value = value == null ? DBNull.Value : value;
            KdbndpParameter sqlParameter = new KdbndpParameter(name, value);
            sqlParameter.Direction = ParameterDirection.Input;
            sqlParameter.KdbndpDbType = dbType;
            command.Parameters.Add(sqlParameter);
        }

        public void AddParameter(KdbndpCommand command, string name, KdbndpDbType dbType, ParameterDirection direction, object value)
        {
            value = value == null ? DBNull.Value : value;
            KdbndpParameter sqlParameter = new KdbndpParameter(name, value);
            sqlParameter.Direction = direction;
            sqlParameter.KdbndpDbType = dbType;
            command.Parameters.Add(sqlParameter);
        }

        public void AddOutParameter(KdbndpCommand command, string name, KdbndpDbType dbType, int size)
        {
            KdbndpParameter sqlParameter = new KdbndpParameter();
            sqlParameter.ParameterName = name;
            sqlParameter.Direction = ParameterDirection.Output;
            sqlParameter.KdbndpDbType = dbType;
            sqlParameter.Size = size;
            command.Parameters.Add(sqlParameter);
        }

        public object GetParameterValue(KdbndpCommand command, string name)
        {
            KdbndpParameter p1 = new KdbndpParameter();
            p1.ParameterName = name;
            command.Parameters.Add(p1);
            return command;
        }

        public virtual DataSet ExecuteDataSet(KdbndpCommand command)
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
                        sqlDataAdapter.SelectCommand = command;
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

        public virtual DataSet ExecuteDataSet(KdbndpCommand command, KdbndpTransaction Transaction)
        {
            DataSet dataSet = new DataSet();
            KdbndpDataAdapter sqlDataAdapter = new KdbndpDataAdapter();
            command.Connection = Transaction.Connection;
            command.Transaction = Transaction;
            sqlDataAdapter.SelectCommand = command;
            sqlDataAdapter.Fill(dataSet);
            return dataSet;
        }

        public virtual int ExecuteNonQuery(KdbndpCommand command)
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

        public virtual int ExecuteNonQuery(KdbndpCommand command, KdbndpTransaction Transaction)
        {
            command.Connection = Transaction.Connection;
            command.Transaction = Transaction;
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected;

        }

        public object ExecuteScalar(string sql, params KdbndpParameter[] param)
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

        public object ExecuteScalar(string sql)
        {
            using (KdbndpConnection conn = new KdbndpConnection(connectionString))
            {
                conn.Open();
                using (KdbndpCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    return cmd.ExecuteScalar();
                }
            }
        }

        public virtual object ExecuteScalar(KdbndpCommand command, KdbndpTransaction Transaction)
        {
            command.Connection = Transaction.Connection;
            command.Transaction = Transaction;
            object returnValue = command.ExecuteScalar();
            return returnValue;
        }

        public DataSet Query(string sql)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                KdbndpCommand command = GetSQLStringCommand(sql);
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
                KdbndpCommand command = GetSQLStringCommand(sql);
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

        public object ExecuteScalar(string sql, IDataParameter[] parameters)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                KdbndpCommand command = GetSQLStringCommand(sql);
                command.Parameters.AddRange(parameters);
                connection.Open();
                command.Connection = connection;
                object returnValue = command.ExecuteScalar();
                connection.Close();
                return returnValue;
            }
        }

        public int ExecuteNonQuery(string sql, IDataParameter[] parameters)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                KdbndpCommand command = GetSQLStringCommand(sql);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="procedurename">函数名</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet RunProcedure(string procedurename, IDataParameter[] parameters)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                KdbndpCommand command = GetStoredProcCommand(procedurename);
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
        /// 无法返回影响行数
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        /// <param name="rowsAffected"></param>
        /// <returns></returns>
        public int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                int result;
                connection.Open();
                KdbndpCommand command = BuildIntCommand(connection, storedProcName, parameters);
                rowsAffected = command.ExecuteNonQuery();
                result = (int)command.Parameters["ReturnValue"].KdbndpValue;
                connection.Close();
                return result;
            }
        }

        public int ExecuteSql(string sql)
        {
            using (KdbndpConnection connection = new KdbndpConnection(connectionString))
            {
                KdbndpCommand command = GetSQLStringCommand(sql);
                connection.Open();
                command.Connection = connection;
                int rowsAffected = command.ExecuteNonQuery();
                connection.Close();
                return rowsAffected;
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
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
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

        public int ExecuteSqlTran(List<String> SQLStringList)
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
                            cmd = GetSQLStringCommand(cmdText);
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
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);
                }
            }
            return command;
        }

        private static KdbndpCommand BuildIntCommand(KdbndpConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            KdbndpCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new KdbndpParameter("ReturnValue",
                KdbndpDbType.Integer, 4, null,
                ParameterDirection.ReturnValue, false, 0, 0, DataRowVersion.Default, null));
            return command;
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
                    if (p.PropertyType == typeof(Int32) ||
                        p.PropertyType == typeof(Int64) ||
                        p.PropertyType == typeof(String) ||
                        p.PropertyType == typeof(Boolean) ||
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
                    if (p.PropertyType == typeof(Int32) ||
                        p.PropertyType == typeof(Int64) ||
                        p.PropertyType == typeof(String) ||
                        p.PropertyType == typeof(Boolean) ||
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

        public string GetValue(string SQLString, params KdbndpParameter[] cmdParms)
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

        public DataTable Execute<T>(string value, T model)
        {
            KdbndpCommand cmd = GetStoredProcCommand(value);
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
        private void AddInParameter(KdbndpCommand command, string name, object value)
        {
            value = value == null ? DBNull.Value : value;
            KdbndpParameter sqlParameter = new KdbndpParameter(name, value);
            sqlParameter.Direction = ParameterDirection.Input;
            command.Parameters.Add(sqlParameter);
        }
    }
}
