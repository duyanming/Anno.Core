
using System.Threading;
using Grpc.Core;

namespace Anno.Rpc.Server
{
    public static class Server
    {
        private static Grpc.Core.Server _server;
        public static bool State { get; private set; } = false;
        public static void Start()
        {
            OutputLogo();
            _server = new Grpc.Core.Server
            {
                Services = { BrokerService.BindService(new BusinessImpl()) },
                Ports = { new ServerPort("0.0.0.0", Const.SettingService.Local.Port, ServerCredentials.Insecure) }
            };
            new Thread(_server.Start) { IsBackground = true }.Start();//开启业务服务
            State = true;
        }
        public static bool Stop()
        {
            _server.ShutdownAsync().Wait();
            State = false;
            return true;
        }
        private static void OutputLogo()
        {
            var logo = "\r\n";
            logo += " -----------------------------------------------------------------------------\r\n";
            logo +=
@"                                                _                    
     /\                           ___          (_)                   
    /  \    _ __   _ __    ___   ( _ )  __   __ _  _ __    ___  _ __ 
   / /\ \  | '_ \ | '_ \  / _ \  / _ \/\\ \ / /| || '_ \  / _ \| '__|
  / ____ \ | | | || | | || (_) || (_>  < \ V / | || |_) ||  __/| |   
 /_/    \_\|_| |_||_| |_| \___/  \___/\/  \_/  |_|| .__/  \___||_|   
                                                  | |                
                                                  |_|                
                                            anno&viper  grpc service
";
            logo += " -----------------------------------------------------------------------------\r\n";
            logo += $" Server Port      {Const.SettingService.Local.Port} \r\n";
            logo += $" Author           YanMing.Du \r\n";
            logo += $" Version          [{ typeof(Client.Connector).Assembly.GetName().Version}]\r\n";
            logo += $" Repository       https://github.com/duyanming/anno.core \r\n";
            logo += " -----------------------------------------------------------------------------\r\n";
            System.Console.WriteLine(logo);
        }
    }
}
