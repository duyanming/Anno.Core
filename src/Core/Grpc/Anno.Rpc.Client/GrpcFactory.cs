using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Anno.CronNET;
using Grpc.Core;

namespace Anno.Rpc.Client
{
    /// <summary>
    /// Transport 工厂
    /// </summary>
    internal static class GrpcFactory
    {
        /// <summary>
        /// 服务连接列表
        /// </summary>
        //private static volatile List<ServiceTransportPool> _transportPools = new List<ServiceTransportPool>();

        private static volatile ConcurrentDictionary<string, ServiceTransportPool> _tranPool = new ConcurrentDictionary<string, ServiceTransportPool>();

        /// <summary>
        /// 根据配置信息增加一个服务 连接池（增加服务） 如果已经存在 则不再添加
        /// </summary>
        /// <param name="service"></param>
        private static void AddServicePool(ServiceConfig service)
        {
            #region 如果已经在服务列表中 不再添加直接结束          
            if (_tranPool.ContainsKey(service.Id))
            {
                return;
            }
            ServiceTransportPool stp = new ServiceTransportPool()
            {
                ServiceConfig = service,
                TransportPool = new ConcurrentStack<TTransportExt>(),
                ResetEvent = new AutoResetEvent(false)
            };
            _tranPool.TryAdd(service.Id, stp);
            #endregion

        }
        /// <summary>
        /// 同步连接池
        /// </summary>
        /// <param name="scs"></param>
        public static void Synchronization(List<ServiceConfig> scs)
        {
            if (scs != null)
            {
                var removes = _tranPool
                      .Where(service => !scs.Exists(s => s.Id == service.Key))
                      .Select(p => p.Key).ToList();

                removes.ForEach(RemoveServicePool);
                scs.ForEach(AddServicePool);
            }
        }

        /// <summary>
        /// 移除服务
        /// </summary>
        /// <param name="id"></param>
        public static void RemoveServicePool(string id)
        {
            _tranPool.TryRemove(id, out ServiceTransportPool transpool);
        }
        /// <summary>
        /// 连接池借出
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TTransportExt BorrowInstance(string id)
        {
            if (!_tranPool.TryGetValue(id, out var transpool))
            {
                throw new GrpcException(GrpcException.ExceptionType.ServerCaptured,$"未找到服务【{id}】");
            }
            if (!transpool.TransportPool.TryPop(out var transport))
            {
                if (transpool.TransportPool.Count < transpool.ServiceConfig.MinIdle && transpool.ActivedTransportCount < transpool.ServiceConfig.MaxActive)
                {
                    transpool.TransportPool.Push(CreateTransport(transpool.ServiceConfig));
                }
                if (!transpool.TransportPool.Any() && transpool.ActivedTransportCount >= transpool.ServiceConfig.MaxActive)
                {
                    //Console.WriteLine("WaitingTime");
                    bool result = transpool.ResetEvent.WaitOne(transpool.ServiceConfig.WaitingTimeout);
                    //Console.WriteLine("WaitingTimeoutEnd");
                    if (!result)
                    {
                        throw new GrpcException(GrpcException.ExceptionType.Timeout,$"Timeout连接池等待超时！");
                        //monitor.TimeoutNotify(transpool.ServiceConfig.Name, transpool.ServiceConfig.WaitingTimeout);
                    }
                }
                if (!transpool.TransportPool.TryPop(out transport))
                {
                    throw new GrpcException(GrpcException.ExceptionType.ServerUnkown, "连接池异常");
                }
            }
            transpool.InterlockedIncrement();
            return transport;
        }
        /// <summary>
        /// 归还连接池
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="id"></param>
        public static void ReturnInstance(TTransportExt transport, string id)
        {
            if (!_tranPool.TryGetValue(id, out var transpool))
            {
                transport.Channel.ShutdownAsync();
                //可能服务连接池已经被移除
                return;
            }
            if (transpool.TransportPool.Count >= transpool.ServiceConfig.MaxIdle
                || transport.Channel.State == ChannelState.Shutdown
                || transport.Channel.State == ChannelState.TransientFailure)//已经断开的连接直接释放
            {
                transport.Channel.ShutdownAsync();
            }
            else
            {
                transport.LastDateTime = DateTime.Now; //记录最后访问时间
                transpool.TransportPool.Push(transport);
            }
            transpool.InterlockedDecrement();
            transpool.ResetEvent.Set();

        }
        /// <summary>
        /// 创建Transport 传输对象
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static TTransportExt CreateTransport(ServiceConfig config)
        {
            var channel = new Channel($"{config.Ip}:{config.Port}", ChannelCredentials.Insecure);
            return new TTransportExt()
            {
                TimeOut = config.Timeout,
                Channel = channel,
                Client = new BrokerService.BrokerServiceClient(channel),
                LastDateTime = DateTime.Now
            };
        }
        /// <summary>
        /// 30秒没用的空闲连接
        /// </summary>
        private static readonly TimeSpan Timespan = TimeSpan.FromSeconds(30);
        /// <summary>
        /// 定时清理空闲链接
        /// </summary>
        /// <returns></returns>
        internal static void CleanPoolLink()
        {
            try
            {
                List<TTransportExt> invalidLink = null;  //无效连接
                var nowDiff = DateTime.Now - Timespan;
                _tranPool.Keys.ToList().ForEach(key =>
                {
                    if (!_tranPool.TryGetValue(key, out var pool))
                    {
                        return;
                    }
                    //只有当 空闲线程存在 30秒没用的空闲连接 才做清理 。防止影响正在运行的 线程
                    if (pool.TransportPool.Count > pool.ServiceConfig.MinIdle && pool.TransportPool.ToList().Exists(t => t.LastDateTime < nowDiff))
                    {
                        if (pool.TransportPool.ToList().Exists(t => t.LastDateTime < nowDiff))
                        {
                            //有效连接
                            var validLink = pool.TransportPool.Where(t => t.LastDateTime >= nowDiff).ToList();
                            //无效连接
                            invalidLink = pool.TransportPool.Where(t => t.LastDateTime < nowDiff).ToList();

                            pool.TransportPool.Clear();
                            if (validLink.Any())
                            {
                                pool.TransportPool.PushRange(validLink.ToArray());
                            }
                        }
                        if (pool.TransportPool.Count <= 0 && invalidLink != null && invalidLink.Any())//保存一条存货链接
                        {
                            pool.TransportPool.PushRange(invalidLink.Take(pool.ServiceConfig.MinIdle).ToArray());
                            invalidLink = invalidLink.Skip(pool.ServiceConfig.MinIdle).ToList();
                        }
                    }
                });
                if (invalidLink != null)
                {
                    foreach (var invalid in invalidLink)
                    {
                        try
                        {
                            invalid.Channel.ShutdownAsync().Wait();
                        }
                        catch
                        {//回收连接池
                        }
                    }
                }
            }
            catch
            {//回收连接池
            }
        }
    }

}
