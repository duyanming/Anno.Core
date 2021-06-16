using System;
using System.Linq;
using System.Threading;

namespace Anno.Rpc.Server
{
    using System.Collections.Generic;
    using Anno.Rpc.Storage;
    using System.Reflection;
    using System.Threading;
    using Anno.Const.Attribute;
    public static class Bootstrap
    {
        /// <summary>
        /// 启动 server
        /// </summary>
        /// <param name="args"></param> 
        /// <param name="diAction"></param>
        /// <param name="iocType">依赖注入类型</param>
        public static void StartUp(string[] args, Action diAction, Action startUpCallBack = null, Loader.IocType iocType = Loader.IocType.Autofac)
        {
            var reStar = false;
        reStart:
            try
            {
                Enter(args, diAction, reStar, iocType);
                startUpCallBack?.Invoke();
                AppDomain.CurrentDomain.ProcessExit += (s, e) =>
                {
                    if (Server.State)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine(
                            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {Const.SettingService.AppName} Service is being stopped·····");
                        Server.Stop();
                        Console.WriteLine(
                            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {Const.SettingService.AppName} The service has stopped!");
                        Console.ResetColor();
                    }
                };
                //阻止daemon进程退出
                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                Log.Log.Error(e);
                if (e is Grpc.Core.RpcException)
                {
                    reStar = true;
                }
                else
                {
                    throw;
                }
            }
            //服务因为传输协议异常自动退出，需要重启启动。如果是配置错误弹出配置错误信息
            goto reStart;
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="args"></param>
        /// <param name="diAction"></param>
        /// <param name="reStart">异常之中回复启动，true。正常启动，false</param>
        static void Enter(string[] args, Action diAction, bool reStart, Loader.IocType iocType)
        {
            if (!reStart)
            {
                EngineData.AnnoBootstrap.Bootstrap(diAction, iocType);
            }
            #region 设置监听端口（可以通过参数 设置。没有取配置文件）

            int.TryParse(ArgsValue.GetValueByName("-p", args), out int port);
            if (port > 0)
            {
                Const.SettingService.Local.Port = port;
            }
            long.TryParse(ArgsValue.GetValueByName("-t", args), out long timeout);
            if (timeout > 0)
            {
                Const.SettingService.TimeOut = timeout;
            }
            int.TryParse(ArgsValue.GetValueByName("-w", args), out int weight);
            if (weight > 0)
            {
                Const.SettingService.Weight = weight;
            }

            var host = ArgsValue.GetValueByName("-h", args);
            if (host != null)
            {
                System.Net.IPAddress.TryParse(host, out System.Net.IPAddress ipAddress);
                if (ipAddress != null)
                {
                    Const.SettingService.Local.IpAddress = host;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("-h 参数错误!");
                    Console.ResetColor();
                }
            }
            var traceOnOffStr = ArgsValue.GetValueByName("-tr", args);
            if (!string.IsNullOrWhiteSpace(traceOnOffStr))
            {
                bool.TryParse(traceOnOffStr, out bool traceOnOff);
                Const.SettingService.TraceOnOff = traceOnOff;
            }
            #endregion

            Server.Start();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"节点【{Const.SettingService.AppName}】(端口：{Const.SettingService.Local.Port})已启动");
            foreach (var f in Anno.Const.SettingService.FuncName.Split(','))
            {
                Console.WriteLine($"{f}");
            }
            Console.WriteLine($"{"权重:" + Anno.Const.SettingService.Weight}");
            Console.ResetColor();
            Console.WriteLine($"----------------------------------------------------------------- ");
            Const.SettingService.Ts.ForEach(t => { new Client.Register().ToCenter(t, 60); });
            /*
             * 1、 Const.SettingService.Local 在AnnoService(服务提供方)中 作为 本机信息
             * 2、Const.SettingService.Local 在客户端 作为 负载均衡地址信息(AnnoCenter)
             * 3、此处为了让AnnoService 可以调用外部服务 Local使用完毕后更改为 AnnoCenter 地址
             */
            Const.Target target = Const.SettingService.Ts?.First();
            Client.DefaultConfigManager.SetDefaultConfiguration(Const.SettingService.AppName, target.IpAddress, target.Port, Const.SettingService.TraceOnOff);
        }

        /// <summary>
        ///服务启动后将服务Api文档写入注册中心
        ///
        ///增加自己的服务的时候只用复制下面的代码就可以不用做修改
        /// </summary>
        public static void ApiDoc()
        {
            List<AnnoData> routings = new List<AnnoData>();
            foreach (var item in Anno.EngineData.Routing.Routing.Router)
            {
                if (item.Value.RoutMethod.Name == "get_ActionResult")
                {
                    continue;
                }
                var parameters = item.Value.RoutMethod.GetParameters().ToList().Select(it =>
                {
                    var parameter = new ParametersValue
                    { Name = it.Name, Position = it.Position, ParameterType = it.ParameterType.FullName };
                    var pa = it.GetCustomAttributes<AnnoInfoAttribute>().ToList();
                    if (pa.Any())
                    {
                        parameter.Desc = pa.First().Desc;
                    }
                    return parameter;
                }).ToList();
                string methodDesc = String.Empty;
                var mAnnoInfoAttributes = item.Value.RoutMethod.GetCustomAttributes<AnnoInfoAttribute>().ToList();
                if (mAnnoInfoAttributes.Count > 0)
                {
                    methodDesc = mAnnoInfoAttributes.First().Desc;
                }
                routings.Add(new AnnoData()
                {
                    App = Anno.Const.SettingService.AppName,
                    Id = $"{Anno.Const.SettingService.AppName}:{item.Key}",
                    Value = Newtonsoft.Json.JsonConvert.SerializeObject(new DataValue { Desc = methodDesc, Name = item.Value.RoutMethod.Name, Parameters = parameters })
                });
            }
            Dictionary<string, string> input = new Dictionary<string, string>();
            input[StorageCommand.COMMAND] = StorageCommand.APIDOCCOMMAND;
            input.Add(CONST.Opt, CONST.DeleteByApp);
            input.Add(CONST.App, Anno.Const.SettingService.AppName);
            var del = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoDataResult>(StorageEngine.Invoke(input));
            if (del.Status == false)
            {
                Log.Log.Error(del);
            }
            input.Clear();
            input[StorageCommand.COMMAND] = StorageCommand.APIDOCCOMMAND;
            input.Add(CONST.Opt, CONST.UpsertBatch);
            input.Add(CONST.Data, Newtonsoft.Json.JsonConvert.SerializeObject(routings));
            var rlt = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoDataResult>(StorageEngine.Invoke(input));
            if (rlt.Status == false)
            {
                Log.Log.Error(rlt);
            }
        }
    }
}
