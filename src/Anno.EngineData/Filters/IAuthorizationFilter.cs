using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData.Filters
{
    public interface IAuthorizationFilter : IFilterMetadata
    {
        void OnAuthorization(BaseModule context);
    }
}
