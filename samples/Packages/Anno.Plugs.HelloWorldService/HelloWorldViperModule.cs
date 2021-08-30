/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/10/30 13:15:24 
Functional description： HelloWorldViperModule
******************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Plugs.HelloWorldService
{
    using Anno.Const.Attribute;
    using Anno.EngineData;
    using Anno.Plugs.HelloWorldService.Filters;
    using HelloWorldDto;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;

    public class HelloWorldViperModule : BaseModule
    {
        [AnnoInfo(Desc = "世界你好啊SayHi")]
        public dynamic SayHello([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            input.Add("vname", name);
            input.Add("vage", age.ToString());
            var soEasyMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<ActionResult<string>>(this.InvokeProcessor("Anno.Plugs.SoEasy", "AnnoSoEasy", "SayHi", input)).OutputData;
            return new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了", SoEasyMsg = soEasyMsg };
        }

        [AnnoInfo(Desc = "两个整数相减等于几？我来帮你算（x-y=?）")]
        public int Subtraction([AnnoInfo(Desc = "整数X")] int x, [AnnoInfo(Desc = "整数Y")] int y)
        {
            return x - y;
        }
        [AnnoInfo(Desc = "买个商品吧，双十一马上就来了")]
        public ProductDto BuyProduct([AnnoInfo(Desc = "商品名称")] string productName, [AnnoInfo(Desc = "商品数量")] int number)
        {
            double price = new Random().Next(2, 90);
            Dictionary<string, string> input = new Dictionary<string, string>();
            input.Add("productName", productName);
            input.Add("number", number.ToString());
            var product = Newtonsoft.Json.JsonConvert.DeserializeObject<ActionResult<ProductDto>>(this.InvokeProcessor("Anno.Plugs.SoEasy", "AnnoSoEasy", "BuyProduct", input)).OutputData;
            product.CountryOfOrigin = $"中国北京中转--{ product.CountryOfOrigin}";
            return product;
        }
        
        [AnnoInfo(Desc = "带有授权的接口仅限用户yrm访问密码123456")]
        [Authorization]
        public dynamic SayHi([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            input.Add("vname", name);
            input.Add("vage", age.ToString());
            var soEasyMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<ActionResult<string>>(this.InvokeProcessor("Anno.Plugs.SoEasy", "AnnoSoEasy", "SayHi", input)).OutputData;
            return new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了", SoEasyMsg = soEasyMsg };
        }
        #region 测试接口
        [AnnoInfo(Desc = "测试接口（模拟等待，返回等待毫秒数）")]
        public ActionResult Test()
        {
            var i = new Random().Next(1, 80);
            System.Threading.Tasks.Task.Delay(i).Wait();//等待1秒
            return new ActionResult(true, i + " :Test");
        }
        [AnnoInfo(Desc = "测试接口（返回true）")]
        public ActionResult Test0()
        {
            return new ActionResult(true);
        }
        [AnnoInfo(Desc = "测试接口（{Id} From Server Test1.）")]
        public ActionResult Test1([AnnoInfo(Desc = "Id")] string id)
        {
            return new ActionResult(true, id + " From Server Test1.");
        }
        [AnnoInfo(Desc = "测试属性校验接口（名称字段【Name】必须输入、年龄有效范围0-150）")]
        public ActionResult Test2([AnnoInfo(Desc = "接收输入对象")] TestDto dto)
        {
            var vrlt = dto.IsValid();
            if (!vrlt.IsVaild)
            {
                return new ActionResult(false, vrlt.ErrorMembers);
            }
            return new ActionResult(true, "OK  Test2");
        }
        [AnnoInfo(Desc = "测试属性校验接口[FromBody]（名称字段【Name】必须输入、年龄有效范围0-150）")]
        public ActionResult TestFb([FromBody] TestDto dto)
        {
            var vrlt = dto.IsValid();
            if (!vrlt.IsVaild)
            {
                return new ActionResult(false, vrlt.ErrorMembers);
            }
            return new ActionResult(true, "TestFb");
        }
        #endregion

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [AnnoInfo(Desc = "上传文件")]
        public dynamic UpLoadFile()
        {
            var file = Request<AnnoFile>("annoFile");
            var filePath = AppDomain.CurrentDomain.BaseDirectory;
            using (var stream = System.IO.File.Create(System.IO.Path.Combine(filePath, file.FileName)))
            {
                stream.Write(file.Content, 0, file.Length);
            }
            return new ActionResult(true, new { Msg = "上传成功", SourceId = 18 });
        }

        public dynamic AddProducts(List<ProductDto> products) {

            return products;
        }
        /// <summary>
        /// 等待指定的秒数
        /// </summary>
        /// <param name="seconds">时间单位秒</param>
        /// <returns></returns>
        public dynamic WaitFor(int seconds) {
            DateTime starTime = DateTime.Now;
            Task.Delay(seconds * 1000).Wait();
            DateTime endTime = DateTime.Now;
            return $"starTime:[{starTime:yyyy-MM-dd HH:mm:ss}],endTime:[{endTime:yyyy-MM-dd HH:mm:ss}]";
        }
    }
    public class TestDto
    {
        [Required(ErrorMessage = "名称字段【Name】必须输入")]
        public string Name { get; set; }
        [Range(0, 150, ErrorMessage = "年龄有效范围0-150")]
        public int Age { get; set; }

        public DateTime Birthday { get; set; }
    }
}
