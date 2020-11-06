using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anno.Rpc.Center
{
    public static class Bootstrap
    {
        /// <summary>
        /// Daemon工作状态的主方法
        /// </summary>
        /// <param name="args"></param>
        public static void StartUp(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                if (Monitor.State)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} AnnoCenter Service is being stopped·····");
                    Monitor.Stop();
                    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} AnnoCenter The service has stopped!");
                    Console.ResetColor();
                }
            };
            Monitor.Start();
            var tc = ThriftConfig.CreateInstance();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}:服务注册、发现、健康检查、负载均衡中心，端口：{tc.Port}（AnnoCenter）已启动！");
            Console.ResetColor();
            //阻止daemon进程退出
            while (true)
            {
                tc.ServiceInfoList.Distinct().Where(s => s.Checking == false).ToList().ForEach(service =>
                {
                    Task.Run(() => { Distribute.HealthCheck(service); });
                });
                Thread.Sleep(3000);
            }
        }
    }
}
