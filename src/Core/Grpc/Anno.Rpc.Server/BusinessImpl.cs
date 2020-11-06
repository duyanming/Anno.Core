using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Anno.EngineData;
using Anno.Rpc;
using Grpc.Core;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;

namespace Anno.Rpc.Server
{
    public class BusinessImpl : BrokerService.BrokerServiceBase
    {
        public override Task<BrokerReply> broker(BrokerRequest request, ServerCallContext context)
        {
           return Task.Run(()=> {
               BrokerReply reply = new BrokerReply();
               ActionResult actionResult = null;
               try
               {
                   Dictionary<string, string> input = new Dictionary<string, string>(request.Input);
                   actionResult = Engine.Transmit(input);
               }
               catch (Exception ex)
               { //记录异常日志
                   actionResult = new ActionResult
                   {
                       Msg = ex.InnerException.Message
                   };
               }
               reply.Reply= JsonConvert.SerializeObject(actionResult);
               return reply;
           });
        }
        public override Task<PingReply> Ping(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new PingReply() { Reply=true});
        }
    }
}
