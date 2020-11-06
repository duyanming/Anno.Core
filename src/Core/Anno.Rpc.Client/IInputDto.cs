using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Client
{
    /// <summary>
    /// 请求参数标记
    /// </summary>
    public interface IInputDto
    {
        /// <summary>
        /// 业务模块名称（命名空间 去掉尾部的Service标志）
        /// Anno.Plugs.DLock( Anno.Plugs.DLockService)
        /// </summary>
        string channel { get; set; }
        /// <summary>
        /// 功能模块名称（类 去掉尾部的Module标志）
        /// DLock(DLockModule)
        /// </summary>
        string router { get; set; }
        /// <summary>
        /// 具体功能（方法名）
        /// EnterLock
        /// </summary>
        string method { get; set; }
    }
}
