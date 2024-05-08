namespace Test.TestAttribute
{
    /// <summary>
    /// 特性
    /// </summary>
    public class MySpecialAttribute : Attribute
    {
        /// <summary>
        /// 构造方法初始化
        /// </summary>
        /// <param name="DisplayName">显示名称</param>
        /// <param name="DisplayVersion">显示宽度</param>
        public MySpecialAttribute(string DisplayName, double DisplayVersion)
        {
            this.DisplayName = DisplayName;
            this.DisplayVersion = DisplayVersion;
        }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 显示宽度
        /// </summary>
        public double DisplayVersion { get; set; }
    }
}
