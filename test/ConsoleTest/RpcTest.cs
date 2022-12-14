using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Anno.Const;
//using Thrift.Transport;

namespace ConsoleTest
{
    using Anno.Rpc.Client;
    using Anno.Rpc.Storage;

    public class RpcTest
    {
        public void Handle()
        {
            Init();
        To:
            Console.Write("请输入调用次数：");
            long.TryParse(Console.ReadLine(), out long num);
            List<Task> ts = new List<Task>();

          

            Stopwatch sw = Stopwatch.StartNew();
            Parallel.For(0, num, i =>
            {
                Dictionary<string, string> input = new Dictionary<string, string>();

                input.Add("channel", "anno.plugs.HelloWorld");
                input.Add("router", "HelloWorldViper");
                input.Add("method", "Test0");

                //input.Add("channel", "Anno.Plugs.Viper");
                //input.Add("router", "Exam");
                //input.Add("method", "SayHi");
                //input.Add("name", "anno");
                var x = Connector.BrokerDns(input);
                //Console.WriteLine(x);
                if (x.IndexOf("true") <= 0)
                {
                    Console.WriteLine(x);
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
        public void Handle2()
        {
            Console.Write("请输入类型 1：Handle, 4：Handle4, 5：Handle5Parallel,其他 Handle2：");
            string type = Console.ReadLine();
            if (type.Equals("1"))
            {
                Handle();
                return;
            }
            else if (type.Equals("4"))
            {
                Console.Write("请输入类型为：4!");
                Handle4();
                return;
            }
            else if (type.Equals("5"))
            {
                Console.Write("请输入类型为：5!");
                Handle5();
                return;
            }
            else if (type.Equals("6"))
            {
                Console.Write("请输入类型为：6!");
                Handle6();
                return;
            }
            Init();
        To:
            Console.Write("请输入线程数：");
            long.TryParse(Console.ReadLine(), out long th);
            Console.Write("请输入每个线程数调用次数：");
            long.TryParse(Console.ReadLine(), out long num);
            List<Task> ts = new List<Task>();

            Stopwatch sw = Stopwatch.StartNew();
            int total = 0;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < th; i++)
            {

                var t = Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < num; j++)
                    {
                        //Dictionary<string, string> input = new Dictionary<string, string>();                        

                        //input.Add("channel", "anno.component");
                        //input.Add("router", "UserInfo");
                        //input.Add("method", "HelloWorld");
                        //input.Add("name", Anno.Const.SettingService.AppName);

                        Dictionary<string, string> input = new Dictionary<string, string>();

                        input.Add("channel", "Anno.Plugs.HelloWorld");
                        input.Add("router", "HelloWorldViper");
                        input.Add("method", "Test0");

                        //input.Add("channel", "Anno.Plugs.Viper");
                        //input.Add("router", "Exam");
                        //input.Add("method", "SayHi");
                        //input.Add("name", "test_name");

                        //调用异步方法
                        //input.Add("channel", "Anno.Plugs.HelloWorld");
                        //input.Add("router", "HelloWorldTask");
                        //input.Add("method", "SayHello");
                        //input.Add("name", "Jack");
                        //input.Add("age", "12");

                        //input.Add("channel", "Anno.Plugs.Viper");
                        //input.Add("router", "Exam");
                        //input.Add("method", "SayHi");
                        //input.Add("name", "anno");

                        var x = Connector.BrokerDns(input);
                        if (x.IndexOf("true") <= 0)
                        {
                            Console.WriteLine(x);
                        }
                        //调用异步方法Async
                        //input.Clear();
                        //input.Add("channel", "Anno.Plugs.HelloWorld");
                        //input.Add("router", "HelloWorldTask");
                        //input.Add("method", "SayHelloAsync");
                        //input.Add("name", "Jack");
                        //input.Add("age", "12");
                        //x = Connector.BrokerDns(input);
                        //Console.WriteLine(x);
                        //tasks.Add(t1);

                        //if (x.IndexOf("true") <= 0)
                        //{
                        //    Console.WriteLine(x);
                        //}
                        //Interlocked.Increment(ref total);
                        //Console.WriteLine(x);
                    }
                });
                ts.Add(t);
            }
            Console.WriteLine($"-----------------------1----------------------");
            Task.WaitAll(ts.ToArray());
            Console.WriteLine($"-----------------------2----------------------");
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"-----------------------3----------------------");
            long ElapsedMilliseconds = sw.ElapsedMilliseconds;
            if (ElapsedMilliseconds == 0)
            {
                ElapsedMilliseconds = 1;
            }
            Console.WriteLine($"运行时间：{sw.ElapsedMilliseconds}/ms,TPS:{(num * th) * 1000 / ElapsedMilliseconds}");
            sw.Stop();
            goto To;
        }

        public void Handle1()
        {
            List<Task> ts = new List<Task>();
            int[] x = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int i = 0; i < 10; i++)
            {
                var i1 = i;
                var t = Task.Factory.StartNew(() =>
                {
                    Task.Delay(1000).Wait();
                    Console.WriteLine(x[i1]);
                });
                ts.Add(t);
            }

            Task.WaitAll(ts.ToArray());
        }

        public void Handle3()
        {
            Init();
            Dictionary<string, string> input = new Dictionary<string, string>();
            var productsStr = Newtonsoft.Json.JsonConvert.SerializeObject(
                new List<HelloWorldDto.ProductDto>() {
            new HelloWorldDto.ProductDto() {
                CountryOfOrigin="sdf",
                Number=3,
                Name="x",
                Price=9
            }
            });
            input.Add("channel", "Anno.Plugs.HelloWorld");
            input.Add("router", "HelloWorldViper");
            input.Add("method", "AddProducts");
            input.Add("products", productsStr);
            var x = Connector.BrokerDns(input);
        }

        public void HandleLinkNum()
        {
            //To:
            //    int total = 0;
            //    List<TTransport> tt = new List<TTransport>();
            //    Console.Write("请输入线程数：");
            //    long.TryParse(Console.ReadLine(), out long th);

            //    Dictionary<string, string> input = new Dictionary<string, string>();
            //input.Add("channel", "Anno.Plugs.HelloWorld");
            //input.Add("router", "HelloWorldViper");
            //input.Add("method", "Test");
            //    try
            //    {
            //        //    TTransport transport = new TSocket("192.168.1.2", 6659, timeout: 3000);
            //        //    transport.Open();
            //        for (int i = 0; i < th; i++)
            //        {
            //            TTransport transport = new TSocket("192.168.1.2", 6659, timeout: 3000);
            //            transport.Open();
            //            //TProtocol protocol = new TBinaryProtocol(transport);
            //            //var client = new BrokerService.Client(protocol);
            //            //var r= client.broker(input);
            //            ////tt.Add(transport);
            //            //Console.WriteLine(r);
            //            Interlocked.Increment(ref total);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //    }
            //    Console.WriteLine(total);
            //    goto To;
        }

        public void Handle4()
        {
            Init();
        To:
            Console.Write("请输入调用次数：");
            long.TryParse(Console.ReadLine(), out long num);

            for (int i = 0; i < num; i++)
            {
                Dictionary<string, string> input = new Dictionary<string, string>();

                input.Add("channel", "Anno.Plugs.HelloWorld");
                input.Add("router", "HelloWorldViper");
                input.Add("method", "Test0");
                var x = Connector.BrokerDns(input);
                Anno.Log.Log.WriteLine(x);
                Task.Delay(500).Wait();
            }
            goto To;
        }

        public void Handle5()
        {
            Init();
        To:
            Console.Write("请输入调用次数：");
            long.TryParse(Console.ReadLine(), out long num);

            Parallel.For(0, num,new ParallelOptions() { MaxDegreeOfParallelism=4}, i =>
            {
                Dictionary<string, string> input = new Dictionary<string, string>();

                input.Add("channel", "Anno.Plugs.HelloWorld");
                input.Add("router", "HelloWorldViper");
                input.Add("method", "Test0");
                var x = Connector.BrokerDns(input);
                Anno.Log.Log.WriteLine(x);
                Task.Delay(200).Wait();
            });
            goto To;
        }

        public void Handle6()
        {
            Init();
        To:
            Console.Write("请输入调用次数：");
            long.TryParse(Console.ReadLine(), out long num);

            for (int i = 0; i < num; i++)
            {
                Dictionary<string, string> input = new Dictionary<string, string>();

                input.Add("channel", "Jky.Web.Core.ModelManage");
                input.Add("router", "SayHi");
                input.Add("method", "SayHi");
                input.Add("name", "Jky");
                var x = Connector.BrokerDns(input);
                Anno.Log.Log.WriteLine(x);
                Task.Delay(500).Wait();
            }
            goto To;
        }

        public void Handle7()
        {
            Init();
            Dictionary<string, string> input = new Dictionary<string, string>();
            input.Add("channel", "Anno.Plugs.HelloWorld");
            input.Add("router", "HelloWorldTask");
            input.Add("method", "TaskActionResult");
            var x = Connector.BrokerDns(input);
            Console.Write(x);   
        }
        public void Handle9()
        {
            Init();
            Parallel.For(0, 160, i => {
                Dictionary<string, string> input = new Dictionary<string, string>();
                input.Add("channel", "Anno.Plugs.HelloWorld");
                input.Add("router", "HelloWorldTask");
                input.Add("method", "TaskinputNullActionResult");
                input.Add("x", "1");
                var x = Connector.BrokerDns(input);
                Console.Write(x);
            });
        }
        void Init()
        {
            DefaultConfigManager.SetDefaultConnectionPool(100, Environment.ProcessorCount * 2, 50);
            DefaultConfigManager.SetDefaultConfiguration("RpcTest", "127.0.0.1", 6660, false);
        }

        /// <summary>
        /// 修改服务权重
        /// </summary>
        public void Handle8() {
            Init();
        To:
            Console.Write("请输入权重：");
            long.TryParse(Console.ReadLine(), out long weight);
            Dictionary<string, string> input = new Dictionary<string, string>();
            input[StorageCommand.COMMAND] = StorageCommand.ANNOMICROSERVICE;
            input["ip"] = "10.128.3.109";
            input["port"] = "6659";
            input["weight"] = weight.ToString();
            StorageEngine.Invoke(input);
            for (int i = 0; i < 10; i++)
            {
                Dictionary<string, string> inputx = new Dictionary<string, string>();
                inputx.Add("channel", "Anno.Plugs.HelloWorld");
                inputx.Add("router", "HelloWorldTask");
                inputx.Add("method", "TaskActionResult");
                var x = Connector.BrokerDns(inputx);
                Console.WriteLine(x);
                Task.Delay(1000).Wait();
            }
            goto To;
        }
    }

    public class Test
    {
        public string name { get; set; }
        public AutoResetEvent ResetEvent { get; set; }
    }

    public class ConcurrentStackT
    {
        public void Handle1()
        {
            var xx1 = new ConcurrentStack<Test>();
            xx1.Push(new Test() { name = "12" });
            xx1.Push(new Test() { name = "13" });
            var xx2 = xx1.ToArray();
            xx1.TryPop(out Test xx);
        }
        /// <summary>
        /// AutoResetEvent 测试 Set一次只能通过一个WaitOne
        /// </summary>
        public void Handle()
        {
            var xx1 = new ConcurrentStack<Test>();
            xx1.Push(new Test() { name = "12", ResetEvent = new AutoResetEvent(false) });
            xx1.Push(new Test() { name = "13", ResetEvent = new AutoResetEvent(false) });
            xx1.TryPop(out Test xx);
            List<Task> ts = new List<Task>();
            for (int i = 0; i < 10; i++)
            {

                var t = Task.Factory.StartNew(() =>
                {
                    Console.WriteLine($"WaitOne.........{Task.CurrentId}");
                    if (!xx.ResetEvent.WaitOne(4000))
                    {
                        Console.WriteLine($"WaitOne.........END 超时--{Task.CurrentId}");
                    }
                    else
                    {
                        Console.WriteLine($"WaitOne.........END--{Task.CurrentId}");
                    }
                }, TaskCreationOptions.None);
                ts.Add(t);
            }
            //Task.WaitAll(ts.ToArray());
            Console.WriteLine(" xx.ResetEvent.Set()");
            xx.ResetEvent.Set();
            Thread.Sleep(1000);
            xx.ResetEvent.Set();
            Thread.Sleep(1000);
            xx.ResetEvent.Set();
            Thread.Sleep(1000);
            xx.ResetEvent.Set();
        }
    }
}
