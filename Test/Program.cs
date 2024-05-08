using Kdbndp;
using System.Configuration;
using System.Data;
using Test.TestDataBase;

namespace Test
{
    internal class Program
    {
        static void Main()
        {
            SqlSugarCore.CreateModel("Server=localhost;Port=54321;UID=system;PWD=abcde12345!;database=test;", "Job",SqlSugar.DbType.Kdbndp);
        }
    }
}