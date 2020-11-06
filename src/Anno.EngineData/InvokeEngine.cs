using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Anno.EngineData;


namespace Anno.EngineData
{
    using Anno.Const.Enum;
    public abstract class InvokeEngine
    {
        private static readonly IRpcConnector rpcConnector = Loader.IocLoader.Resolve<IRpcConnector>();
        /// <summary>
        /// 集群--全局调用处理器
        /// </summary>
        /// <param name="channel">管道Anno.Logic（Anno.Plugs.LogicService）</param>
        /// <param name="router">路由Platform(PlatformModule)</param>
        /// <param name="method">方法ChangePwd</param>
        /// <param name="response">发送内容</param>
        /// <returns></returns>
        public static string InvokeProcessor(string channel, string router, string method, Dictionary<string, string> response)
        {
            if (string.IsNullOrWhiteSpace(channel) || string.IsNullOrWhiteSpace(router) ||
                string.IsNullOrWhiteSpace(method))
            {
                throw new AnnoInvokeEngineException(AnnoInvokeEngineException.AnnoInvokeEngineExceptionType.Argument,"【管道、路由、方法】三个参数为必输参数！");
            }
            if (response.ContainsKey(Eng.NAMESPACE))
            {
                response[Eng.NAMESPACE] = channel;
            }
            else
            {
                response.Add(Eng.NAMESPACE, channel);
            }
            if (response.ContainsKey(Eng.CLASS))
            {
                response[Eng.CLASS] = router;
            }
            else
            {
                response.Add(Eng.CLASS, router);
            }
            if (response.ContainsKey(Eng.METHOD))
            {
                response[Eng.METHOD] = method;
            }
            else
            {
                response.Add(Eng.METHOD, method);
            }
            return rpcConnector.BrokerDns(response);
        }
        /// <summary>
        /// 集群--全局调用处理器---异步
        /// </summary>
        /// <param name="channel">管道Anno.Logic（Anno.Plugs.LogicService）</param>
        /// <param name="router">路由Platform(PlatformModule)</param>
        /// <param name="method">方法ChangePwd</param>
        /// <param name="response">发送内容</param>
        /// <returns></returns>
        public static async Task<string> InvokeProcessorAsync(string channel, string router, string method,
            Dictionary<string, string> response)
        {
            return await Task.Run(() => InvokeProcessor(channel, router, method, response));
        }
    }
}
