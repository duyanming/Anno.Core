using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Anno.EngineData;

namespace Anno.Rpc.Server
{
    public class RpcConnectorImpl : IRpcConnector
    {
        public string BrokerDns(Dictionary<string, string> input)
        {
            return Client.Connector.BrokerDns(input);
        }

        public Task<string> BrokerDnsAsync(Dictionary<string, string> input)
        {
            return Client.Connector.BrokerDnsAsync(input);
        }

        public Task<string> BrokerDnsAsync(Dictionary<string, string> input, string nickName)
        {
            return Client.Connector.BrokerDnsAsync(input, nickName);
        }

        public void SetDefaultConfiguration(string appName, string centerAddress, int port = 6660, bool traceOnOff = true)
        {
            Client.DefaultConfigManager.SetDefaultConfiguration(appName,centerAddress,port,traceOnOff);
        }

        public void SetDefaultConnectionPool(int maxActive, int minIdle, int maxIdle)
        {
            Client.DefaultConfigManager.SetDefaultConnectionPool(maxActive, minIdle, maxIdle);
        }
    }
}
