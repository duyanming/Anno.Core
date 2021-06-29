using Thrift.Transport;
using Thrift.Server;
using System.Threading;

namespace Anno.Rpc.Server
{
    using Anno.Log;
    public static class Server
    {
        private static  TServer _server;
        public static bool State { get; private set; } = false;
        public static void Start()
        {
            OutputLogo();
            TServerSocket serverTransport = new TServerSocket(Const.SettingService.Local.Port, 0, true);
            BrokerService.Processor processor = new BrokerService.Processor(new BusinessImpl());
            int maxThreads = Const.SettingService.MaxThreads;
            if (maxThreads <= 0)
            {
                maxThreads = 500;
            }
            _server = new TThreadedServer(processor, serverTransport,Log.ConsoleWriteLine, maxThreads);
            new Thread(_server.Serve) { IsBackground = true }.Start();//开启业务服务
            State = true;
        }
        public static bool Stop()
        {
            _server.Stop();
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
                                            anno&viper  thrift service 
";
            logo += " -----------------------------------------------------------------------------\r\n";
            logo += $" Server Port      {Const.SettingService.Local.Port} \r\n";
            logo += $" Author           YanMing.Du \r\n";
            logo += $" Version          [{ typeof(Client.Connector).Assembly.GetName().Version}]\r\n";
            logo += $" Repository       https://github.com/duyanming/anno.core \r\n";
            logo += " -----------------------------------------------------------------------------\r\n";
            Log.WriteLineNoDate(logo);
        }
    }
}
