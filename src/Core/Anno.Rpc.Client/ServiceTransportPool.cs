using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Transport;

namespace Anno.Rpc.Client
{
    /// <summary>
    /// TransportPool 连接池
    /// </summary>
    internal class ServiceTransportPool
    {
        private int _activedTransportCount = 0;
        public ServiceConfig ServiceConfig { get; set; }

        public ConcurrentQueue<TTransportExt> TransportPool { get; set; }

        public AutoResetEvent ResetEvent { get; set; }
        /// <summary>
        /// 活动链接数量
        /// </summary>
        public int ActivedTransportCount => _activedTransportCount;

        /// <summary>
        /// 原子性增加 活动链接数量
        /// </summary>
        public void InterlockedIncrement()
        {
            Interlocked.Increment(ref _activedTransportCount);
        }
        /// <summary>
        /// 原子性减少 活动链接数量
        /// </summary>
        public void InterlockedDecrement()
        {
            Interlocked.Decrement(ref _activedTransportCount);
        }
        /// <summary>
        /// 初始化连接池
        /// </summary>
        public void InitTransportPool()
        {
            if (ServiceConfig != null && TransportPool != null && TransportPool.Count <= 0)
            {
                Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < ServiceConfig.MinIdle; i++)
                    {
                        try
                        {
                            var transport = new TSocket(ServiceConfig.Ip, ServiceConfig.Port, ServiceConfig.Timeout);
                            var tExt = new TTransportExt()
                            {
                                Transport = transport
                                ,
                                Client = new BrokerService.Client(new Thrift.Protocol.TBinaryProtocol(transport))
                                ,
                                LastDateTime = DateTime.Now
                            };
                            transport.Open();
                            TransportPool.Enqueue(tExt);
                        }
                        catch{}
                    }
                });
            }
            else
            {
                new ThriftException(ExceptionType.InitTransportPool, $"Please initialize ServiceConfig before calling InitTransportPool");
            }
        }
    }
    /// <summary>
    /// 数据传输对象扩展
    /// </summary>
    public class TTransportExt
    {
        /// <summary>
        /// Thrift 传输对象
        /// </summary>
        public TTransport Transport { get; set; }
        /// <summary>
        /// BrokerService.Client 客户端对象
        /// </summary>
        public BrokerService.Client Client { get; set; }

        /// <summary>
        /// 最后使用时间
        /// </summary>
        public DateTime LastDateTime { get; set; } = DateTime.Now;
    }
}
