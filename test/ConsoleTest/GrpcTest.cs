using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Anno.Const;

namespace ConsoleTest
{
    using Anno.Rpc.Client;
    public class GrpcTest
    {
        public void Handle()
        {
            Init();
        To:
            Console.Write("请输入线程数：");
            long.TryParse(Console.ReadLine(), out long th);
            Console.Write("请输入每个线程数调用次数：");
            long.TryParse(Console.ReadLine(), out long num);
            List<Task> ts = new List<Task>();

            Stopwatch sw = Stopwatch.StartNew();
            int total = 0;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < th; i++)
            {

                var t = Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < num; j++)
                    {
                        Dictionary<string, string> input = new Dictionary<string, string>();

                        input.Add("channel", "Anno.Plugs.HelloWorld");
                        input.Add("router", "HelloWorldViper");
                        input.Add("method", "Test0");
                        var t1 = Task.Run(() =>
                        {
                            var x = Connector.BrokerDns(input);
                            //Console.WriteLine(x);
                            if (x.IndexOf("true") <= 0)
                            {
                                Console.WriteLine(x);
                            }
                        });
                        tasks.Add(t1);

                        //if (x.IndexOf("true") <= 0)
                        //{
                        //    Console.WriteLine(x);
                        //}
                        //Interlocked.Increment(ref total);
                        //Console.WriteLine(x);
                    }
                }, TaskCreationOptions.LongRunning);
                ts.Add(t);
            }
            Console.WriteLine($"-----------------------1----------------------");
            Task.WaitAll(ts.ToArray());
            Console.WriteLine($"-----------------------2----------------------");
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"-----------------------3----------------------");
            long ElapsedMilliseconds = sw.ElapsedMilliseconds;
            if (ElapsedMilliseconds == 0)
            {
                ElapsedMilliseconds = 1;
            }
            Console.WriteLine($"运行时间：{sw.ElapsedMilliseconds}/ms,TPS:{(num * th) * 1000 / ElapsedMilliseconds}");
            sw.Stop();
            goto To;
        }

        public void Handle1()
        {
            List<Task> ts = new List<Task>();
            int[] x = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int i = 0; i < 10; i++)
            {
                var i1 = i;
                var t = Task.Factory.StartNew(() =>
                {
                    Task.Delay(1000).Wait();
                    Console.WriteLine(x[i1]);
                });
                ts.Add(t);
            }

            Task.WaitAll(ts.ToArray());
        }

        void Init()
        {
            //DefaultConfigManager.SetDefaultConnectionPool(new ConnectionPoolConfiguration() {
            //    MaxActive=1000,
            //    MaxIdle=100,
            //    MinIdle=50
            //});
            DefaultConfigManager.SetDefaultConfiguration("RpcTest", "127.0.0.1", 6660, false);
        }
    }
}
