using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnnoCenter
{
    using Anno.Rpc.Center;
    static class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "AnnoCenter";
            Bootstrap.StartUp(args, (service, noticeType) => {
                Console.WriteLine(noticeType.ToString() + ":" + Newtonsoft.Json.JsonConvert.SerializeObject(service));
            }, (newService, oldService) => {
                Console.WriteLine("NewConfig:" + Newtonsoft.Json.JsonConvert.SerializeObject(newService));
                Console.WriteLine("OldConfig:" + Newtonsoft.Json.JsonConvert.SerializeObject(oldService));
            });
        } 

    }
}
