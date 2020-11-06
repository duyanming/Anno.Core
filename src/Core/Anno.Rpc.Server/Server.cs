
using Thrift.Transport;
using Thrift.Server;
using System.Threading;
namespace Anno.Rpc.Server
{
    public static class Server
    {
        private static  TServer _server;
        public static bool State { get; private set; } = false;
        public static void Start()
        {
            TServerSocket serverTransport = new TServerSocket(Anno.Const.SettingService.Local.Port, 0, true);
            BrokerService.Processor processor = new BrokerService.Processor(new BusinessImpl());
            int maxThreads = Const.SettingService.MaxThreads;
            if (maxThreads <= 0)
            {
                maxThreads = 200;
            }
            _server = new TThreadedServer(processor, serverTransport,Log.Log.ConsoleWriteLine, maxThreads);
            new Thread(_server.Serve) { IsBackground = true }.Start();//开启业务服务
            State = true;
        }
        public static bool Stop()
        {
            _server.Stop();
            State = false;
            return true;
        }
    }
}
