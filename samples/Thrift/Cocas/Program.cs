using System;
/*
 * 这是一个将注册中心和服务宿主合并到一个程序的示例
 * 注册中心配置文件为：Cocas.config
 * 服务配置文件为：Anno.config
 */
namespace Cocas
{
    using Cocas = Anno.Rpc.Center;
    using System.Threading.Tasks;


    using Anno.EngineData;
    using Anno.Rpc.Server;
    using System.Linq;
    using Anno.Loader;
    using Anno.Log;
    using Autofac;
    /// <summary>
    /// 这是一个将注册中心和服务宿主合并到一个程序的示例
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Cocas(Combine Center And Service) / 联合注册中心和服务 ";
            //注册中心启动
            AnnoCenter(args);
            AnnoService(args);
        }

        static async void AnnoCenter(string[] args)
        {
            await Task.Factory.StartNew(() =>
            {
                Cocas.ThriftConfig.AnnoFile = "Cocas.config";
                Cocas.Bootstrap.StartUp(args, (service, noticeType) =>
                {
                    //Console.WriteLine(noticeType.ToString() + ":" + Newtonsoft.Json.JsonConvert.SerializeObject(service));
                }, (newService, oldService) =>
                {
                    //Console.WriteLine("NewConfig:" + Newtonsoft.Json.JsonConvert.SerializeObject(newService));
                    //Console.WriteLine("OldConfig:" + Newtonsoft.Json.JsonConvert.SerializeObject(oldService));
                });
            });
        }
        static  void AnnoService(string[] args)
        {
            if (args.Contains("-help"))
            {
                Log.ConsoleWriteLine(@"
启动参数：
	-p 6659		设置启动端口
	-xt 200		设置服务最大线程数
	-t 20000		设置超时时间（单位毫秒）
	-w 1		设置权重
	-h 192.168.0.2	设置服务在注册中心的地址
	-tr false		设置调用链追踪是否启用");
                return;
            }
            /**
             * 启动默认DI库为 Autofac 可以切换为微软自带的DI库 DependencyInjection
             */
            Bootstrap.StartUp(args, () =>//服务配置文件读取完成后回调(服务未启动)
            {
                var autofac = IocLoader.GetAutoFacContainerBuilder();
                 /**
                 * IRpcConnector 是Anno.EngineData 内置的服务调用接口
                 * 例如：this.InvokeProcessor("Anno.Plugs.SoEasy", "AnnoSoEasy", "SayHi", input)
                 * IRpcConnector 接口用户可以自己实现也可以使用 Thrift或者Grpc Anno内置的实现
                 * 此处使用的是Thrift的实现
                 */
                autofac.RegisterType(typeof(RpcConnectorImpl)).As(typeof(IRpcConnector)).SingleInstance();
            }
            , () =>//服务启动后的回调方法
            {
                    /**
                     * 服务Api文档写入注册中心
                     */
                Bootstrap.ApiDoc();
            });
        }


    }
}
