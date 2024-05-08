using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Xml;

namespace Test.TestAttribute
{
    /// <summary>
    /// 系统类
    /// </summary>
    public class TestSystem
    {
        /// <summary>
        /// 系统L
        /// </summary>
        [MySpecial("Centos", 3.1)]
        public string? Linux { get; set; }
        /// <summary>
        /// 系统W
        /// </summary>
        [MySpecial("Windows11", 11.4)]
        public string? Windows { get; set; }
        /// <summary>
        /// 数字
        /// </summary>
        public int? Port { get; set; }
        /// <summary>
        /// 布尔
        /// </summary>
        public bool? Isolated { get; set; }
        /// <summary>
        /// 枚举
        /// </summary>
        public TestSystemEnum SystemEnum { get; set; }
        /// <summary>
        /// 特性-获取对象特性信息
        /// </summary>
        public static void TestAttribute()
        {
            Type type = typeof(TestSystem);
            foreach (PropertyInfo pi in type.GetProperties())
            {
                var propertyName = pi.Name;
                var displayName = pi.GetCustomAttribute<MySpecialAttribute>()?.DisplayName;
                var displayWidth = pi.GetCustomAttribute<MySpecialAttribute>()?.DisplayVersion;
                Console.WriteLine("属性名称：" + propertyName + "；显示名称：" + displayName + "；显示版本：" + displayWidth);
            }
        }

        /// <summary>
        /// 获取对象信息
        /// </summary>
        public static void TestMapSystem<T>(T obj) where T : class
        {
            foreach (var item in obj.GetType().GetProperties())
            {
                var value = item.GetValue(obj);
                var msg = $"属性名称：{item.Name}；属性类型：{item.PropertyType}；属性值：{value}";
                Console.WriteLine(msg);
            }
        }

        /// <summary>
        /// 获取某类中字段的summary注释
        /// </summary>
        public static void GetSummary<T>()
        {
            var doc = new XmlDocument();
            doc.Load("TestXMLFile.xml");
            var currNode = doc.GetElementsByTagName("member");
            foreach (XmlNode node in currNode)
            {
                var type = typeof(T).FullName is null ? "" : $"P:{typeof(T).FullName}";
                if (node?.Attributes?["name"]?.Value.Contains(type) ?? false)
                {
                    var msg = $"注释内容：{node?.InnerText.Replace("\r\n","").Trim(' ')}；属性路径：{node?.Attributes?["name"]?.Value}；类全称：{typeof(T).FullName}";
                    Console.WriteLine(msg);
                }
            }
        }

        /// <summary>
        /// 枚举-循环所有枚举
        /// </summary>
        public static void TestEnum()
        {
            foreach (var item in Enum.GetNames(typeof(TestSystemEnum)))
            {
                Console.WriteLine(item);
            }
        }


    }
}