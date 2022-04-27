using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;
using System.Text;

namespace Anno.Rpc.Center
{
    using Anno.Log;
    /// <summary>
    /// 系统配置
    /// </summary>
    public class ThriftConfig
    {
        private static ThriftConfig _instance = null;
        /// <summary>
        /// 服务上线通知事件
        /// </summary>
        public event ServiceNotice OnlineNotice = null;
        /// <summary>
        /// 服务更改通知事件
        /// </summary>
        public event ServiceChangeNotice ChangeNotice = null;
        /// <summary>
        /// AnnoCenter 默认配置文件名称
        /// </summary>
        public static string AnnoFile { get; set; } = "Anno.config";
        private static readonly object LockHelper = new object();

        private static readonly object LockAdd = new object();

        private ThriftConfig()
        {
            Init();
        }
        public Int32 Port { get; set; }
        private Int32 TimeOut { get; set; }
        public readonly List<ServiceInfo> ServiceInfoList = new List<ServiceInfo>();
        /// <summary>
        /// 服务MD5值
        /// </summary>
        internal string ServiceMd5 { get; private set; }
        /// <summary>
        /// 刷新Md5值
        /// </summary>
        internal void RefreshServiceMd5()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var service in ServiceInfoList)
            {
                stringBuilder.Append(service.Name);
                stringBuilder.Append("#");
                stringBuilder.Append(service.NickName);
                stringBuilder.Append("#");
                stringBuilder.Append(service.Ip);
                stringBuilder.Append("#");
                stringBuilder.Append(service.Port);
                stringBuilder.Append("#");
                stringBuilder.Append(service.Timeout);
                stringBuilder.Append("#");
                stringBuilder.Append(service.Weight);
            }
            ServiceMd5 = stringBuilder.ToString().HashCode();
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public static ThriftConfig CreateInstance()
        {
            if (_instance == null)
            {
                lock (LockHelper)
                {
                    if (_instance == null)
                        _instance = new ThriftConfig();
                }
            }
            return _instance;
        }

        /// <summary>
        /// 预加载
        /// </summary>
        private void Init()
        {
            string xmlPath = Path.Combine(Directory.GetCurrentDirectory(), AnnoFile);
            if (File.Exists(xmlPath))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(xmlPath);
                this.Port = Convert.ToInt32(xml.GetElementsByTagName("Port")[0].InnerText);
                TimeOut = Convert.ToInt32(xml.GetElementsByTagName("TimeOut")[0].InnerText);
                foreach (XmlNode n in xml.SelectNodes("//configuration/Servers/dc"))
                {
                    try
                    {
                        var svrInfo = new ServiceInfo
                        {
                            Timeout = n.Attributes["timeout"] == null ? TimeOut : Convert.ToInt32(n.Attributes["timeout"].Value),
                            Name = n.Attributes["name"].Value,
                            NickName = n.Attributes["nickname"].Value,
                            Ip = n.Attributes["ip"].Value,
                            Port = Convert.ToInt32(n.Attributes["port"].Value)
                        };
                        int weight = n.Attributes["weight"] == null ? 1 : (int)Convert.ToDecimal(n.Attributes["weight"].Value);
                        svrInfo.Weight = weight;
                        for (int w = 0; w < weight; w++)//权重
                        {
                            ServiceInfoList.Add(svrInfo);
                        }
                    }
                    catch
                    {
                        //配置错误
                    }
                }
            }
            else
            {
                this.Port = 6660;
                this.TimeOut = 120000;

                XmlDocument xmlDoc = new XmlDocument(); //创建空的XML文档 
                StringBuilder xmlText = new StringBuilder();
                xmlText.AppendLine("<?xml version='1.0' encoding='utf-8'?>");
                xmlText.AppendLine("<configuration>");
                xmlText.AppendLine("  <!--#lbs 配置-->");
                xmlText.AppendLine("  <Port>6660</Port>");
                xmlText.AppendLine("  <TimeOut>120000</TimeOut>");
                xmlText.AppendLine("  <Servers>");

                xmlText.AppendLine("  </Servers>");
                xmlText.AppendLine("</configuration>");
                xmlDoc.LoadXml(xmlText.ToString());
                xmlDoc.Save(xmlPath); //保存 
            }
            this.RefreshServiceMd5();
        }

        /// <summary>
        /// 移除节点
        /// </summary>
        /// <param name="input">ip=192.168.X.X,port=6659</param>
        public bool Remove(Dictionary<string, string> input)
        {
            lock (LockAdd)
            {
                Save();
            }

            return true;
        }
        /// <summary>
        /// 加载节点
        /// </summary>
        /// <param name="input">"name=dc1,ip=192.168.X.X,port=6659,timeout=3000,weight=5"</param>
        public bool Add(Dictionary<string, string> input)
        {
            lock (LockAdd)
            {
                try
                {
                    var ip = GetValidIp(input["ip"], Convert.ToInt32(input["port"]));
                    if (ip == string.Empty)
                    {
                        return false;
                    }
                    ServiceInfo ips = new ServiceInfo
                    {
                        Timeout = input["timeout"] == null ? TimeOut : Convert.ToInt32(input["timeout"]),
                        Name = input["name"],
                        NickName = input["nickname"],
                        Ip = ip,
                        Port = Convert.ToInt32(input["port"])
                    };
                    int weight = input["weight"] == null ? 1 : (int)Convert.ToDecimal(input["weight"]);
                    ips.Weight = weight;
                    #region 原有服务
                    var oldService = ServiceInfoList.FirstOrDefault(t => ips.Ip == t.Ip && ips.Port == t.Port);
                    #endregion

                    ServiceInfoList.RemoveAll(t => ips.Ip == t.Ip && ips.Port == t.Port);
                    for (int w = 0; w < weight; w++) //权重
                    {
                        ServiceInfoList.Add(ips);
                    }

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"{ips.Ip}:{ips.Port}");
                    foreach (var f in ips.Name.Split(','))
                    {
                        stringBuilder.AppendLine($"{f}");
                    }
                    stringBuilder.AppendLine($"{"权重:" + ips.Weight}");
                    stringBuilder.AppendLine($"{ips.NickName}已登记！");
                    Log.Anno(stringBuilder.ToString(), typeof(ThriftConfig));

                    #region 上线和变更通知                   
                    if (OnlineNotice != null && oldService == null)
                    {
                        OnlineNotice.Invoke(ips, NoticeType.OnLine);
                    }
                    else if (ChangeNotice != null && oldService != null)
                    {
                        ChangeNotice.Invoke(ips, oldService);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Log.Anno(ex, typeof(ThriftConfig));
                    return false;
                }
                finally
                {
                    Save();
                }
            }
            return true;
        }
        /// <summary>
        /// 变更服务信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool ChangeMicroServiceWeight(Dictionary<string, string> input)
        {
            lock (LockAdd)
            {
                try
                {
                    if (!input.ContainsKey("ip") || !input.ContainsKey("port") || (!input.ContainsKey("weight") && !input.ContainsKey("timeout")))
                    {
                        return false;
                    }
                    var ip = input["ip"];
                    int.TryParse(input["port"], out int port);
                    var hasWeight = false;
                    int weight = 0;
                    int timeout = 0;
                    if (input.ContainsKey("weight"))
                    {
                        hasWeight = int.TryParse(input["weight"], out weight);
                    }
                    var hasTimeOut = false;
                    if (input.ContainsKey("timeout"))
                    {
                        hasTimeOut = int.TryParse(input["timeout"], out timeout);
                    }
                    #region 原有服务
                    var oldService = ServiceInfoList.FirstOrDefault(t => ip == t.Ip && port == t.Port);
                    if (oldService == null)
                    {
                        return false;
                    }
                    #endregion
                    ServiceInfo serviceInfo = new ServiceInfo()
                    {
                        Timeout = oldService.Timeout,
                        Name = oldService.Name,
                        NickName = oldService.NickName,
                        Ip = oldService.Ip,
                        Port = oldService.Port,
                        Weight = oldService.Weight
                    };
                    if (hasTimeOut)
                        serviceInfo.Timeout = timeout;
                    if (hasWeight)
                        serviceInfo.Weight = weight;

                    ServiceInfoList.RemoveAll(t => ip == t.Ip && port == t.Port);

                    for (int w = 0; w < weight; w++) //权重
                    {
                        ServiceInfoList.Add(serviceInfo);
                    }
                    /*
                     * 至少保留一条服务信息数据
                     */
                    if (weight <= 0)
                    {
                        ServiceInfoList.Add(serviceInfo);
                    }

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"{serviceInfo.Ip}:{serviceInfo.Port}");
                    foreach (var f in serviceInfo.Name.Split(','))
                    {
                        stringBuilder.AppendLine($"{f}");
                    }
                    stringBuilder.AppendLine($"{"权重:" + serviceInfo.Weight}");
                    stringBuilder.AppendLine($"{serviceInfo.NickName}已登记！");
                    Log.Anno(stringBuilder.ToString(), typeof(ThriftConfig));

                    #region 上线和变更通知                   
                    if (ChangeNotice != null && oldService != null)
                    {
                        ChangeNotice.Invoke(serviceInfo, oldService);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Log.Anno(ex, typeof(ThriftConfig));
                    return false;
                }
                finally
                {
                    Save();
                }
            }
            return true;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        private bool Save()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(Path.Combine(Directory.GetCurrentDirectory(), AnnoFile));
                XmlNode servers = xml.SelectSingleNode("//configuration/Servers");//查找<Servers> 
                servers.RemoveAll();
                List<ServiceInfo> tempIps = new List<ServiceInfo>();
                ServiceInfoList.ForEach(p =>
                {
                    if (tempIps.FindAll(t => p.Ip == t.Ip && p.Port == t.Port).Count <= 0)
                    {
                        tempIps.Add(p);
                    }
                });
                tempIps.ForEach(p =>
                {
                    XmlElement xe = xml.CreateElement("dc");
                    xe.SetAttribute("name", p.Name);
                    xe.SetAttribute("nickname", p.NickName);
                    xe.SetAttribute("ip", p.Ip);
                    xe.SetAttribute("port", p.Port.ToString());
                    xe.SetAttribute("timeout", p.Timeout.ToString());
                    xe.SetAttribute("weight", p.Weight.ToString());
                    servers.AppendChild(xe);
                });
                xml.Save(Path.Combine(Directory.GetCurrentDirectory(), AnnoFile));
            }
            catch (Exception ex)
            {
                Log.Anno(ex, typeof(ThriftConfig));
                return false;
            }
            return true;
        }
        /// <summary>
        /// 从IP列表获取有效IP
        /// </summary>
        /// <param name="ips"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private static string GetValidIp(string ips, int port)
        {
            string[] ipsArr = ips.Split(',');
            foreach (var ip in ipsArr)
            {
                if (ip != "127.0.0.1")
                {
                    Thrift.Transport.TTransport service = new Thrift.Transport.TSocket(ip, port, 300);
                    try
                    {
                        if (!service.IsOpen)
                        {
                            service.Open();
                            return ip;
                        }
                    }
                    catch
                    {
                        //
                    }
                    finally
                    {
                        if (service.IsOpen)
                        {
                            service.Close();
                            service.Dispose();
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
    /// <summary>
    /// 节点信息
    /// </summary>
    public class ServiceInfo
    {
        /// <summary>
        /// 节点功能Tag
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 节点昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 节点IP
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 节点端口
        /// </summary>
        public Int32 Port { get; set; }
        /// <summary>
        /// 节点超时时间
        /// </summary>
        public Int32 Timeout { get; set; }
        /// <summary>
        /// 权重
        /// </summary>
        public Int32 Weight { get; set; }
        /// <summary>
        /// 是否正在检测
        /// </summary>
        public bool Checking { get; set; } = false;

        /// <summary>
        /// 是否已临时移除
        /// </summary>
        public bool IsTemporaryRemove { get; set; } = false;
    }
}
