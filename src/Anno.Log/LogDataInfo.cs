/****************************************************** 
Writer: Du YanMing-admin
Mail:dym880@163.com
Create Date: 7/13/2021 1:24:17 PM
Functional description： LogDataInfo
******************************************************/
using System;
using System.Collections.Generic;
using System.Text;


namespace Anno.Log
{
    /// <summary>
    /// 日志信息
    /// </summary>
    internal class LogDataInfo
    {
        public object logStr { get; set; }
        public Type type { get; set; }
        public int threadId { get; set; }

        public LogType logType { get; set; }
    }
}
