using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData.SysInfo
{
    /// <summary>
    /// 记录程序运行时长
    /// </summary>
    public static class RunTimeWatch
    {
        static RunTimeWatch()
        {
            mWatch = new System.Diagnostics.Stopwatch();
            StartTime =DateTime.Now;
            mWatch.Start();
        }

        private static readonly System.Diagnostics.Stopwatch mWatch;
        /// <summary>
        /// 启动时间
        /// </summary>
        public static DateTime StartTime { get; private set; }

        /// <summary>
        /// 获取程序运行时长
        /// </summary>
        /// <returns></returns>

        public static long GetRunTimeMilliseconds()
        {
            return mWatch.ElapsedMilliseconds;
        }
    }
}
