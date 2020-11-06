using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData.Filters
{
    public interface IActionFilter : IFilterMetadata
    {
        void OnActionExecuted(BaseModule context);
        void OnActionExecuting(BaseModule context);
    }
}
