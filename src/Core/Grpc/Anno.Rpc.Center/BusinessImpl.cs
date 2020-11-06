using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anno.Rpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Anno.Rpc.Center
{
    public class BusinessImpl : BrokerCenter.BrokerCenterBase
    {
        public override Task<BrokerReply> Add_broker(Micro request, ServerCallContext context)
        {
            BrokerReply reply = new BrokerReply();
            ThriftConfig tc = ThriftConfig.CreateInstance();
            tc.Add(request);
            reply.Reply = "1";
            return Task.FromResult(reply);
        }

        public override Task<GetMicroReply> GetMicro(GetMicroRequest request, ServerCallContext context)
        {
            var reply = new GetMicroReply();
            reply.Micros.AddRange(Distribute.GetMicro(request.Request));
            return Task.FromResult(reply);
        }
        public override Task<PingReply> Ping(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new PingReply() { Reply = true });
        }
        public override Task<BrokerReply> Invoke(BrokerRequest request, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                BrokerReply reply = new BrokerReply();
                var input = new Dictionary<string, string>(request.Input);

                try
                {
                    if (input.ContainsKey("KV"))
                    {

                        reply.Reply = new Storage.KvStorage().Invoke(input);
                    }
                    else
                    {
                        reply.Reply = new Storage.AnnoStorage().Invoke(input);
                    }
                }
                catch (Exception ex)
                {
                    reply.Reply = Newtonsoft.Json.JsonConvert.SerializeObject(new Storage.AnnoDataResult() { Status = false, Msg = ex.Message });
                }
                return reply;
            });
        }
    }
}
