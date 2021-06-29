using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Anno.Const;
using Grpc.Core;

namespace Anno.Rpc.Client
{
    using Anno.Log;
    /// <summary>
    /// 注册中心
    /// </summary>
    public class Register
    {
        private  Channel _channel;
        private  BrokerCenter.BrokerCenterClient _client;
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="target">注册目标</param>
        /// <param name="countDown">注册超时次数</param>
        /// <returns></returns>
        public bool ToCenter(Target target, int countDown = 10)
        {

        begin:
            try
            {
                _channel = new Channel($"{target.IpAddress}:{target.Port}", ChannelCredentials.Insecure);
                _client = new BrokerCenter.BrokerCenterClient(_channel);
                Micro micro = new Micro();
                micro.Timeout = (int)SettingService.TimeOut;
                micro.Name = SettingService.FuncName;
                micro.Ip = SettingService.Local.IpAddress == null ? GetLocalIps() : SettingService.Local.IpAddress;
                micro.Port = SettingService.Local.Port;
                micro.Weight = SettingService.Weight;
                micro.Nickname = SettingService.AppName;
                bool rlt = (_client.Add_broker(micro).Reply=="1");
                if (rlt)
                {
                    Log.WriteLine($"本机【{SettingService.AppName}】：", ConsoleColor.DarkGreen);
                    foreach (var ip in micro.Ip.Split(','))
                    {
                        Log.WriteLine($"{ip}", ConsoleColor.DarkGreen);
                    }
                    Log.WriteLine($"已注册到：{target.IpAddress}", ConsoleColor.DarkGreen);
                    Log.WriteLineNoDate(" -----------------------------------------------------------------------------");
                }
                return rlt;
            }
            catch (Exception ex)
            {
                Thread.Sleep(1000);//间隔一秒后重新注册
                if (countDown > 0)
                {
                    Log.WriteLine($"注册到{target.IpAddress}:{target.Port}失败......剩余重试次数（{countDown}）", ConsoleColor.DarkYellow);
                    Log.WriteLineNoDate($"错误信息：{ex.Message}");
                    try
                    {
                        _client = new BrokerCenter.BrokerCenterClient(_channel);
                    }
                    catch
                    {
                        //忽略异常
                    }
                    --countDown;
                    goto begin;
                }
                else
                {
                    Log.WriteLine($"{DateTime.Now} 未连接到{target.IpAddress}:{target.Port}注册失败......", ConsoleColor.DarkRed);
                }

            }
            return true;
        }

        /// <summary>
        /// 获取本机IPv4集合字符串
        /// </summary>
        /// <returns></returns>
        private static string GetLocalIps()
        {
            var addresses = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().Where(nw => nw.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                .Select(p => p.GetIPProperties())
                .SelectMany(p => p.UnicastAddresses)
                .Where(p => p.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                            !System.Net.IPAddress.IsLoopback(p.Address)).Select(p => p.Address.ToString()).ToList();
            if (addresses.Count > 0)
            {
                return string.Join(",", addresses.ToList());
            }
            Log.WriteLine("找不到有效IPv4地址！", ConsoleColor.DarkYellow);
            return string.Empty;
        }
    }
}
