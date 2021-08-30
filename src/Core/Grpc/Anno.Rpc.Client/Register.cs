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
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"本机【{SettingService.AppName}】：");
                    foreach (var ip in micro.Ip.Split(','))
                    {
                        stringBuilder.AppendLine($"{ip}");
                    }
                    stringBuilder.AppendLine($"已注册到：{target.IpAddress}");
                    Log.Anno(stringBuilder.ToString(),typeof(Register));
                    Log.WriteLine($"已注册到：{target.IpAddress}");
                }
                return rlt;
            }
            catch (Exception ex)
            {
                Task.Delay(1000).Wait();//间隔一秒后重新注册
                if (countDown > 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"注册到{target.IpAddress}:{target.Port}失败......剩余重试次数（{countDown}）");
                    stringBuilder.AppendLine(ex.Message);
                    Log.Anno(stringBuilder.ToString(), typeof(Register));
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
                    Log.Anno($"{DateTime.Now} 未连接到{target.IpAddress}:{target.Port}注册失败......", typeof(Register));
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
            Log.Anno("找不到有效IPv4地址！", typeof(Register));
            return string.Empty;
        }
    }
}
