using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnnoCenter
{
    using Anno.Rpc.Center;
    using Anno.Log;
    static class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "AnnoCenter";
            /*
            * 服务上线、下线、不健康提醒、变更事件
            */
            Bootstrap.StartUp(args, (service, noticeType) => {
                Log.WriteLine(noticeType.ToString() + ":\t\n" + Newtonsoft.Json.JsonConvert.SerializeObject(service));
            }, (newService, oldService) => {
                Log.WriteLine("NewConfig:\t\n" + Newtonsoft.Json.JsonConvert.SerializeObject(newService));
                Log.WriteLine("OldConfig:\t\n" + Newtonsoft.Json.JsonConvert.SerializeObject(oldService));
            });
        } 

    }
}
