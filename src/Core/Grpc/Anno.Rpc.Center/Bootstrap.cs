using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anno.Rpc.Center
{
    using Anno.Log;
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
            var tc = ThriftConfig.CreateInstance();
            OutputLogo(tc);
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                if (Monitor.State)
                {
                    Log.WriteLine("AnnoCenter Service is being stopped·····", ConsoleColor.DarkGreen);
                    Monitor.Stop();
                    Log.WriteLine("AnnoCenter The service has stopped!", ConsoleColor.DarkGreen);
                }
            };
            Monitor.Start();
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
            Log.WriteLine($"服务注册、发现、健康检查、KV存储、API文档、负载均衡中心，端口：{tc.Port}（AnnoCenter）已启动！", ConsoleColor.DarkGreen);
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
        private static void OutputLogo(ThriftConfig tc)
        {
            var logo = "\r\n";
            logo += " -----------------------------------------------------------------------------\r\n";
            logo +=
$@"                                                _                    
     /\                           ___          (_)                   
    /  \    _ __   _ __    ___   ( _ )  __   __ _  _ __    ___  _ __ 
   / /\ \  | '_ \ | '_ \  / _ \  / _ \/\\ \ / /| || '_ \  / _ \| '__|
  / ____ \ | | | || | | || (_) || (_>  < \ V / | || |_) ||  __/| |   
 /_/    \_\|_| |_||_| |_| \___/  \___/\/  \_/  |_|| .__/  \___||_|   
                                                  | |                
                                                  |_|                
                                    [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] grpc center
";
            logo += " -----------------------------------------------------------------------------\r\n";
            logo += $" Center Port      {tc.Port} \r\n";
            logo += $" Author           YanMing.Du \r\n";
            logo += $" Version          [{ typeof(Center.Bootstrap).Assembly.GetName().Version}]\r\n";
            logo += $" Repository       https://github.com/duyanming/anno.core \r\n";
            logo += " -----------------------------------------------------------------------------\r\n";
            Log.WriteLineNoDate(logo);
        }
    }
}
