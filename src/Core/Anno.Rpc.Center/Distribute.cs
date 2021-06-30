using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Thrift.Transport;

namespace Anno.Rpc.Center
{
    using Anno.Log;
    public static class Distribute
    {
        /// <summary>
        /// 服务检查通知事件
        /// </summary>
        public static event ServiceNotice CheckNotice = null;
        static readonly ThriftConfig Tc = ThriftConfig.CreateInstance();
        static readonly object LockHelper = new object();
        /// <summary>
        /// 获取微服务地址
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static List<Micro> GetMicro(string channel)
        {
            List<ServiceInfo> service = new List<ServiceInfo>();
            //if (channel.StartsWith("md5:"))
            //{
            //    long waitTime = 19000;
            //    var md5 = channel.Substring(4);
            //    while(md5.Equals(Tc.ServiceMd5) && waitTime > 0)
            //    {
            //        waitTime = waitTime - 10;
            //        Task.Delay(10).Wait();// Thread.Sleep(10);
            //    }
            //    service = Tc.ServiceInfoList;
            //}

            if (channel.StartsWith("md5:"))
            {
                service = Tc.ServiceInfoList;
            }
            else
            {
                service = Tc.ServiceInfoList.FindAll(i => i.Name.Contains(channel));
            }
            List<Micro> msList = new List<Micro>();
            service.ForEach(s =>
            {
                Micro micro = new Micro
                {
                    Ip = s.Ip,
                    Port = s.Port,
                    Timeout = s.Timeout,
                    Name = s.Name,
                    Nickname = s.NickName,
                    Weight = s.Weight
                };
                msList.Add(micro);
            });
            return msList;
        }
        /// <summary>
        /// 健康检查，如果连接不上 每秒做一次尝试。
        /// 尝试 errorCount 次失败，软删除。
        /// 60次失败，永久删除。
        /// </summary>
        /// <param name="service">被检测主机信息</param>
        /// <param name="errorCount">尝试 errorCount 次失败，软删除</param>
        /// <returns></returns>
        public static void HealthCheck(ServiceInfo service, int errorCount = 3)
        {
            int hc = 60;//检查次数

        hCheck://再次  心跳检测
            TTransport transport = new TSocket(service.Ip, service.Port, 10_000);
            try
            {
                service.Checking = true;
                transport.Open();
                if (transport.IsOpen)
                {
                    if (hc != 60)
                    {
                        Log.WriteLine($"{service.Ip}:{service.Port}", ConsoleColor.DarkGreen);
                        foreach (var f in service.Name.Split(','))
                        {
                            Log.WriteLine($"{f}", ConsoleColor.DarkGreen);
                        }
                        Log.WriteLine($"{"权重:" + service.Weight}", ConsoleColor.DarkGreen);
                        Log.WriteLine($"恢复正常！", ConsoleColor.DarkGreen);
                        Log.WriteLineNoDate($"-----------------------------------------------------------------------------");
                    }                   
                    transport.Flush();
                    transport.Close();
                    lock (LockHelper) //防止高并发下 影响权重
                    {
                        if (!Tc.ServiceInfoList.Exists(s => s.Ip == service.Ip && s.Port == service.Port))
                        {
                            //已存在不再添加 不存在则添加 
                            for (int i = 0; i < service.Weight; i++)
                            {
                                Tc.ServiceInfoList.Add(service);
                            }
                        }
                    }
                    if (hc <= (60 - errorCount))
                    {
                        CheckNotice?.Invoke(service, NoticeType.RecoverHealth);
                    }
                }

                transport.Dispose();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error Info:{service.Ip}:{service.Port} {ex.Message}", ConsoleColor.DarkYellow);
                if (hc == 60)
                {
                    Log.WriteLine($"{service.Ip}:{service.Port}", ConsoleColor.DarkYellow);
                    foreach (var f in service.Name.Split(','))
                    {
                        Log.WriteLine($"{f}", ConsoleColor.DarkYellow);
                    }
                    Log.WriteLine($"{"权重:" + service.Weight}", ConsoleColor.DarkYellow);
                    Log.WriteLine($"检测中···{hc}！", ConsoleColor.DarkYellow);
                    Log.WriteLineNoDate($"-----------------------------------------------------------------------------");
                }
                else if (hc == (60 - errorCount))
                {
                    Log.WriteLine($"{service.Ip}:{service.Port}", ConsoleColor.DarkYellow);
                    foreach (var f in service.Name.Split(','))
                    {
                        Log.WriteLine($"{f}", ConsoleColor.DarkYellow);
                    }
                    Log.WriteLine($"{"权重:" + service.Weight}", ConsoleColor.DarkYellow);
                    Log.WriteLine($"故障恢复中···{hc}！", ConsoleColor.DarkYellow);
                    Log.WriteLineNoDate($"-----------------------------------------------------------------------------");
                }
                else if (hc == 0) //硬删除
                {
                    Log.WriteLine($"{service.Ip}:{service.Port}", ConsoleColor.DarkYellow);
                    foreach (var f in service.Name.Split(','))
                    {
                        Console.WriteLine($"{f}");
                    }
                    Log.WriteLine($"{"权重:" + service.Weight}", ConsoleColor.DarkYellow);
                    Log.WriteLine($"永久移除···{hc}！", ConsoleColor.DarkYellow);
                    Log.WriteLineNoDate($"-----------------------------------------------------------------------------");                 
                }

                if (hc == (60 - errorCount)) //三次失败之后 临时移除 ，防止更多请求转发给此服务节点 
                {
                    //临时移除 并不从配置文件移除
                    Tc.ServiceInfoList.RemoveAll(i => i.Ip == service.Ip && i.Port == service.Port);
                    CheckNotice?.Invoke(service, NoticeType.NotHealth);
                }
                else if (hc == 0) //硬删除
                {
                    Dictionary<string, string> rp = new Dictionary<string, string>
                    {
                        {"ip", service.Ip},
                        {"port", service.Port.ToString()}
                    };
                    Tc.Remove(rp);
                    CheckNotice?.Invoke(service, NoticeType.OffLine);
                    return;
                }

                Thread.Sleep(1000); //间隔一秒 健康检查
                hc--;
                transport.Dispose();
                goto hCheck;
            }
            finally
            {
                if (transport.IsOpen)
                {
                    transport.Flush();
                    transport.Close();
                }
                transport.Dispose();
                service.Checking = false;
            }
        }
    }
}
