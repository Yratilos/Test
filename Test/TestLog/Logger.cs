using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.TestLog
{
    public class Logger
    {
        private static readonly string logFilePath = "log.txt";

        public static void Log(string message)
        {
            using (StreamWriter writer = File.AppendText(logFilePath))
            {
                writer.WriteLine($"{DateTime.Now} - {message}");
            }
        }
    }
}
