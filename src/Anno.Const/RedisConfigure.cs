using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Const
{
    /// <summary>
    /// Redis 配置
    /// </summary>
    public class RedisConfigure
    {
        private static readonly object _locker = new Object();
        private static RedisConfigure redisConfigure = null;
        /// <summary>
        /// 连接字符串 默认：127.0.0.1:6379,abortConnect=false,allowAdmin =true,keepAlive=180
        /// </summary>
        public string Conn { get; set; } = "127.0.0.1:6379,abortConnect=false,allowAdmin =true,keepAlive=180";
        /// <summary>
        /// Key前缀 默认：Anno:
        /// </summary>
        public string Prefix { get; set; } = "Anno:";

        /// <summary>
        /// 有效期 默认：20分钟
        /// </summary>
        public TimeSpan ExpiryDate { get; set; } = TimeSpan.FromMinutes(20);
        /// <summary>
        /// 开关 默认 关 false
        /// </summary>
        public Boolean Switch { get; set; } = false;
        private RedisConfigure() { }

        public static RedisConfigure Default()
        {
            if (redisConfigure == null)
            {
                lock (_locker)
                {
                    if (redisConfigure == null)
                    {
                        redisConfigure = new RedisConfigure();
                    }
                }
            }
            return redisConfigure;
        }
        /// <summary>
        /// Redis 配置
        /// </summary>
        /// <param name="Conn">数据库连接字符串</param>
        /// <param name="Prefix">前缀</param>
        /// <param name="ExpiryDate">有效期</param>
        /// <param name="Switch">开关</param>
        public void SetDefault(string Conn, string Prefix, TimeSpan ExpiryDate, Boolean Switch)
        {
            this.Conn = Conn;
            this.Prefix = Prefix;
            this.ExpiryDate = ExpiryDate;
            this.Switch = Switch;
        }
    }
}
