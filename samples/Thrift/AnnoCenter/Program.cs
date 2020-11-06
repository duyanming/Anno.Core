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
            Bootstrap.StartUp(args);
        } 

    }
}
