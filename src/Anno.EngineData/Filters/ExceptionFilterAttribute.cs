using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData.Filters
{
    public abstract class ExceptionFilterAttribute : Attribute, IExceptionFilter, IFilterMetadata
    {
        public virtual void OnException(Exception ex, BaseModule context)
        {

        }
    }
}
