using System;
using System.Collections.Generic;
namespace Anno.Const
{
    public static class AppSettings
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static String ConnStr { get; set; }

        /// <summary>
        /// 用户重置密码的时候的默认密码
        /// </summary>
        public static String DefaultPwd { get; set; } = "Anno";
        /// <summary>
        /// Ioc插件DLL列表
        /// </summary>
        public static List<string> IocDll { get; set; } = new List<string>();        
    }
}
