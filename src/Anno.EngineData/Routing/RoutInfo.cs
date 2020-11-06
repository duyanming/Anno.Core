using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Anno.EngineData.Cache;
using Anno.EngineData.Filters;

namespace Anno.EngineData.Routing
{
    public class RoutInfo
    {
        private MethodInfo methodInfo;

        public Type RoutModuleType { get; set; }

        public MethodInfo RoutMethod
        {
            get { return methodInfo; }
            set { methodInfo = value; }
        }
        public List<IAuthorizationFilter> AuthorizationFilters { get; set; } = new List<IAuthorizationFilter>();
        public List<IActionFilter> ActionFilters { get; set; } = new List<IActionFilter>();
        public List<IExceptionFilter> ExceptionFilters { get; set; } = new List<IExceptionFilter>();
        public List<ICacheMiddleware> CacheMiddleware { get; set; } = new List<ICacheMiddleware>();
    }
}
