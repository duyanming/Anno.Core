using Anno.EngineData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jky.Web.Core.ModelManageService
{
    public class SayHiModule:BaseModule
    {
        public dynamic SayHi(string name) {
            return $"你好,{name}";
        }
    }
}
