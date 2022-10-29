using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Thrift.Transport;
using Thrift.Server;
using Anno.Rpc;

namespace Anno.Rpc.Center
{
    /// <summary>
    /// 接受消息
    /// </summary>
    public static class Monitor
    {
        static TServer _server;
        public static bool State { get;private set; } = false;
        public static void Start()
        {
            ThriftConfig tc = ThriftConfig.CreateInstance();
            TServerSocket serverTransport = new TServerSocket(tc.Port, 0, true);
            BrokerCenter.Processor processor = new BrokerCenter.Processor(new BusinessImpl());
            _server = new TThreadedServer(processor, serverTransport,Log.Log.Anno, 2000);
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
