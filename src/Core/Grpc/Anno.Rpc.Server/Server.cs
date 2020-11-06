
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
            _server = new Grpc.Core.Server
            {
                Services = { BrokerService.BindService(new BusinessImpl()) },
                Ports = { new ServerPort("0.0.0.0" ,Const.SettingService.Local.Port, ServerCredentials.Insecure) }
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
    }
}
