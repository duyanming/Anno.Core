using Anno.Const;
using Anno.Rpc;
using Anno.Rpc.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    public class RpcStorage
    {
        public void Handle()
        {
            Init();
            using (Anno.Rpc.Storage.KvStorageEngine kvEngine = new Anno.Rpc.Storage.KvStorageEngine())
            {
                var rlt = kvEngine.Set("viper", "Viper 你好啊！");
                var getViper = kvEngine.Get("viper");
                var rltobj = kvEngine.Set("12", new ViperTest() { Id = 12, Name = "Viper" });
                var getobj = kvEngine.Get<ViperTest>("12");
            }
        }
        void Init()
        {
            //DefaultConfigManager.SetDefaultConnectionPool(new ConnectionPoolConfiguration() {
            //    MaxActive=1000,
            //    MaxIdle=100,
            //    MinIdle=50
            //});
            DefaultConfigManager.SetDefaultConfiguration("RpcTest", "127.0.0.1", 6660, false);
        }

    }
    public class ViperTest
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
