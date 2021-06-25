using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Anno.Const
{
    public static class SettingService
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public static string AppName { get; set; } = "AnnoService";
        /// <summary>
        /// 服务端最大工作线程数量
        /// </summary>
        public static int MaxThreads { get; set; } = 200;

        /// <summary>
        /// 本机信息
        /// </summary>
        public static Target Local { get; set; } = new Target();
        /// <summary>
        /// 权重
        /// </summary>
        public static int Weight { get; set; }
        /// <summary>
        /// 功能
        /// </summary>
        public static string FuncName { get; set; }
        /// <summary>
        /// 忽略的功能
        /// </summary>
        public static List<string> IgnoreFuncNames { get; set; } = new List<string>();
        /// <summary>
        /// 服务超时时间 单位毫秒
        /// </summary>
        public static long TimeOut { get; set; }

        /// <summary>
        /// 调用追踪开关
        /// </summary>
        public static bool TraceOnOff { get; set; } = true;
        /// <summary>
        /// 服务注册目标
        /// </summary>
        public static List<Target> Ts { get; set; } = new List<Target>();

        /// <summary>
        /// 日志配置文件路径 Log4net
        /// </summary>                      
        public static string LogCfg;

        /// <summary>
        /// 工作站
        /// </summary>
        public static long WorkerId { get; set; } = 0;
        /// <summary>
        /// 数据中心
        /// </summary>
        public static long DatacenterId { get; set; } = 0;
        /// <summary>
        /// 通过Anno.config 初始化AppSettings
        /// </summary>
        /// <param name="doc"></param>
        private static void InitConstAppSettings(XmlDocument doc)
        {
            XmlNodeList assembly = doc.SelectNodes("//IocDll/Assembly");
            foreach (XmlNode n in assembly)
            {
                AppSettings.IocDll.Add(n.InnerText);
            }
            AppSettings.ConnStr = AppSetting("ConnStr");
            AppSettings.DefaultPwd = AppSetting("DefaultPwd");
            //Redis 配置
            RedisConfigure.Default().Conn = AppSetting("redisConn");
            RedisConfigure.Default().Prefix = AppSetting("redisPrefix");
            if (double.TryParse(AppSetting("redisExpiryDate"), out double redisExpiryDate))
            {

                RedisConfigure.Default().ExpiryDate = TimeSpan.FromMinutes(redisExpiryDate);
            }
            if (bool.TryParse(AppSetting("redisSwitch"), out bool redisSwitch))
            {
                RedisConfigure.Default().Switch = redisSwitch;
            }

            //MongoDb 配置
            MongoConfigure.connectionString = AppSetting("MongoConn");
            MongoConfigure.database = AppSetting("MongodName");

            //Log4net 配置
            LogCfg = Path.Combine(Directory.GetCurrentDirectory(), "log4net.config");
        }
        /// <summary>
        /// AppSetting //appSettings/add[@key='{key}']
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string AppSetting(string key)
        {
            XmlDocument doc = GetXmlDocument();
            XmlNode appset = doc.SelectSingleNode($"//appSettings/add[@key='{key}']");
            return appset == null ? string.Empty : appset.Attributes["value"].Value;
        }
        /// <summary>
        ///NodeText   
        ///SelectSingleNode("//AppName")?.InnerText
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string NodeText(string key)
        {
            XmlDocument doc = GetXmlDocument();
            string appset = doc.SelectSingleNode($"//{key}")?.InnerText;
            return appset ?? string.Empty;
        }
        /// <summary>
        /// 获取文档
        /// </summary>
        /// <param name="docName">默认 Anno.config</param>
        /// <returns></returns>
        private static XmlDocument GetXmlDocument(string docName = "Anno.config")
        {
            string xmlPath = Path.Combine(Directory.GetCurrentDirectory(), docName);
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(xmlPath))
            {
                xmlDoc.Load(xmlPath);
            }
            else
            {
                StringBuilder xmlText = new StringBuilder();
                #region AnnoService Anno.config

                xmlText.Append(@"<?xml version='1.0' encoding='utf-8' ?>
                                <configuration>
                                  <!--0,0 第一位是 工作站，第二位数据中心
                                  （所有的 AnnoService 的 两位数不能重复例如不能存在【1,2】【1,2】）
                                  可以存在【1,2】【2,1】
                                  -->
                                  <IdWorker>0,0</IdWorker>
                                  <!--App名称-->
                                  <AppName>App001</AppName>
                                  <!--监听端口-->
                                  <Port>6659</Port>
                                  <!--权重-->
                                  <Weight>1</Weight>
                                  <!--功能  <FuncName>Anno.Plugs.LogicService,Anno.Plugs.TraceService</FuncName> -->
                                  <FuncName></FuncName>
                                  <!--忽略的功能-->
                                  <IgnoreFuncName></IgnoreFuncName>
                                  <!--超时时间毫秒-->
                                  <TimeOut>2000</TimeOut>
                                  <!--注册到的目标-->
                                  <Ts Ip='127.0.0.1' Port='6660'/>
                                  <IocDll>
                                    <!-- IOC 仓储-->
                                    <!-- <Assembly>Anno.Repository</Assembly> -->
                                    <!-- 领域-->
                                    <!-- <Assembly>Anno.Domain</Assembly> -->
                                    <!-- 查询服务-->
                                    <!-- <Assembly>Anno.QueryServices</Assembly> -->
                                    <!--事件Handler-->
                                    <!--<Assembly>Anno.Command.Handler</Assembly>-->
                                  </IocDll>
                                  <appSettings>
                                    <!-- 数据库连接字符串 Mysql-->
                                    <add key='ConnStr' value='server=127.0.0.1;database=bif;uid=bif;pwd=123456;SslMode=None;'/>
                                    <!--重置默认密码-->
                                    <add key='DefaultPwd' value='123456'/>
                                    <!--
                                    redisConn Redis 连接字符串
                                    redisPrefix Key 前缀
                                    redisExpiryDate Key 有效期  单位（分钟）
                                    redisSwitch 是否开启数据库 false 不开启
                                    -->
                                    <add key='redisConn' value='127.0.0.1:6379,abortConnect=false,allowAdmin =true,keepAlive=180'/>
                                    <add key='redisPrefix' value='Anno:'/>
                                    <add key='redisExpiryDate' value='20'/>
                                    <add key='redisSwitch' value='false'/>
                                    <!--MongoDB 配置-->
                                    <add key='MongoConn' value='mongodb://192.168.1.2'/>
                                    <add key='MongodName' value='bif'/>
                                  </appSettings>
                                  <!--RabbitMQ 配置-->
                                  <RabbitMQ key='RabbitMQ' HostName='192.168.100.173' UserName='dev' Password='dev' VirtualHost='dev' Port='5672'/>
                                </configuration>
                                ");
                #endregion
                xmlDoc.LoadXml(xmlText.ToString());
                xmlDoc.Save(xmlPath);
            }
            return xmlDoc;
        }
        /// <summary>
        /// 初始化配置Anno.config
        /// </summary>
        public static void InitConfig()
        {
            #region 配置初始化
            XmlDocument xml = GetXmlDocument();
            XmlNode local = xml.SelectSingleNode("//Port");
            Local.Port = Convert.ToInt32(NodeText("Port"));
            //Local.IpAddress = local.Attributes["Ip"].Value;
            Weight = Convert.ToInt32(NodeText("Weight"));
            AppName = NodeText("AppName");
            FuncName = NodeText("FuncName");
            if (long.TryParse(NodeText("TimeOut"), out long timeOut))
            {
                TimeOut = timeOut;
            }
            else
            {
                TimeOut = 10000;
            }
            XmlNodeList _Ts = xml.SelectNodes("//Ts");
            #region 需要忽略的 插件
            var ignoreFunc = NodeText("IgnoreFuncName");
            ignoreFunc.Split(',').ToList().ForEach(item =>
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    IgnoreFuncNames.Add(item.ToUpper());
                }
            });
            #endregion
            foreach (XmlNode ts in _Ts)
            {
                Target target = new Target
                {
                    IpAddress = ts.Attributes["Ip"].Value,
                    Port = Convert.ToInt32(ts.Attributes["Port"].Value)
                };
                Ts.Add(target);
            }
            //通过Anno.config 初始化AppSettings
            InitConstAppSettings(xml);
            //根据  SettingService.FuncName 加载服务集合
            Assemblys.InitAssembly();
            Assemblys.AddPlugsAssembly();

            CustomConfiguration.InitConst(xml);
            #endregion

            #region 初始化 ID生成器

            var idWorker = NodeText("IdWorker");
            if (!string.IsNullOrWhiteSpace(idWorker))
            {
                var wd = idWorker.Split(',');
                if (wd.Length == 2)
                {
                    long.TryParse(wd[0], out long worker);
                    long.TryParse(wd[1], out long datacenter);
                    WorkerId = worker;
                    DatacenterId = datacenter;
                }
            }
            #endregion
        }
    }
    /// <summary>
    /// 目标信息
    /// </summary>
    public class Target
    {
        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

    }

    /// <summary>
    /// 服务集
    /// </summary>
    public static class Assemblys
    {
        /// <summary>
        /// 服务集合
        /// </summary>
        public static readonly Dictionary<string, Assembly> Dic = new Dictionary<string, Assembly>();
        /// <summary>
        /// Ioc模块DLL列表
        /// </summary>
        public static List<Assembly> DependedTypes { get; set; } = new List<Assembly>();
        /// <summary>
        /// 根据  SettingService.FuncName 加载服务集合
        /// </summary>
        public static void InitAssembly()
        {
            SettingService.FuncName?.Split(',').Where(f => !string.IsNullOrWhiteSpace(f)).ToList().ForEach(f =>
            {
                string dllPath = Path.Combine(Directory.GetCurrentDirectory(), $"{f}.dll");
                Assembly assembly = null;
                if (File.Exists(dllPath))
                {
                    assembly = Assembly.UnsafeLoadFrom(dllPath); //装载组件
                }
#if DEBUG
                else if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", Environment.CommandLine.Split(Path.DirectorySeparatorChar)[Environment.CommandLine.Split(Path.DirectorySeparatorChar).Length - 2], $"{f}.dll")))
                {
                    dllPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", Environment.CommandLine.Split(Path.DirectorySeparatorChar)[Environment.CommandLine.Split(Path.DirectorySeparatorChar).Length - 2],
                        $"{f}.dll");
                    assembly = Assembly.UnsafeLoadFrom(dllPath); //装载组件
                }
#endif
                else if (!string.IsNullOrWhiteSpace(f))
                {
                    Console.WriteLine($"{f} 服务没有找到！检查目录【{Directory.GetCurrentDirectory()}】下文件是否存在！");
                }
                if (assembly != null && !Dic.ContainsKey(f))
                {
                    Dic.Add(f, assembly);

                    if (AppSettings.IocDll.All(plug => plug != f))
                    {
                        AppSettings.IocDll.Add(f);
                    }
                }
            });
        }
        /// <summary>
        /// 添加扩展Assembly
        /// </summary>
        public static void AddPlugsAssembly()
        {
            #region 根目录 插件
            var plugs = new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles()
                .Where(f => f.Name.StartsWith("Anno.Plugs.") && f.Name.EndsWith("Service.dll"))
                .ToList();
            for (int i = 0; i < plugs.Count; i++)
            {
                var plugNameService = plugs[i].Name.Remove(plugs[i].Name.Length - 4, 4);
                if (SettingService.IgnoreFuncNames.Exists(ig => plugNameService.ToUpper().Contains(ig)))
                {
                    continue;
                }
                try
                {
                    var assembly =
                        Assembly.UnsafeLoadFrom(plugs[i].FullName); //装载组件
                    if (!Dic.ContainsKey(plugNameService))
                    {
                        Dic.Add($"{plugNameService}", assembly);
                        if (!string.IsNullOrEmpty(SettingService.FuncName))
                        {
                            SettingService.FuncName += ",";
                        }
                        SettingService.FuncName += plugNameService;
                    }
                    if (AppSettings.IocDll.All(plug => plug != plugNameService))
                    {
                        AppSettings.IocDll.Add(plugNameService);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"插件【{ plugNameService}】加载出错！");
                    Console.WriteLine($"错误信息：");
                    Console.WriteLine(ex.Message);
                }

            }
            #endregion
            #region Packages 目录 插件
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Packages");
            if (Directory.Exists(basePath))
            {
                foreach (DirectoryInfo plugInfo in new DirectoryInfo(basePath).GetDirectories().Where(dir => dir.Name.StartsWith("Anno.Plugs.")))
                {
                    if (SettingService.IgnoreFuncNames.Exists(ig => plugInfo.Name.ToUpper().Trim().Contains(ig)))
                    {
                        continue;
                    }
                    try
                    {
                        var plugName = plugInfo.Name; //$"Anno.Plugs.SerialRule";
                        var plugNameService = $"{plugName}Service";
                        var plugsPath = Path.Combine(basePath, plugName, $"{plugNameService}.dll");
                        if (File.Exists(plugsPath))
                        {
                            var assembly =
                                Assembly.UnsafeLoadFrom(plugsPath); //装载组件
                            if (!Dic.ContainsKey(plugNameService))
                            {
                                Dic.Add($"{plugNameService}", assembly);
                                if (!string.IsNullOrEmpty(SettingService.FuncName))
                                {
                                    SettingService.FuncName += ",";
                                }
                                SettingService.FuncName += plugNameService;
                            }
                            if (AppSettings.IocDll.All(plug => plug != plugNameService))
                            {
                                AppSettings.IocDll.Add(plugNameService);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"插件【{ plugInfo.Name}】加载出错！");
                        Console.WriteLine($"错误信息：");
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            #endregion

        }
    }

    /// <summary>
    /// 自定义参数
    /// </summary>
    public static class CustomConfiguration
    {
        private static readonly Dictionary<string, string> settings = new Dictionary<string, string>();
        /// <summary>
        ///  自定义参数
        /// //appSettings
        /// </summary>
        public static Dictionary<string, string> Settings => settings;

        public static void InitConst(XmlDocument doc)
        {
            XmlNode appSettings = doc.SelectSingleNode("//appSettings");
            if (appSettings == null || appSettings.ChildNodes.Count <= 0)
            {
                return;
            }

            foreach (XmlNode node in appSettings.ChildNodes)
            {
                try
                {
                    if (node.Attributes != null)
                    {
                        string key = node.Attributes["key"].Value;
                        if (!string.IsNullOrWhiteSpace(key) && settings.ContainsKey(key) == false)
                        {
                            string value = node.Attributes["value"].Value;
                            settings.Add(key, value);
                        }
                    }
                }
                catch
                {
                    //
                }
            }
        }
    }
}
