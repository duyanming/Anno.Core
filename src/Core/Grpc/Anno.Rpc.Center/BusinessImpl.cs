using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anno.Rpc;
using Anno.Rpc.Adapter;
using Anno.Rpc.Storage;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Anno.Rpc.Center
{
    public class BusinessImpl : BrokerCenter.BrokerCenterBase
    {
        private static ConcurrentDictionary<string, BaseAdapter> _adapters = new ConcurrentDictionary<string, BaseAdapter>();
        public BusinessImpl()
        {
            _adapters.TryAdd(StorageCommand.KVCOMMAND, new KvStorageAdapter());
            _adapters.TryAdd(StorageCommand.APIDOCCOMMAND, new ApiDocStorageAdapter());
            _adapters.TryAdd(StorageCommand.ANNOMICROSERVICE, new AnnoMicroManagementStorageAdapter());
        }
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

                BaseAdapter adapter;
                string rlt;
                try
                {
                    if (input.ContainsKey(StorageCommand.COMMAND) && _adapters.TryGetValue(input[StorageCommand.COMMAND], out adapter))
                    {
                        rlt = adapter.Invoke(input);
                    }
                    else
                    {
                        rlt = FailMessage("未知指令,参考[Anno.Rpc.Storage.StorageCommand]下指令。");
                    }
                }
                catch (Exception ex)
                {
                    rlt = FailMessage(ex.Message);
                }
                reply.Reply = rlt;
                return reply;
            });
        }
        /// <summary>
        /// 构建错误消息Json字符串
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="status">默认False</param>
        /// <returns>"{\"Msg\":\""+message+"\",\"Status\":false,\"Output\":null,\"OutputData\":null}"</returns>
        internal static string FailMessage(string message, bool status = false)
        {
            return "{\"Msg\":\"" + message + "\",\"Status\":" + status.ToString().ToLower() + ",\"Data\":null}";
        }
    }
}
