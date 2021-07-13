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

            Stopwatch sw = Stopwatch.StartNew();
            Parallel.For(0, num, i =>
            {
                Log.Debug($"Debug{i}");
                Log.Info($"Info{i}");
                Log.Error($"Error{i}");
                Log.Trace($"Trace{i}");
                Log.Warn($"Warn{i}");
                Log.Fatal($"Fatal{i}");
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
