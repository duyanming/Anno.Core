using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Grpc.Core;

namespace Anno.Rpc.Client
{
    /// <summary>
    /// TransportPool 连接池
    /// </summary>
    internal class ServiceTransportPool
    {
        private int _activedTransportCount = 0;
        public ServiceConfig ServiceConfig { get; set; }

        public ConcurrentStack<TTransportExt> TransportPool { get; set; }

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
        internal readonly object SyncObeject = new object();
    }
    /// <summary>
    /// 数据传输对象扩展
    /// </summary>
    public class TTransportExt
    {
        /// <summary>
        /// 管道
        /// </summary>
        public Channel Channel { get; set; }
        /// <summary>
        /// 毫秒
        /// </summary>
        public int TimeOut { get; set; }
        /// <summary>
        /// BrokerService.Client 客户端对象
        /// </summary>
        public BrokerService.BrokerServiceClient Client { get; set; }

        /// <summary>
        /// 最后使用时间
        /// </summary>
        public DateTime LastDateTime { get; set; } = DateTime.Now;
    }
}
