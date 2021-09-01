/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/11/6 13:09:28 
Functional description： LogTest
******************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    using Anno.Log;
    public class LogTest
    {
        public void Handle() {
        To:
            Console.Write("请输入调用次数：");
            long.TryParse(Console.ReadLine(), out long num);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("你好1");
            sb.AppendLine("你好11");
            sb.AppendLine("你好111");
            sb.AppendLine("你好1111");
            sb.AppendLine("你好11你好111你好111你好111你好111你好111你好111你好111你好111你好111你好111你好111你好111你好111你好111你好111你好1111");
            Stopwatch sw = Stopwatch.StartNew();
            Parallel.For(0, num, i =>
            {
                Log.Debug($"Debug{i}-{sb}");
                Log.Info($"Info{i}-{sb}");
                Log.Error($"Error{i}-{sb}");
                Log.Trace($"Trace{i}-{sb}");
                Log.Warn($"Warn{i}-{sb}");
                Log.Fatal($"Fatal{i}-{sb}");
            });
            long ElapsedMilliseconds = sw.ElapsedMilliseconds;
            if (ElapsedMilliseconds == 0)
            {
                ElapsedMilliseconds = 1;
            }
            Console.WriteLine($"运行时间：{sw.ElapsedMilliseconds}/ms,TPS:{(num) * 1000 / ElapsedMilliseconds}");
            sw.Stop();
            goto To;
            Log.Debug("debug");

            Log.DebugConsole("debug");
            Log.Info("debug",typeof(LogTest));
        }
    }
}
