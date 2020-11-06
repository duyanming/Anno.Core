using Anno.EngineData.Filters;
using Anno.EngineData.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData.Cache
{
    public abstract class CacheMiddlewareAttribute : Attribute, ICacheMiddleware, IFilterMetadata
    {
        public abstract void RemoveCache(string key);

        public abstract void SetCache(string key, ActionResult actionResult);
        public abstract bool TryGetCache(string key, out ActionResult actionResult);
    }
}
