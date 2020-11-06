using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EngineData
{
    public interface IRpcConnector
    {
        string BrokerDns(Dictionary<string, string> input);
        Task<string> BrokerDnsAsync(Dictionary<string, string> input, string nickName);
        Task<string> BrokerDnsAsync(Dictionary<string, string> input);
        void SetDefaultConfiguration(string appName, string centerAddress, int port = 6660, bool traceOnOff = true);
        /// <summary>
        /// 设置连接池信息
        /// </summary>
        /// <param name="maxActive"></param>
        /// <param name="minIdle"></param>
        /// <param name="maxIdle"></param>
        void SetDefaultConnectionPool(int maxActive, int minIdle, int maxIdle);
    }
}
