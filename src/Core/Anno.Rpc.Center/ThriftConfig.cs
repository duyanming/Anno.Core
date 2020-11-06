using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;
using System.Text;

namespace Anno.Rpc.Center
{
    /// <summary>
    /// 系统配置
    /// </summary>
    public class ThriftConfig
    {
        private static ThriftConfig _instance = null;
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
                StringBuilder xmlText=new StringBuilder();
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
                    int weight = input["weight"] == null ? 1 : (int) Convert.ToDecimal(input["weight"]);
                    ips.Weight = weight;
                    ServiceInfoList.RemoveAll(t => ips.Ip == t.Ip && ips.Port == t.Port);
                    for (int w = 0; w < weight; w++) //权重
                    {
                        ServiceInfoList.Add(ips);
                    }

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"{ips.Ip}:{ips.Port}");
                    ips.Name.Split(',').ToList().ForEach(f => {
                        Console.WriteLine($"{f}");
                    });
                    Console.WriteLine($"{"w:" + ips.Weight}");
                    Console.WriteLine($"{ips.NickName}已登记！");
                    Console.ResetColor();
                    Console.WriteLine($"----------------------------------------------------------------- ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
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
                Console.WriteLine(ex);
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
        private static string GetValidIp(string ips,int port)
        {
            string[] ipsArr = ips.Split(',');
            foreach (var ip in ipsArr)
            {
                if (ip != "127.0.0.1")
                {
                    Thrift.Transport.TTransport service = new Thrift.Transport.TSocket(ip,port, 300);
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
    }
}
