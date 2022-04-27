using System;

namespace Anno.Rpc.Client
{
    using Const;
    using Anno.CronNET;

    /// <summary>
    /// RPC 客户端配置
    /// </summary>
    public static class DefaultConfigManager
    {
        /// <summary>
        /// 任务管理器
        /// </summary>
        private static readonly CronDaemon CronDaemon = new CronDaemon();
        private static readonly object _locker = new object();

        public static ConnectionPoolConfiguration DefaultConnectionPoolConfiguration = new ConnectionPoolConfiguration()
        {
            MaxActive = 500,
            MaxIdle = 50,
            MinIdle = 0

        };

        /// <summary>
        /// 设置RPC 基础配置
        /// </summary>
        /// <param name="appName">客户端名称</param>
        /// <param name="centerAddress">注册中心地址</param>
        /// <param name="port">注册中心端口</param>
        /// <param name="traceOnOff">调用链追踪默认打开</param>
        public static void SetDefaultConfiguration(string appName, string centerAddress, int port = 6660, bool traceOnOff = true)
        {
            SettingService.AppName = appName;
            SettingService.Local.IpAddress = centerAddress;
            SettingService.Local.Port = port;
            SettingService.TraceOnOff = traceOnOff;
            //初始化 拉取配置中心 路由信息
            Connector.UpdateCache(string.Empty);
            if (CronDaemon.Status == DaemonStatus.Stop)
            {
                lock (_locker)
                {
                    if (CronDaemon.Status == DaemonStatus.Stop)
                    {
                        CronDaemon.AddJob("*/30 * * * * ? *", GrpcFactory.CleanPoolLink);
                        if (Const.SettingService.TraceOnOff)
                        {
                            CronDaemon.AddJob("*/10 * * * * ? *", TracePool.TryDequeue);
                        }
                        CronDaemon.AddJob("*/5 * * * * ? *", () => { Connector.UpdateCache("cron:"); });
                        CronDaemon.Start();
                    }
                }
            }
        }
        /// <summary>
        /// 设置连接池信息
        /// </summary>
        /// <param name="maxActive"></param>
        /// <param name="minIdle"></param>
        /// <param name="maxIdle"></param>
        public static void SetDefaultConnectionPool(int maxActive, int minIdle, int maxIdle)
        {
            DefaultConnectionPoolConfiguration.MaxActive = maxActive;
            DefaultConnectionPoolConfiguration.MinIdle = minIdle;
            DefaultConnectionPoolConfiguration.MaxIdle = maxIdle;
        }

    }
}
