/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/10/26 17:57:51 
Functional description： KvStorageEngine
******************************************************/
using Anno.Const;
using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;

namespace Anno.Rpc.Storage
{
    public class KvStorageEngine : IDisposable
    {
        private bool disposed;
        private Channel channel;
        private BrokerCenter.BrokerCenterClient client;
        public KvStorageEngine()
        {
            channel = new Channel($"{SettingService.Local.IpAddress}:{SettingService.Local.Port}", ChannelCredentials.Insecure);
            client = new BrokerCenter.BrokerCenterClient(channel);
        }
        ~KvStorageEngine()
        {
            Dispose(false);
        }
        public string Get(string key)
        {
            return Get<string>(key);
        }
        public T Get<T>(string key)
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            input["KV"] = "KV";
            input[KVCONST.Opt] = KVCONST.FindById;

            BrokerRequest request = new BrokerRequest();
            request.Input.Add(input);
            var rltStr = client.Invoke(request);
            var rlt = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoDataResult<T>>(rltStr.Reply);
            return rlt.Data;
        }
        public bool Set(string key, string value)
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            input[KVCONST.Opt] = KVCONST.InsertOne;
            input[KVCONST.Data] = Newtonsoft.Json.JsonConvert.SerializeObject(new AnnoKV(key, value));

            BrokerRequest request = new BrokerRequest();
            request.Input.Add(input);
            var rltStr = client.Invoke(request);

            var rlt = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoDataResult<AnnoKV>>(rltStr.Reply);
            return rlt.Status;
        }
        public bool Set<T>(string key, T value)
        {
            string data = string.Empty;
            try
            {
                data = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            }
            catch
            {
                data = value.ToString();
            }
            return Set(key, data);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    try
                    {
                        channel.ShutdownAsync();
                    }
                    catch (Exception)
                    {
                        //var x = ex;
                    }
                }
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
