using Anno.Const.Attribute;
using Anno.EngineData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Anno.Plugs.HelloWorldService
{
    public class HelloWorldTaskModule : BaseModule
    {
        [AnnoInfo(Desc = "世界你好啊 async Task<dynamic> SayHelloAsync")]
        public async Task<dynamic> SayHelloAsync([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            dynamic rlt = new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了" };
            return await Task.FromResult(rlt);
        }
        [AnnoInfo(Desc = "世界你好啊Task<dynamic> SayHello")]
        public Task<dynamic> SayHello([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            object rlt = new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了" };
            return Task.FromResult(rlt);
        }
        [AnnoInfo(Desc = "" +
            "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
            "SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello")]
        public Task<dynamic> ApiDocLengthTest([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            object rlt = new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了" };
            return Task.FromResult(rlt);
        }
    }
}
