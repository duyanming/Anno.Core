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
        /// <param name="Notice">通知</param>
        /// <param name="ChangeNotice">变更通知</param>
        public static void StartUp(string[] args, Action<ServiceInfo, NoticeType> Notice = null, Action<ServiceInfo, ServiceInfo> ChangeNotice = null)
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
            #region 服务上线 下线 变更通知

            tc.ChangeNotice += (ServiceInfo newService, ServiceInfo oldService) =>
            {
                try
                {
                    tc.RefreshServiceMd5();
                    ChangeNotice?.Invoke(newService, oldService);
                }
                finally { }
            };
            tc.OnlineNotice += (ServiceInfo service, NoticeType noticeType) =>
            {
                try
                {
                    tc.RefreshServiceMd5();
                    Notice?.Invoke(service, noticeType);
                }
                finally { }
            };
            Distribute.CheckNotice += (ServiceInfo service, NoticeType noticeType) =>
            {
                try
                {
                    tc.RefreshServiceMd5();
                    Notice?.Invoke(service, noticeType);
                }
                finally { }
            };
            #endregion
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
