using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Anno.Rpc;
using Grpc.Core;

namespace Anno.Rpc.Center
{
    /// <summary>
    /// 接受消息
    /// </summary>
    public static class Monitor
    {
        private static Grpc.Core.Server _server;
        public static bool State { get;private set; } = false;
        public static void Start()
        {
            ThriftConfig tc = ThriftConfig.CreateInstance();
            //TServerSocket serverTransport = new TServerSocket(tc.Port, 0, true);
            _server = new Grpc.Core.Server
            {
                Services = { BrokerCenter.BindService(new BusinessImpl()) },
                Ports = { new ServerPort("0.0.0.0", tc.Port, ServerCredentials.Insecure) }
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
