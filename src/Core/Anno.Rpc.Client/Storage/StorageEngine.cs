using Anno.Const;
using System;
using System.Collections.Generic;
using System.Text;
using Thrift.Protocol;
using Thrift.Transport;

namespace Anno.Rpc.Storage
{
    public static class StorageEngine
    {
        public static string Invoke(Dictionary<string, string> input)
        {
            TTransport transport = new TSocket(SettingService.Local.IpAddress, SettingService.Local.Port, 30000);
            TProtocol protocol = new TBinaryProtocol(transport);
            BrokerCenter.Client client = new BrokerCenter.Client(protocol);
            transport.Open();
            var rlt = client.Invoke(input);
            transport.Close();
            transport.Dispose();
            return rlt;
        }
    }
}
