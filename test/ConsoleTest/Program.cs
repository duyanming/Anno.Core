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

            //new CronNetTest().Handle();
            //new RpcStorage().Handle();//RPCRpcStorage
            //new RpcTest().Handle2();//RPC客户端测试
            //new LogTest().Handle();
            //new GrpcTest().Handle();
            //new AttributeVerificationTest().Handle();//属性校验测试
            //new UseSysInfoWatchTest().Handle();//程序使用系统资源监控
            new DLockTest().Handle();//分布式锁

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
