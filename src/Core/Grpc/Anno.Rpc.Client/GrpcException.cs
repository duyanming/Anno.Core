using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Client
{
    public class GrpcException : Exception
    {
        public enum ExceptionType
        {
            //超时异常
            Timeout,
            //服务端捕获异常
            ServerCaptured,
            //服务端未知异常
            ServerUnkown,
            //请求数据异常
            InvalidRequestPara
        }

        public ExceptionType Type { get; set; }

        public GrpcException(ExceptionType type)
        {
            this.Type = type;
        }

        public GrpcException(string message)
            : base(message)
        {
        }

        public GrpcException(ExceptionType type, string message)
            : base(message)
        {
            this.Type = type;
        }
    }
}
