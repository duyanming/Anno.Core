using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Client
{
    /// <summary>
    /// 连接池 调优配置
    /// </summary>
    public class ConnectionPoolConfiguration
    {
        /// <summary>
        /// 最大活动数量 默认500
        /// </summary>
        public int MaxActive { get; set; } = 500;
        /// <summary>
        ///  最小空闲数量(默认个数为 CPU 数量 Environment.ProcessorCount)
        /// </summary>
        public int MinIdle { get; set; } = Environment.ProcessorCount;
        /// <summary>
        /// 最大空闲数量 默认50
        /// </summary>
        public int MaxIdle { get; set; } = 50;
    }
}
