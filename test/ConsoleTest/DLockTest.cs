using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleTest
{
    using Anno.Const;
    using Anno.EngineData;
    using Anno.Loader;
    using Anno.Rpc.Client;
    using Anno.Rpc.Server;
    using Autofac;
    using System.Collections.Concurrent;

    public class DLockTest
    {
        ConcurrentDictionary<string, long> counter;
        public void Handle()
        {
            Init();
        To:
            counter = new ConcurrentDictionary<string, long>();
            List<Task> ts = new List<Task>();
            Console.WriteLine("请输入线程数:");
            int.TryParse(Console.ReadLine(), out int n);
            for (int i = 0; i < n; i++)
            {
                var task = new Task(() => { DLTest1("Anno"); });
                ts.Add(task);
                var taskXX = new Task(() => { DLTest1("Viper"); });
                ts.Add(taskXX);

                var taskJJ = new Task(() => { DLTest1("Key001"); });
                ts.Add(taskJJ);
            }
            Parallel.ForEach(ts, t => { t.Start(); });
            Task.WaitAll(ts.ToArray());
            foreach (var item in counter)
            {
                Console.WriteLine($"{item.Key}:{item.Value},Status:{item.Value==n}");
            }
            goto To;
        }

        private void DLTest1(string lk = "duyanming")
        {
            try
            {
                //Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:ffff}  {System.Threading.Thread.CurrentThread.ManagedThreadId} DLTest1拉取锁({lk})");
                using (DLock dLock = new DLock(lk, 1000))
                {   
                    //Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:ffff}  {System.Threading.Thread.CurrentThread.ManagedThreadId} DLTest1进入锁({lk})");
                    if (!counter.ContainsKey(lk)) {
                        counter.TryAdd(lk, 0);
                    }
                    var value=counter[lk];
                    //System.Threading.Thread.Sleep(10);
                    counter[lk]= value + 1;
                }

                //Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:ffff}  {System.Threading.Thread.CurrentThread.ManagedThreadId} DLTest1离开锁({lk})");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        void Init()
        {
            //SettingService.AppName = "DLockTest";
            //SettingService.Local.IpAddress = "127.0.0.1";
            //SettingService.Local.Port = 6660;

            IocLoader.GetAutoFacContainerBuilder().RegisterType(typeof(RpcConnectorImpl)).As(typeof(IRpcConnector)).SingleInstance();
            IocLoader.Build();
            DefaultConfigManager.SetDefaultConnectionPool(100, Environment.ProcessorCount * 2, 50);
            DefaultConfigManager.SetDefaultConfiguration("DLockTest", "127.0.0.1", 6660, false);
        }
    }
}
