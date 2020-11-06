using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleTest
{
    using Anno.Const;
    using Anno.EngineData;
    public class DLockTest
    {
        public void Handle()
        {
            Init();
        To:
            List<Task> ts = new List<Task>();
            Console.WriteLine("请输入线程数:");
            int.TryParse(Console.ReadLine(), out int n);
            for (int i = 0; i < n; i++)
            {
                var task = Task.Factory.StartNew(() => { DLTest1("Anno"); });
                ts.Add(task);
                var taskXX = Task.Factory.StartNew(() => { DLTest1("Viper"); });
                ts.Add(taskXX);

                var taskJJ = Task.Factory.StartNew(() => { DLTest1("Key001"); });
                ts.Add(taskJJ);
            }

            Task.WaitAll(ts.ToArray());
            goto To;
        }

        private void DLTest1(string lk = "duyanming")
        {
            try
            {
                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:ffff}  {System.Threading.Thread.CurrentThread.ManagedThreadId} DLTest1拉取锁({lk})");
                using (DLock dLock = new DLock(lk, 10000))
                {
                    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:ffff}  {System.Threading.Thread.CurrentThread.ManagedThreadId} DLTest1进入锁({lk})");
                    System.Threading.Thread.Sleep(50);
                }

                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:ffff}  {System.Threading.Thread.CurrentThread.ManagedThreadId} DLTest1离开锁({lk})");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        void Init()
        {
            SettingService.AppName = "DLockTest";
            SettingService.Local.IpAddress = "192.168.1.2";
            SettingService.Local.Port = 6660;
        }
    }
}
