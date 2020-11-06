using System;
using System.Linq;
using System.Net;

namespace Anno.Rpc.Client
{
    /// <summary>
    /// 工厂配置文件
    /// </summary>
    internal class ServiceConfig
    {
        private string _id = string.Empty;
        private string _host = string.Empty;
        private int _port;
        /// <summary>
        /// Ip
        /// </summary>
        public string Ip { get; private set; }

        /// <summary>
        /// 服务主机 Ip  或者 Name
        /// </summary>
        public string Host
        {
            get => _host;
            set
            {
                _host = value;
                if (!IPAddress.TryParse(value, out IPAddress ipAddress))
                {
                    var addresses = Dns.GetHostAddresses(value).Where(p => !p.IsIPv6LinkLocal).ToArray();
                    if (addresses.Length > 0)
                    {
                        Ip = addresses[addresses.Length - 1].ToString();
                    }
                }
                else
                {
                    Ip = ipAddress.ToString();
                }
                _id = $"{Ip}:{ _port}";
            }
        }
        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
                _id = $"{Ip }:{ _port}";
            }
        }

        /// <summary>
        /// 和服务建立连接的超时时间
        /// 单位毫秒
        /// </summary>
        public int Timeout { get; set; } = 1000;

        /// <summary>
        /// 最大活动数量 默认500
        /// </summary>
        public int MaxActive { get; set; } = DefaultConfigManager.DefaultConnectionPoolConfiguration.MaxActive;

        /// <summary>
        /// 最大空闲数量
        /// </summary>
        public int MaxIdle { get; set; } = DefaultConfigManager.DefaultConnectionPoolConfiguration.MaxIdle;

        /// <summary>
        /// 最小空闲数量(默认个数为 CPU 数量 Environment.ProcessorCount)
        /// </summary>
        public int MinIdle { get; set; } = DefaultConfigManager.DefaultConnectionPoolConfiguration.MinIdle;
        /// <summary>
        /// 连接池等待连接时间
        /// 单位毫秒
        /// 超时记日志还是通知谁更改连接池配置
        /// </summary>
        public int WaitingTimeout { get; set; } = 1000;

        /// <summary>
        /// IP端口唯一标志（IP+Port）
        /// </summary>
        public string Id => _id;
    }
}
