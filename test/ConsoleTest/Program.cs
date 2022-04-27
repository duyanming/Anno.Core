using System;
using BenchmarkDotNet.Running;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // new TimeOutDemo().Handle();
            //new CircuitBreakerDemo().Handle();
            //var x = Newtonsoft.Json.JsonConvert.SerializeObject(1);
            //var x1 = Newtonsoft.Json.JsonConvert.SerializeObject("sdf");
            //var x2 = Newtonsoft.Json.JsonConvert.SerializeObject(true);
            //var x3 = Newtonsoft.Json.JsonConvert.SerializeObject(1.3);
            //new CronNetTest().Handle();
            //new RpcStorage().Handle();//RPCRpcStorage
            new RpcTest().Handle8();//RPC客户端测试
            //new ExpressionAnalysisTest().Handle();
            //new LogTest().Handle();
            //new GrpcTest().Handle();
            //new AttributeVerificationTest().Handle();//属性校验测试
            //new UseSysInfoWatchTest().Handle();//程序使用系统资源监控
            //new DLockTest().Handle();//分布式锁

            //new RpcTest().HandleLinkNum();//HandleLinkNum 测试打开多少个链接
            //new RabbitMqTest().Handle();//MQ客户端测试

            //new RateLimitTest().Handle();

            //BenchmarkRunner.Run<MappDemo>();

            //BenchmarkRunner.Run<BenchmarkDotNetRpc>();
            Console.WriteLine("测试结束---------------------End");
            Console.ReadLine();
        }
    }
}
