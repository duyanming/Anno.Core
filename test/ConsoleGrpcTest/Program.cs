using System;

namespace ConsoleGrpcTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("测试开始---------------------Start");
            new GrpcTest().Handle8();
            Console.WriteLine("测试结束---------------------End");
            Console.ReadLine();
        }
    }
}
