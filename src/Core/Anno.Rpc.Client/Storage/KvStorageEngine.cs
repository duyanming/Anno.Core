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
using Thrift.Protocol;
using Thrift.Transport;

namespace Anno.Rpc.Storage
{
    public class KvStorageEngine : IDisposable
    {
        private bool disposed;
        private BrokerCenter.Client client;
        private TTransport transport;
        public KvStorageEngine()
        {
            transport = new TSocket(SettingService.Local.IpAddress, SettingService.Local.Port, 30000);
            TProtocol protocol = new TBinaryProtocol(transport);
            client = new BrokerCenter.Client(protocol);
            transport.Open();
        }
        ~KvStorageEngine()
        {
            Dispose(false);
        }
        private AnnoKV GetAnnoKV(string key)
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            input["KV"] = "KV";
            input[KVCONST.Opt] = KVCONST.FindById;
            input[KVCONST.Id] = key;
            var rltStr = client.Invoke(input);
            var rlt = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoDataResult<AnnoKV>>(rltStr);
            if (rlt.Status == false)
            {
                throw new InvalidOperationException(rlt.Msg);
            }
            return rlt.Data;
        }
        public string Get(string key)
        {
            return GetAnnoKV(key).Value;
        }
        public T Get<T>(string key)where T:class,new()
        {
            var rlt = GetAnnoKV(key);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(rlt.Value);
        }
        public bool Set(string key, string value)
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            input["KV"] = "KV";
            input[KVCONST.Opt] = KVCONST.Upsert;
            input[KVCONST.Data] = Newtonsoft.Json.JsonConvert.SerializeObject(new AnnoKV(key, value));
            var rltStr = client.Invoke(input);
            var rlt = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoDataResult>(rltStr);
            if (rlt.Status == false)
            {
                throw new InvalidOperationException(rlt.Msg);
            }
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
                        transport.Close();
                        transport.Dispose();
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
