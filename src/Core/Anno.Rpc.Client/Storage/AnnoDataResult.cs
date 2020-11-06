using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Storage
{
    public class AnnoDataResult:AnnoDataResult<dynamic>
    {
       
    }
    public class AnnoDataResult<T>
    {
        /// <summary>
        /// 状态
        /// </summary>
        public Boolean Status { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }
    }
}
