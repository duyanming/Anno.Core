using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData
{
    /// <summary>
    /// InvokeEngine 集群服务间调用异常
    /// </summary>
    public class AnnoInvokeEngineException:Exception
    {
        public enum AnnoInvokeEngineExceptionType
        {
            //超时异常
            Timeout,
            //参数异常
            Argument,
            //服务端捕获异常
            ServerCaptured,
            //服务端未知异常
            ServerUnkown,
            //请求数据异常
            InvalidRequestPara
        }
        public AnnoInvokeEngineExceptionType Type { get; set; }
        public AnnoInvokeEngineException(AnnoInvokeEngineExceptionType type)
        {
            this.Type = type;
        }
        public AnnoInvokeEngineException(AnnoInvokeEngineExceptionType type, string message)
            : base(message)
        {
            this.Type = type;
        }
        public AnnoInvokeEngineException(string message) : base(message)
        {

        }
    }
}
