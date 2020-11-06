using Anno.Const;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Storage
{
    public static class StorageEngine
    {
        public static string Invoke(Dictionary<string, string> input)
        {
            Channel channel = new Channel($"{SettingService.Local.IpAddress}:{SettingService.Local.Port}", ChannelCredentials.Insecure);
            BrokerCenter.BrokerCenterClient client = new BrokerCenter.BrokerCenterClient(channel);
            BrokerRequest request = new BrokerRequest();
            request.Input.Add(input);
            var rlt = client.Invoke(request);
            channel.ShutdownAsync();
            return rlt.Reply;
        }
    }
}
