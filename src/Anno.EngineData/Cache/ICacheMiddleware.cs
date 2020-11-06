using Anno.EngineData.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData.Cache
{
    public interface ICacheMiddleware : IFilterMetadata
    {
        bool TryGetCache(string key, out ActionResult actionResult);
        void SetCache(string key, ActionResult actionResult);
        void RemoveCache(string key);
    }
}
