using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Thrift.Transport;
using Thrift.Server;
using System.Threading;
using Thrift.Protocol;
using System.Threading.Tasks;
using Anno.Const;
//using Polly;
using Thrift;
using Anno.Const.Enum;

namespace Anno.Rpc.Client
{
    /// <summary>
    /// 客户端连接器
    /// 保留关键字【TraceId,PreTraceId,AppName,AppNameTarget,GlobalTraceId,TTL,X-Original-For】
    /// </summary>
    public static partial class Connector
    {
        /// <summary>
        /// Dns 缓存
        /// </summary>
        private static volatile List<MicroCache> _microCaches = new List<MicroCache>();

        /// <summary>
        /// 处理器代理
        /// </summary>
        /// <param name="input">键值对</param>
        /// <returns>字符串结果</returns>
        public static string BrokerDns(Dictionary<string, string> input)
        {
            string output;
            try
            {
                if (!input.ContainsKey(Eng.NAMESPACE) || !input.ContainsKey(Eng.CLASS) ||
                    !input.ContainsKey(Eng.METHOD))
                {
                    return FailMessage($"缺少必输关键字（{Eng.NAMESPACE}、{Eng.CLASS}、{Eng.METHOD}）");
                }

                //var retryPolicy =
                //     Policy
                //         .Handle<TApplicationException>().Or<TTransportException>().Or<Exception>()
                //         .WaitAndRetry(new[] {
                //            TimeSpan.FromSeconds(0.1),
                //            TimeSpan.FromSeconds(0.5),
                //            TimeSpan.FromSeconds(1) });
                //retryPolicy.Execute(() =>
                //{
                #region 获取目标服务器信息
                var caches = Single(input[Eng.NAMESPACE]);
                if (caches != null)
                {
                    output = BrokerDnsInner(input, caches.Mi);
                }
                else
                {
                    return FailMessage($"未找到服务【{input[Eng.NAMESPACE]}】");
                }
                #endregion

                //});
            }
            catch (Exception e)
            {
                output = FailMessage(e.Message);
            }

            return output;
        }
        /// <summary>
        /// 处理器代理 设定目标服务
        /// </summary>
        /// <param name="input">键值对</param>
        /// <returns>字符串结果</returns>
        public static string BrokerDns(Dictionary<string, string> input, Micro micro)
        {
            string output;
            try
            {
                if (!input.ContainsKey(Eng.NAMESPACE) || !input.ContainsKey(Eng.CLASS) ||
                    !input.ContainsKey(Eng.METHOD))
                {
                    return FailMessage($"缺少必输关键字（{Eng.NAMESPACE}、{Eng.CLASS}、{Eng.METHOD}）");
                }

                //var retryPolicy =
                //     Policy
                //         .Handle<TApplicationException>().Or<TTransportException>().Or<Exception>()
                //         .WaitAndRetry(new[] {
                //            TimeSpan.FromSeconds(0.1),
                //            TimeSpan.FromSeconds(0.5),
                //            TimeSpan.FromSeconds(1) });
                //retryPolicy.Execute(() =>
                //{
                output = BrokerDnsInner(input, micro);
                //});
            }
            catch (Exception e)
            {
                output = FailMessage(e.Message);
            }

            return output;
        }

        /// <summary>
        /// 异步 处理器代理
        /// </summary>
        /// <param name="input">键值对</param>
        /// <returns>字符串结果</returns>
        public static async Task<string> BrokerDnsAsync(Dictionary<string, string> input)
        {
            return await Task.Run(() => BrokerDns(input));
        }
        /// <summary>
        /// 异步 处理器代理 设定目标服务
        /// </summary>
        /// <param name="input">键值对</param>
        /// <returns>字符串结果</returns>
        public static async Task<string> BrokerDnsAsync(Dictionary<string, string> input, Micro micro)
        {
            return await Task.Run(() => BrokerDns(input, micro));
        }
        /// <summary>
        /// 异步 处理器代理
        /// </summary>
        /// <param name="input">键值对</param>
        ///  <param name="nickName">昵称</param>
        /// <returns>字符串结果</returns>
        public static async Task<string> BrokerDnsAsync(Dictionary<string, string> input, string nickName)
        {
            return await Task.Run(() =>
            {
                var micro = _microCaches.FirstOrDefault(m => m.Mi.Nickname == nickName)?.Mi;
                return BrokerDns(input, micro);
            });
        }

        public static string BrokerDns<T>(T input) where T : IInputDto
        {
            return BrokerDns(input.ToDic());
        }
        public static string BrokerDns<T>(T input, Micro micro) where T : IInputDto
        {
            return BrokerDns(input.ToDic(), micro);
        }
        public static Task<string> BrokerDnsAsync<T>(T input) where T : IInputDto
        {
            return BrokerDnsAsync(input.ToDic());
        }
        public static Task<string> BrokerDnsAsync<T>(T input, Micro micro) where T : IInputDto
        {
            return BrokerDnsAsync(input.ToDic(), micro);
        }
        /// <summary>
        /// 对象转换字典
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static Dictionary<string, string> ToDic(this object input)
        {
            Dictionary<string, string> _input = new Dictionary<string, string>();
            var properties = input.GetType().GetProperties().Where(p => p.CanRead).ToList();
            for (int i = 0; i < properties.Count; i++)
            {
                var prop = properties[i];
                var value = prop.GetValue(input);
                Type types = prop.PropertyType;
                if (types.FullName.IndexOf("System.", StringComparison.Ordinal) != -1)
                {
                    _input.Add(prop.Name, value.ToString());
                }
                else
                {
                    _input.Add(prop.Name, Newtonsoft.Json.JsonConvert.SerializeObject(value));
                }
            }
            return _input;
        }
        private static string BrokerDnsInner(Dictionary<string, string> input, Micro micro)
        {
            string output = string.Empty;
            #region 调用链

            var trace = TransmitTrace.SetTraceId(input, micro);

            #endregion
            #region 处理请求
            try
            {
                using (Request request = new Request(micro.Ip, micro.Port))
                {
                    output = request.Invoke(input);
                }
            }
            catch (ThriftException ex)
            {
                if (ex.Type.Equals(ExceptionType.NotFoundService))
                {
                    _microCaches.RemoveAll(c => c.Mi.Ip == micro.Ip && c.Mi.Port == micro.Port);
                }
                output = FailMessage(ex.Message);
            }
            catch (Exception ex) //如果异常则从缓存中清除 该缓存
            {
                _microCaches.RemoveAll(c => c.Mi.Ip == micro.Ip && c.Mi.Port == micro.Port);
                output = FailMessage(ex.Message);
            }
            finally
            {
                TracePool.EnQueue(trace, output);
            }

            #endregion

            return output;
        }

        /// <summary>
        /// 构建错误消息Json字符串
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="status">默认False</param>
        /// <returns>"{\"Msg\":\""+message+"\",\"Status\":false,\"Output\":null,\"OutputData\":null}"</returns>
        internal static string FailMessage(string message, bool status = false)
        {
            return "{\"Msg\":\"" + message + "\",\"Status\":" + status.ToString().ToLower() +
                   ",\"Output\":null,\"OutputData\":null}";
        }

        /// <summary>
        /// 路由管道
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        static MicroCache Single(string channel)
        {
            var ms = _microCaches.FindAll(m => m.Tags.Exists(t => t == channel));
            Random rd = new Random(Guid.NewGuid().GetHashCode());
            if (ms.Count > 0)
            {
                return ms[rd.Next(0, ms.Count)];
            }

            return null;
        }

        #region 更新服务缓存

        private static TTransport _proxyCenter = new TSocket(SettingService.Local.IpAddress, SettingService.Local.Port, 30000);
        private static TProtocol _protocol = new TBinaryProtocol(_proxyCenter);
        private static BrokerCenter.Client _client = new BrokerCenter.Client(_protocol);
        private static bool connectionCenterInit = false;
        /// <summary>
        /// 服务MD5值
        /// </summary>
        internal static string ServiceMd5 { get; private set; }
        /// <summary>
        /// 更新服务缓存
        /// </summary>
        /// <param name="channel">管道</param>
        internal static void UpdateCache(string channel)
        {
            #region 到DNS中心取服务信息
            try
            {
                if (string.IsNullOrWhiteSpace(channel))
                {
                    RefreshServiceMd5();
                    channel = ServiceMd5;
                }
                if (!_proxyCenter.IsOpen)
                {
                    _proxyCenter.Open();
                }
                DateTime now = DateTime.Now; //获取缓存时间
                var microList = _client.GetMicro(channel);
                #region Micro +添加到缓存

                if (microList != null && microList.Count > 0)
                {
                    var microCaches = new List<MicroCache>();
                    microList.ForEach(m =>
                    {
                        microCaches.Add(new MicroCache()
                        {
                            LasTime = now,
                            Mi = m,
                            Tags = m.Name.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Substring(0, t.Length - 7)).ToList()
                        });
                    });
                    _microCaches = microCaches;

                    #region 同步服务到连接池
                    var scs = new List<ServiceConfig>();
                    _microCaches.ForEach(mc =>
                    {
                        if (!scs.Exists(s => s.Host == mc.Mi.Ip && s.Port == mc.Mi.Port))
                        {
                            scs.Add(new ServiceConfig()
                            {
                                Host = mc.Mi.Ip,
                                Port = mc.Mi.Port,
                                Timeout = mc.Mi.Timeout
                            });
                        }
                    });
                    ThriftFactory.Synchronization(scs);

                    #endregion
                }
                else
                {
                    _microCaches.Clear();
                    ThriftFactory.Synchronization(new List<ServiceConfig>());
                }

                #endregion
            }
            catch (Exception ex)
            {
                try
                {
                    if (connectionCenterInit == false)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff") + $":注册中心 {SettingService.Local.IpAddress}:{SettingService.Local.Port} " + ex.Message);
                        Console.ResetColor();
                        connectionCenterInit = true;
                    }
                    // return (null, FailMessage($"负载中心连接失败！"));
                    if (_proxyCenter.IsOpen)
                    {
                        _proxyCenter.Flush();
                        _proxyCenter.Close();
                    }

                    _proxyCenter.Dispose();

                    _proxyCenter = new TSocket(SettingService.Local.IpAddress, SettingService.Local.Port, 30000);
                    _protocol = new TBinaryProtocol(_proxyCenter);
                    _client = new BrokerCenter.Client(_protocol);
                }
                catch
                {
                    // ignored
                }
            }
            #endregion
        }
        /// <summary>
        /// 刷新Md5值
        /// </summary>
        internal static void RefreshServiceMd5()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var service in _microCaches)
            {
                stringBuilder.Append(service.Mi.Name);
                stringBuilder.Append("#");
                stringBuilder.Append(service.Mi.Nickname);
                stringBuilder.Append("#");
                stringBuilder.Append(service.Mi.Ip);
                stringBuilder.Append("#");
                stringBuilder.Append(service.Mi.Port);
                stringBuilder.Append("#");
                stringBuilder.Append(service.Mi.Timeout);
                stringBuilder.Append("#");
                stringBuilder.Append(service.Mi.Weight);
            }
            ServiceMd5 ="md5:"+ stringBuilder.ToString().HashCode();
        }
        #endregion
    }
}
