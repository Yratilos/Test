using SqlSugar;

namespace Test.TestDataBase
{
    /// <summary>
    /// SqlSugar类
    /// </summary>
    public class SqlSugarCore
    {
        /// <summary>
        /// 创建实体类
        /// </summary>
        /// <param name="conn">数据库连接字符串</param>
        /// <param name="url">绝对路径</param>
        /// <param name="name">命名空间</param>
        /// <param name="model">表名(默认全部表)</param>
        public static void CreateModel(string conn, string model = "",DbType type= DbType.SqlServer, string url= "E:\\TestProject\\WebApi\\Models\\",  string name= "WebApi.Models")
        {
            // 创建数据库对象
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = conn,
                DbType = type,
                IsAutoCloseConnection = true
            },
            db =>
            {
                //5.1.3.24统一了语法和SqlSugarScope一样，老版本AOP可以写外面

                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Console.WriteLine(sql);//输出sql,查看执行sql 性能无影响

                    //获取原生SQL推荐 5.1.4.63  性能OK
                    //UtilMethods.GetNativeSql(sql,pars)

                    //获取无参数化SQL 对性能有影响，特别大的SQL参数多的，调试使用
                    //UtilMethods.GetSqlString(DbType.SqlServer,sql,pars)
                };

                //注意多租户 有几个设置几个
                //db.GetConnection(i).Aop
            });
            if (model == "")
            {
                db.DbFirst.IsCreateDefaultValue().CreateClassFile(url);
            }
            else
            {
                db.DbFirst.Where(model).IsCreateDefaultValue().CreateClassFile(url, name);
            }
        }
    }
}