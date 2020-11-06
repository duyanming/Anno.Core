using Anno.EngineData;
using System;
using System.Linq;
using Anno.Const.Attribute;

namespace Anno.Plugs.TraceService
{
    using System.Collections.Generic;

    public class TraceModule : BaseModule
    {

        public TraceModule()
        {

        }
        /// <summary>
        /// 批量接收追踪信息
        /// </summary>
        /// <returns></returns>
        [AnnoInfo(Desc = "持久化链路信息")]
        public ActionResult TraceBatch()
        {
            /*
             * 调用链路信息写入磁盘
             * 开发人员可以根据需要选择写入关系数据库、ES、MongoDB等等
             */
            Log.Log.Info(RequestString("traces"));
            return new ActionResult(true, null, null, null);
        }
        
        #region  Module 初始化
        public override bool Init(Dictionary<string, string> input)
        {
            base.Init(input);            
            return true;
        }
        #endregion
    }
}
