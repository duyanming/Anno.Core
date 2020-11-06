using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData.Filters
{
    public abstract class AuthorizationFilterAttribute : Attribute, IAuthorizationFilter, IFilterMetadata
    {
        public virtual void OnAuthorization(BaseModule context)
        {

        }
    }
}
