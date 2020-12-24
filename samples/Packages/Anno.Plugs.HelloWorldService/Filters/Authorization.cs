using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Plugs.HelloWorldService.Filters
{
    using Anno.EngineData;
    using Anno.EngineData.Filters;
    /// <summary>
    /// 服务权限过滤器
    /// </summary>
    public class Authorization : AuthorizationFilterAttribute
    {
        public string Msg { get; set; }
        public override void OnAuthorization(BaseModule context)
        {
            if (context.RequestString("uname") != "yrm")
            {
                context.Authorized = false;
                return;
            }
            context.Authorized = true;
        }
    }
}
