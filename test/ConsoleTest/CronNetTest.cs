using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Anno.CronNET;


namespace ConsoleTest
{
    public class CronNetTest
    {
        private static CronDaemon cron_daemon = new CronDaemon();
        public void Handle()
        {
          var job1=  cron_daemon.AddJob("* * * * * ? *", () => { MiniteTask("666"); });

            //for (int i = 0; i < 20; i++)
            //{
            //    cron_daemon.AddJob("*/3 * * * * ? *", () => { MiniteTask(i.ToString()); });
            //}
            cron_daemon.Start();
            Task.Delay(3000).Wait();
            cron_daemon.RemoveJob(job1.ID);
            Task.Delay(3000).Wait();
            cron_daemon.AddJob("* * * * * ? *", () => { MiniteTask("777"); });
            Task.Delay(3000).Wait();
            cron_daemon.Stop();
            //cron_daemon.Start();
            Console.WriteLine("任务开始时间：{0}", DateTime.Now.ToLongTimeString());
        }

        static void MiniteTask(string arg)
        {
            Console.WriteLine("{0}:定时任务执行{1}---{2}", DateTime.Now.ToLongTimeString(), arg,DateTime.Now.Millisecond);
            System.Threading.Thread.Sleep(400);
        }
    }
}
