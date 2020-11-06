using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData
{
    public class ActionResult : ActionResult<object>
    {
        public ActionResult()
        {
            Output = new Dictionary<string, object>();
            this.Status = true;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">状态</param>
        public ActionResult(Boolean status) : this()
        {
            this.Status = status;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">状态</param>
        /// <param name="outputData">结果集</param>
        public ActionResult(Boolean status, object outputData) : this(status)
        {
            this.OutputData = outputData;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">状态</param>
        /// <param name="outputData">结果集</param>
        /// <param name="output">字典</param>
        public ActionResult(Boolean status, object outputData, Dictionary<string, object> output) : this(status, outputData)
        {
            this.Output = output;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">状态</param>
        /// <param name="outputData">结果集</param>
        /// <param name="output">字典</param>
        /// <param name="msg">消息</param>
        public ActionResult(Boolean status, object outputData, Dictionary<string, object> output, string msg) : this(status, outputData, output)
        {
            this.Msg = msg;
        }
    }

    public class ActionResult<T>: IActionResult
    { /// <summary>
      /// 状态
      /// </summary>
        public Boolean Status { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 字典
        /// </summary>
        public Dictionary<string, object> Output { get; set; }
        /// <summary>
        /// 结果集
        /// </summary>
        public T OutputData { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public ActionResult()
        {
            Output = new Dictionary<string, object>();
            this.Status = true;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">状态</param>
        public ActionResult(Boolean status) : this()
        {
            this.Status = status;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">状态</param>
        /// <param name="outputData">结果集</param>
        public ActionResult(Boolean status, T outputData) : this(status)
        {
            this.OutputData = outputData;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">状态</param>
        /// <param name="outputData">结果集</param>
        /// <param name="output">字典</param>
        public ActionResult(Boolean status, T outputData, Dictionary<string, object> output) : this(status, outputData)
        {
            this.Output = output;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">状态</param>
        /// <param name="outputData">结果集</param>
        /// <param name="output">字典</param>
        /// <param name="msg">消息</param>
        public ActionResult(Boolean status, T outputData, Dictionary<string, object> output, string msg) : this(status, outputData, output)
        {
            this.Msg = msg;
        }
    }

    public interface IActionResult { }
}
