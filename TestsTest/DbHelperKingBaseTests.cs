using Kdbndp;
using System.Collections;
using System.Configuration;
using System.Data;
using Test.KingBase;

namespace SimCloudDAL.Tests
{
    [TestClass()]
    public class DbHelperKingBaseTests
    {
        DbHelperKingBase db;
        string connectionString = "KingBaseSimCloud";
        private string GetConnection(string connectionStr)
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = "App.config" };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            var connectionString = config.ConnectionStrings.ConnectionStrings[connectionStr].ToString();
            return connectionString;
        }

        [TestInitialize]
        public void Initialize()
        {
            db = new DbHelperKingBase(connectionString);
        }

        [TestMethod()]
        public void DbHelperKingBaseTest()
        {
            Assert.IsNotNull(db);
        }


        [TestMethod()]
        public void GetConnectionTest()
        {
            var connectionString = GetConnection(this.connectionString);
            Assert.IsNotNull(connectionString);
        }

        [TestMethod()]
        public void CreateConnectionTest()
        {
            var conn = db.CreateConnection();
            Assert.IsNotNull(conn);
        }

        [DataRow("dbo.SP_GetGroupPermissions", "0126a5ec-13aa-45bc-8044-91d154bb81e9")]
        [TestMethod()]
        public void GetStoredProcCommandTest(string name,string groupName)
        {
            var command = db.GetStoredProcCommand(name);
            db.AddInParameter(command, "groupname", DbType.String, groupName);
            DataSet ds = db.ExecuteDataSet(command);
            Assert.IsTrue(ds.Tables[0].Rows.Count>0);
        }

        [DataRow("SELECT UserName, Domain, DomainUserName, LastName, FirstName, Email, Phone, LastLoginTime, LastOnlineDate, Password, LastOnlineDate_old, LOGINNAME, MDM_MobilPhoneNumber, MDM_NWYX, MDM_WWYX, MDM_OrganizeInfo, MDM_Positon, AD_JobNumber, AD_Name, IsEnable, LoginFailedTime, LoginFailed FROM dbo.MemberInfo")]
        [TestMethod()]
        public void GetSQLStringCommandTest(string sql)
        {
            var command = db.GetSQLStringCommand(sql);
            Assert.IsNotNull(command);
        }

        [DataRow("SELECT * FROM dbo.MemberInfo where UserName=:name;", "name", DbType.String, "aaaothbu")]
        [TestMethod()]
        public void AddInParameterTest(string sql, string name, DbType type, string value)
        {
            var command = db.GetSQLStringCommand(sql);
            db.AddInParameter((KdbndpCommand)command, name, type, value);
        }

        [DataRow("SELECT * FROM dbo.MemberInfo where UserName=:name;", "name", DbType.String, "aaaothbu")]
        [TestMethod()]
        public void AddParameterTest(string sql, string name, DbType type, string value)
        {
            var command = db.GetSQLStringCommand(sql);
            db.AddParameter((KdbndpCommand)command, name, type, ParameterDirection.Output, value);
        }

        [DataRow("SELECT * FROM dbo.MemberInfo where UserName=:name;", "name", DbType.String, 50)]
        [TestMethod()]
        public void AddOutParameterTest(string sql, string name, DbType type, int size)
        {
            var command = db.GetSQLStringCommand(sql);
            db.AddOutParameter((KdbndpCommand)command, name, type, size);
        }

        [DataRow("SELECT * FROM dbo.MemberInfo where UserName=:name;", "name", DbType.String, "361266197510011345")]
        [TestMethod()]
        public void GetParameterValueTest(string sql, string name, DbType type, string value)
        {
            var command = db.GetSQLStringCommand(sql);
            db.AddInParameter((KdbndpCommand)command, name, type, value);
            var cmd = db.GetParameterValue((KdbndpCommand)command, name);
            Assert.IsNotNull(cmd);
        }

        [DataRow("SELECT * FROM dbo.MemberInfo where UserName=:name;", "name", DbType.String, "361266197510011345")]
        [TestMethod()]
        public void ExecuteDataSetTest(string sql, string name, DbType type, string value)
        {
            var command = db.GetSQLStringCommand(sql);
            db.AddInParameter(command, name, type, value);
            var ds = db.ExecuteDataSet(command);
            Assert.AreEqual(ds.Tables[0].Rows[0]["UserName"], value);
        }

        [DataRow("dbo.SP_GetGroupPermissions", "groupname", DbType.String, "6885628EEE3A4111B54B827951C07AF1")]
        [TestMethod()]
        public void ExecuteDataSetTestAlter(string sql, string name, DbType type, string value)
        {
            var cmd = db.GetStoredProcCommand(sql);
            db.AddInParameter(cmd, name, type, value);
            DataSet ds = db.ExecuteDataSet(cmd);
            Assert.IsTrue(ds.Tables[0].Rows.Count>0);
        }

        [DataRow("SELECT * FROM dbo.MemberInfo where UserName=:name;", "name", DbType.String, "361266197510011345", "SELECT * FROM dbo.MemberInfo where DomainUserName=:domain;", "domain", "VANCLOUDCORP\\361266197510011345")]
        [TestMethod()]
        public void ExecuteDataSetTest1(string sql, string name, DbType type, string value, string sql1, string name1, string value1)
        {
            var connectionString = GetConnection(this.connectionString);

            var command = db.GetSQLStringCommand(sql);
            db.AddInParameter((KdbndpCommand)command, name, type, value);
            var command1 = db.GetSQLStringCommand(sql1);
            db.AddInParameter((KdbndpCommand)command1, name1, type, value1);

            KdbndpConnection connection = new KdbndpConnection(connectionString);
            connection.Open();
            KdbndpTransaction transaction = connection.BeginTransaction();
            try
            {
                var ds = db.ExecuteDataSet((KdbndpCommand)command, transaction);
                Assert.AreEqual(ds.Tables[0].Rows[0]["UserName"], value);
                var ds1 = db.ExecuteDataSet((KdbndpCommand)command1, transaction);
                Assert.AreEqual(ds1.Tables[0].Rows[0]["DomainUserName"], value1);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
            finally
            {
                connection.Close();
            }
        }

        [DataRow("DELETE FROM dbo.MemberInfo  WHERE UserName =:name;", "SELECT * FROM dbo.MemberInfo where UserName=:name;", "name", DbType.String, "aafidt")]
        [TestMethod()]
        public void ExecuteNonQueryTest(string sql, string sql1, string name, DbType type, string value)
        {
            var command = db.GetSQLStringCommand(sql);
            db.AddInParameter((KdbndpCommand)command, name, type, value);
            db.AddInParameter((KdbndpCommand)command, name, type, value);
            db.ExecuteNonQuery((KdbndpCommand)command);

            var command1 = db.GetSQLStringCommand(sql1);
            db.AddInParameter((KdbndpCommand)command1, name, type, value);
            var ds = db.ExecuteDataSet((KdbndpCommand)command1);
            Assert.AreEqual(ds.Tables[0].Rows.Count, 0);
        }

        [DataRow("call dbo.DeleteMemberInfo(:name);", "name", DbType.String, "aakhjl", "DELETE FROM dbo.MemberInfo  WHERE UserName =:name;", "aalqunz", "SELECT * FROM dbo.MemberInfo where UserName=:name;")]
        [TestMethod()]
        public void ExecuteNonQueryTest1(string sql, string name, DbType type, string value, string sql1, string value1, string sql2)
        {
            var connectionString = GetConnection(this.connectionString);

            var command = db.GetSQLStringCommand(sql);
            db.AddInParameter((KdbndpCommand)command, name, type, value);
            var command1 = db.GetSQLStringCommand(sql1);
            db.AddInParameter((KdbndpCommand)command1, name, type, value1);

            KdbndpConnection connection = new KdbndpConnection(connectionString);
            connection.Open();
            KdbndpTransaction transaction = connection.BeginTransaction();
            try
            {
                var ds = db.ExecuteNonQuery((KdbndpCommand)command, transaction);
                var ds1 = db.ExecuteNonQuery((KdbndpCommand)command1, transaction);
                transaction.Commit();

                var command2 = db.GetSQLStringCommand(sql2);
                db.AddInParameter((KdbndpCommand)command2, name, type, value);
                var ds2 = db.ExecuteDataSet((KdbndpCommand)command2);
                Assert.AreEqual(ds2.Tables[0].Rows.Count, 0);

                var command3 = db.GetSQLStringCommand(sql2);
                db.AddInParameter((KdbndpCommand)command3, name, type, value1);
                var ds3 = db.ExecuteDataSet((KdbndpCommand)command3);
                Assert.AreEqual(ds3.Tables[0].Rows.Count, 0);
            }
            catch
            {
                transaction.Rollback();
            }
            finally
            {
                connection.Close();
            }
        }

        [DataRow("SELECT UserName FROM dbo.MemberInfo where UserName=:name;", "name", "361266197510011345")]
        [TestMethod()]
        public void ExecuteScalarTest(string sql, string key, string value)
        {
            KdbndpParameter[] parameters = new KdbndpParameter[]
            {
                new KdbndpParameter(key, value)
            };
            var data = db.ExecuteScalar(sql, parameters);
            Assert.AreEqual(data, value);
        }

        [DataRow("SELECT UserName FROM dbo.MemberInfo where UserName='361266197510011345';")]
        [TestMethod()]
        public void ExecuteScalarTest1(string sql)
        {
            var data = db.ExecuteScalar(sql);
            Assert.IsNotNull(data);
        }

        [DataRow("call dbo.DeleteMemberInfo(:name);", "name", DbType.String, "aakhjl", "SELECT UserName FROM dbo.MemberInfo where UserName=:name;")]
        [TestMethod()]
        public void ExecuteScalarTest2(string sql, string name, DbType type, string value, string sql1)
        {
            var connectionString = GetConnection(this.connectionString);

            var command = db.GetSQLStringCommand(sql);
            db.AddInParameter((KdbndpCommand)command, name, type, value);
            var command1 = db.GetSQLStringCommand(sql1);
            db.AddInParameter((KdbndpCommand)command1, name, type, value);

            KdbndpConnection connection = new KdbndpConnection(connectionString);
            connection.Open();
            KdbndpTransaction transaction = connection.BeginTransaction();
            try
            {
                var ds = db.ExecuteNonQuery((KdbndpCommand)command, transaction);
                var data = db.ExecuteScalar((KdbndpCommand)command1, transaction);
                transaction.Commit();

                Assert.IsNull(data);
            }
            catch
            {
                transaction.Rollback();
                connection.Close();
            }

        }

        [DataRow("SELECT UserName, Domain, DomainUserName, LastName, FirstName, Email, Phone, CAST (LastLoginTime as TEXT), LastOnlineDate, Password, LastOnlineDate_old, LOGINNAME, MDM_MobilPhoneNumber, MDM_NWYX, MDM_WWYX, MDM_OrganizeInfo, MDM_Positon, AD_JobNumber, AD_Name, IsEnable, LoginFailedTime, LoginFailed FROM dbo.MemberInfo;")]
        [TestMethod()]
        public void QueryTest(string sql)
        {
            var ds = db.Query(sql);
            Assert.IsNotNull(ds);
        }

        //[DataRow("SELECT UserName,DomainUserName FROM dbo.GetMemberInfo(:name);", "name", "361266197510011345")]
        [DataRow("SELECT UserName, Domain, DomainUserName, LastName, FirstName, Email, Phone, LastLoginTime, LastOnlineDate, Password, LastOnlineDate_old, LOGINNAME, MDM_MobilPhoneNumber, MDM_NWYX, MDM_WWYX, MDM_OrganizeInfo, MDM_Positon, AD_JobNumber, AD_Name, IsEnable, LoginFailedTime, LoginFailed FROM dbo.MemberInfo where UserName=:name;", "name", "361266197510011345")]
        [TestMethod()]
        public void QueryTest1(string sql, string key, string value)
        {
            IDataParameter[] parameters = new KdbndpParameter[]
            {
                new KdbndpParameter(key, value)
            };
            var ds = db.Query(sql, parameters);
            Assert.AreEqual(ds.Tables[0].Rows.Count, 1);
        }

        [DataRow("SELECT UserName FROM dbo.MemberInfo where UserName=:name;", "name", "361266197510011345")]
        [TestMethod()]
        public void ExecuteScalarTest3(string sql, string key, string value)
        {
            IDataParameter[] parameters = new KdbndpParameter[]
            {
                new KdbndpParameter(key, value)
            };
            var data = db.ExecuteScalar(sql, parameters);
            Assert.AreEqual(data, value);
        }

        [DataRow("SELECT * FROM dbo.MemberInfo where UserName=:name;", "name", "aamkkpj", DbType.String)]
        [TestMethod()]
        public void ExecuteNonQueryTest2(string sql, string key, string value, DbType type)
        {
            IDataParameter[] parameters = new KdbndpParameter[]
            {
                new KdbndpParameter(key, value)
            };
            var res=db.ExecuteNonQuery(sql, parameters);
            Assert.AreEqual(res,-1);
        }

        [DataRow("dbo.SP_GetGroupPermissions", "groupname", "0126a5ec-13aa-45bc-8044-91d154bb81e9")]
        [TestMethod()]
        public void RunProcedureTest(string fn, string key, string value)
        {
            IDataParameter[] parameters = new KdbndpParameter[]
            {
                new KdbndpParameter(key, value),
            };
            var ds = db.RunProcedure(fn, parameters);
            Assert.IsTrue(ds.Tables[0].Rows.Count>0);
        }

        [DataRow("SELECT UserName FROM dbo.MemberInfo where UserName=:name;", "name", "aamkkpj")]
        [TestMethod()]
        public void RunProcedureTest1(string fn, string key, string value)
        {
            IDataParameter[] parameters = new KdbndpParameter[]
            {
                new KdbndpParameter(key, value)
            };
            var data = db.RunProcedure(fn, parameters, out int res);

        }

        [DataRow("SELECT UserName FROM dbo.MemberInfo")]
        [TestMethod()]
        public void ExecuteSqlTest(string sql)
        {
            var data = db.ExecuteSql(sql);
            Assert.IsNotNull(data);
        }

        [DataRow("SELECT Count(UserName) FROM dbo.MemberInfo where UserName='361266197510011345';")]
        [TestMethod()]
        public void ExistsTest(string sql)
        {
            var data = db.Exists(sql);
            Assert.IsTrue(data >= -1);
        }

        [DataRow("DELETE FROM dbo.MemberInfo  WHERE UserName ='aazjmx';", "DELETE FROM dbo.MemberInfo  WHERE UserName ='aazql';")]
        [TestMethod()]
        public void ExecuteSqlTranTest(string sql, string sql1)
        {
            var lst = new List<string>() { sql, sql1 };
            var data = db.ExecuteSqlTran(lst);
            Assert.IsTrue(data >= 0);
        }

        [DataRow("name", "aazw", "DELETE FROM dbo.MemberInfo  WHERE UserName =:name;", "ab")]
        [TestMethod()]
        public void ExecuteSqlTranTest1(string key, string value, string sql, string value1)
        {
            var ht = new Hashtable()
            {
                { new KdbndpParameter[]{ new KdbndpParameter(key, value) },sql },
                { new KdbndpParameter[]{ new KdbndpParameter(key, value1) },sql }
            };
            var data = db.ExecuteSqlTran(ht);
            Assert.IsTrue(data >= 0);
        }


        [DataRow("小明", "小明", "小红", "小红")]
        [TestMethod()]
        public void InsertTest(string name, string value, string name1, string value1)
        {
            List<object> lst = new List<object>()
            {
                new test()
                {
                    name=name,
                    val=value,
                },
                new test()
                {
                    name=name1,
                    val=value1,
                }
            };
            var data = db.Insert(lst);
            Assert.IsTrue(data > 0);
        }

        [DataRow("小明", "小明", "小红", "小红")]
        [TestMethod()]
        public void DeleteTest(string name, string value, string name1, string value1)
        {
            List<object> lst = new List<object>()
            {
                new test()
                {
                    name=name,
                    val=value,
                },
                new test()
                {
                    name=name1,
                    val=value1,
                }
            };
            var data = db.Delete(lst);
            Assert.IsTrue(data >= 0);
        }

        [DataRow("SELECT UserName FROM dbo.MemberInfo where UserName=:name;", "name", "361266197510011345")]
        [TestMethod()]
        public void GetValueTest(string sql, string key, string value)
        {
            KdbndpParameter[] parameters = new KdbndpParameter[]
            {
                new KdbndpParameter(key, value)
            };
            var data = db.GetValue(sql, parameters);
            Assert.AreEqual(data, value);
        }

        [DataRow("dbo.SP_GetGroupPermissions", "0126a5ec-13aa-45bc-8044-91d154bb81e9")]
        [TestMethod()]
        public void ExecuteTest(string fn, string name)
        {
            var tst = new
            {
                groupname = name
            };
            var dt = db.Execute(fn, tst);
            Assert.IsTrue(dt.Rows.Count >= 0);
        }
    }

    internal class test
    {
        public test()
        {
        }

        public string name { get; set; }
        public string val { get; set; }
    }
}