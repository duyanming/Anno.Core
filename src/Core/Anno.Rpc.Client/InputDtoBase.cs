using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Client
{
    /// <summary>
    /// 请求参数标记
    /// </summary>
    public class InputDtoBase : IInputDto
    {
        /// <summary>
        /// 业务模块名称（命名空间 去掉尾部的Service标志）
        /// Anno.Plugs.DLock( Anno.Plugs.DLockService)
        /// </summary>
        public string channel { get; set; }
        /// <summary>
        /// 功能模块名称（类 去掉尾部的Module标志）
        /// DLock(DLockModule)
        /// </summary>
        public string router { get; set; }
        /// <summary>
        /// 具体功能（方法名）
        /// EnterLock
        /// </summary>
        public string method { get; set; }
    }
}
