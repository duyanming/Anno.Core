using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Client
{
    public enum ExceptionType
    {
        /// <summary>
        /// 超时异常
        /// </summary>
        Timeout,
        /// <summary>
        /// 服务端捕获异常
        /// </summary>
        ServerCaptured,
        /// <summary>
        /// 服务端未知异常
        /// </summary>
        ServerUnkown,
        /// <summary>
        /// 请求数据异常
        /// </summary>
        InvalidRequestPara,
        /// <summary>
        /// 初始化连接池异常
        /// </summary>
        InitTransportPool,
        /// <summary>
        /// 默认类型
        /// </summary>
        Default,
        /// <summary>
        /// 没有发现服务
        /// </summary>
        NotFoundService
    }
    public class ThriftException : Exception
    {
        public ExceptionType Type { get; set; }

        public ThriftException(ExceptionType type)
        {
            this.Type = type;
        }

        public ThriftException(string message)
            : base(message)
        {
            this.Type = ExceptionType.Default;
        }

        public ThriftException(ExceptionType type, string message)
            : base(message)
        {
            this.Type = type;
        }
    }
}
