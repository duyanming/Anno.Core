using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Anno.Rpc.Client;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Validators;
using System.Linq;
using BenchmarkDotNet.Jobs;

namespace ConsoleTest
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    //[SimpleJob(RuntimeMoniker.Net461)]
    [Config(typeof(AllowNonOptimized))]
    //[AllStatisticsColumn]
    //[MemoryDiagnoser]
    [MaxColumn, MinColumn, MemoryDiagnoser]
    public class BenchmarkDotNetRpc
    {

        public BenchmarkDotNetRpc()
        {
            DefaultConfigManager.SetDefaultConnectionPool(1000, Environment.ProcessorCount * 2, 100);
            DefaultConfigManager.SetDefaultConfiguration("RpcTest", "127.0.0.1", 6660, false);

            Dictionary<string, string> input = new Dictionary<string, string>();
            input.Add("channel", "Anno.Plugs.HelloWorld");
            input.Add("router", "HelloWorldViper");
            input.Add("method", "Test0");

            var x = Connector.BrokerDns(input);
            Console.WriteLine(x);

        }
        [Benchmark]
        public void RpcCall()
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            input.Add("channel", "Anno.Plugs.HelloWorld");
            input.Add("router", "HelloWorldViper");
            input.Add("method", "Test0");

            var x = Connector.BrokerDns(input);
            if (x.IndexOf("true") <= 0)
            {
                Console.WriteLine(x);
            }
        }
    }

    public class AllowNonOptimized : ManualConfig
    {
        public AllowNonOptimized()
        {
            IConfig config = DefaultConfig.Instance.WithOptions(ConfigOptions.DisableOptimizationsValidator);
            Add(config);
            //Add(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLS

            //Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
            //Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
            //Add(DefaultConfig.Instance.GetColumnProviders().ToArray()); // manual config has no columns by default
        }
    }
}
