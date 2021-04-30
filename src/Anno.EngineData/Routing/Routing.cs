using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Anno.EngineData.Filters;
using System.Linq;
using Anno.EngineData.Cache;

namespace Anno.EngineData.Routing
{
    public static class Routing
    {
        internal static List<IActionFilter> GlobalActionFilters { get; private set; } = new List<IActionFilter>();
        internal static List<IExceptionFilter> GlobalExceptionFilters { get; private set; } = new List<IExceptionFilter>();
        internal static List<IAuthorizationFilter> GlobalAuthorizationFilters { get; private set; } = new List<IAuthorizationFilter>();
        internal static List<ICacheMiddleware> GlobalCacheMiddleware { get; set; } = new List<ICacheMiddleware>();

        #region 全局过滤器
        public static bool AddFilter(IFilterMetadata metadata)
        {
            bool success = false;
            if (metadata.GetType().GetInterface("IActionFilter") != null)
            {
                if (!GlobalActionFilters.Contains(metadata))
                {
                    GlobalActionFilters.Add(metadata as IActionFilter);
                    success = true;
                }
            }
            if (metadata.GetType().GetInterface("IExceptionFilter") != null)
            {
                if (!GlobalExceptionFilters.Contains(metadata))
                {
                    GlobalExceptionFilters.Add(metadata as IExceptionFilter);
                    success = true;
                }
            }
            if (metadata.GetType().GetInterface("IAuthorizationFilter") != null)
            {
                if (!GlobalAuthorizationFilters.Contains(metadata))
                {
                    GlobalAuthorizationFilters.Add(metadata as IAuthorizationFilter);
                    success = true;
                }
            }
            if (metadata.GetType().GetInterface("ICacheMiddleware") != null)
            {
                if (!GlobalCacheMiddleware.Contains(metadata))
                {
                    GlobalCacheMiddleware.Add(metadata as ICacheMiddleware);
                    success = true;
                }
            }
            return success;
        }
        #endregion
        public static ConcurrentDictionary<string, RoutInfo> Router { get; set; } = new ConcurrentDictionary<string, RoutInfo>();
    }
}
