using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData.Filters
{
  public abstract class ActionFilterAttribute: Attribute, IActionFilter, IFilterMetadata
    {
        public virtual void OnActionExecuted(BaseModule context)
        {
            
        }

        public virtual void OnActionExecuting(BaseModule context)
        {
           
        }
    }
}
