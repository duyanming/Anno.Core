using Anno.Const;
using Anno.Rpc;
using Anno.Rpc.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public class RpcStorage
    {
        public void Handle()
        {
            Init();

        To:
            Console.Write("请输入调用次数：");
            long.TryParse(Console.ReadLine(), out long num);

            Stopwatch sw = Stopwatch.StartNew();
            Parallel.For(0, num, i =>
            {
                using (Anno.Rpc.Storage.KvStorageEngine kvEngine = new Anno.Rpc.Storage.KvStorageEngine())
                {
                    var rlt = kvEngine.Set("viper", "Viper 你好啊！");
                    var getViper = kvEngine.Get("viper");
                    var rltobj = kvEngine.Set("12", new ViperTest() { Id = 12, Name = "Viper" });
                    var getobj = kvEngine.Get<ViperTest>("12");
                }
            });
            long ElapsedMilliseconds = sw.ElapsedMilliseconds;
            if (ElapsedMilliseconds == 0)
            {
                ElapsedMilliseconds = 1;
            }
            Console.WriteLine($"运行时间：{sw.ElapsedMilliseconds}/ms,TPS:{(num) * 1000 / ElapsedMilliseconds}");
            sw.Stop();
            goto To;

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
