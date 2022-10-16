using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anno
{
    /// <summary>
    /// Task扩展
    /// </summary>
    public static class AnnoContext
    {
        /// <summary>
        /// WebApi 调用唯一标识
        /// </summary>
        [ThreadStatic]
        public static AnnoRequestContext Current;
    }
    /// <summary>
    /// Anno请求上下文
    /// </summary>
    public class AnnoRequestContext
    {
        /// <summary>
        /// 请求上下文
        /// </summary>
        public Dictionary<string, string> Input { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 扩展数据
        /// </summary>
        public dynamic Data { get; set; }

        #region Request+获取请求上下文
        /// <summary>
        /// 根据键获取值 Int16
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Int16 Value</returns>
        public Int16? RequestInt16(string key)
        {
            if (RequestContainsKey(key))
            {
                return Convert.ToInt16(GetValueByKey(key));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据键获取值 Int32
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Int32 Value</returns>
        public Int32? RequestInt32(string key)
        {
            if (RequestContainsKey(key))
            {
                return Convert.ToInt32(GetValueByKey(key));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据键获取值 Int64
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Int64 Value</returns>
        public Int64? RequestInt64(string key)
        {

            if (RequestContainsKey(key))
            {
                return Convert.ToInt64(GetValueByKey(key));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据键获取值 Boolean
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Boolean Value</returns>
        public Boolean? RequestBoolean(string key)
        {

            if (RequestContainsKey(key))
            {
                return Convert.ToBoolean(GetValueByKey(key).ToLower());
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据键获取值 DateTime
        /// </summary>
        /// <param name="key"></param>
        /// <returns>DateTime Value</returns>
        public DateTime? RequestDateTime(string key)
        {

            if (RequestContainsKey(key))
            {
                return Convert.ToDateTime(GetValueByKey(key));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据键获取值 Decimal
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Decimal Value</returns>
        public Decimal? RequestDecimal(string key)
        {

            if (RequestContainsKey(key))
            {
                return Convert.ToDecimal(GetValueByKey(key));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据键获取值 Double
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Double Value</returns>
        public Double? RequestDouble(string key)
        {

            if (RequestContainsKey(key))
            {
                return Convert.ToDouble(GetValueByKey(key));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据键获取值 float
        /// </summary>
        /// <param name="key"></param>
        /// <returns>float Value</returns>
        public float? RequestSingle(string key)
        {

            if (RequestContainsKey(key))
            {
                return Convert.ToSingle(GetValueByKey(key));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据键获取值 string
        /// </summary>
        /// <param name="key"></param>
        /// <returns>string Value</returns>
        public string RequestString(string key)
        {
            return GetValueByKey(key);
        }
        /// <summary>
        /// 根据Key 获取字符串值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>字符串值</returns>
        private string GetValueByKey(string key)
        {
            if (RequestContainsKey(key))
            {
                return Input[key];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 上下文是否包含 key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RequestContainsKey(string key)
        {
            return Input.ContainsKey(key);
        }
        /// <summary>
        /// 获取Request 键值集合
        /// </summary>
        /// <returns></returns>
        public List<string> RequestKeys()
        {
            return Input.Keys.ToList();
        }
        #endregion
    }
}
