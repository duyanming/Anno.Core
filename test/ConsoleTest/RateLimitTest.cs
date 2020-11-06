using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Anno.RateLimit;

namespace ConsoleTest
{
    /// <summary>
    /// 限流测试
    /// </summary>
    public class RateLimitTest
    {
        /// <summary>
        /// 限流测试
        /// </summary>
        public void Handle()
        {
            var service = LimitingFactory.Build(TimeSpan.FromSeconds(1),LimitingType.TokenBucket, 20, 5);
            Console.Write("请输入线程数：");
            long.TryParse(Console.ReadLine(), out long th);
            for (int i = 0; i < th; i++)
            {
                var t = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        var result = service.Request();
                        //如果返回true，说明可以进行业务处理，否则需要继续等待
                        if (result)
                        {
                            Console.WriteLine($"{DateTime.Now}--{Task.CurrentId}---ok");
                            //业务处理......
                        }
                        else
                            Thread.Sleep(100);
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }
    }
}
