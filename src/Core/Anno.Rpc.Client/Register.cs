using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Anno.Const;
using Thrift.Protocol;
using Thrift.Transport;

namespace Anno.Rpc.Client
{
    using Anno.Log;
    /// <summary>
    /// 注册中心
    /// </summary>
    public class Register
    {
        TTransport _transport;
        BrokerCenter.Client _client;
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
                _transport = new TSocket(target.IpAddress, target.Port, 3000);
                TProtocol protocol = new TBinaryProtocol(_transport);
                _client = new BrokerCenter.Client(protocol);

                if (!_transport.IsOpen)
                {
                    _transport.Open();
                }
                Dictionary<string, string> info = new Dictionary<string, string>
                    {
                        { "timeout",SettingService.TimeOut.ToString() },
                        { "name", SettingService.FuncName },
                        { "ip", SettingService.Local.IpAddress==null?GetLocalIps():SettingService.Local.IpAddress },
                        { "port", SettingService.Local.Port.ToString() },
                        { "weight", SettingService.Weight.ToString() },
                        { "nickname", SettingService.AppName }
                    };
                bool rlt = _client.add_broker(info);
                _transport.Close();
                if (rlt)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"本机【{SettingService.AppName}】：");
                    foreach (var ip in info["ip"].Split(','))
                    {
                        stringBuilder.AppendLine($"{ip}");
                    }
                    stringBuilder.AppendLine($"已注册到：{target.IpAddress}");
                    Log.Anno(stringBuilder.ToString(), typeof(Register));
                    Log.WriteLine($"已注册到：{target.IpAddress}");
                }
                return rlt;
            }
            catch (Exception ex)
            {
                Thread.Sleep(1000);//间隔一秒后重新注册
                if (countDown > 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"注册到{target.IpAddress}:{target.Port}失败......剩余重试次数（{countDown}）");
                    stringBuilder.AppendLine(ex.Message);
                    Log.Anno(stringBuilder.ToString(), typeof(Register));
                    try
                    {
                        if (_transport.IsOpen)
                        {
                            _transport.Close();
                        }
                        _transport.Dispose();
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
            finally
            {
                try
                {
                    if (_transport.IsOpen)
                    {
                        _transport.Close();
                    }
                    _transport.Dispose();
                }
                catch
                {
                    //忽略异常
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
            Log.Anno("找不到有效IPv4地址！",typeof(Register));
            return string.Empty;
        }
    }
}
