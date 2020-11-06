using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData
{
    /// <summary>
    /// 分布式锁异常
    /// </summary>
    public class DLockException : Exception
    {
        public DLockException(string message) : base(message)
        {
        }
    }
}
