using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Anno.EngineData.Cache;
using Anno.EngineData.Filters;

namespace Anno.EngineData.Routing
{
    public class RoutInfo
    {
        private readonly static Type iActionResultType = typeof(IActionResult);
        private readonly static Type taskType = typeof(Task);

        private MethodInfo methodInfo;

        public Type RoutModuleType { get; set; }

        private bool returnTypeIsTask = false;
        /// <summary>
        /// 返回类型是否为Task子类
        /// </summary>
        public bool ReturnTypeIsTask { get { return returnTypeIsTask; } }

        private bool returnTypeIsIActionResult = false;
        /// <summary>
        /// 返回类型是否为IActionResult子类
        /// </summary>
        public bool ReturnTypeIsIActionResult { get { return returnTypeIsIActionResult; } }

        public MethodInfo RoutMethod
        {
            get { return methodInfo; }
            set
            {
                methodInfo = value;
                if (value != null)
                {
                    returnTypeIsTask = taskType.IsAssignableFrom(value.ReturnType);
                    if (returnTypeIsTask&& value.ReturnType.GetGenericArguments().Length>0)
                    {
                        returnTypeIsIActionResult = iActionResultType.IsAssignableFrom(value.ReturnType.GetGenericArguments()[0]);  
                    }
                    else
                    {
                        returnTypeIsIActionResult = iActionResultType.IsAssignableFrom(value.ReturnType);
                    }
                }
                else
                {
                    returnTypeIsTask = false;
                    returnTypeIsIActionResult = false;
                }
            }
        }
        public List<IAuthorizationFilter> AuthorizationFilters { get; set; } = new List<IAuthorizationFilter>();
        public List<IActionFilter> ActionFilters { get; set; } = new List<IActionFilter>();
        public List<IExceptionFilter> ExceptionFilters { get; set; } = new List<IExceptionFilter>();
        public List<ICacheMiddleware> CacheMiddleware { get; set; } = new List<ICacheMiddleware>();
    }
}
