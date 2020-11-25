using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anno.Const.Enum;

namespace Anno.EngineData
{
    public abstract class BaseModule : InvokeEngine
    {
        /// <summary>
        /// 是否通过了授权
        /// </summary>
        public bool Authorized { get; set; } = false;
        public const string Table = "__table__";
        /// <summary>
        /// FormInput信息
        /// </summary>
        private Dictionary<string, string> _input;

        /// <summary>
        /// FormInput信息
        /// </summary>
        public Dictionary<string, string> Input => _input;

        /// <summary>
        /// 前置初始化方法
        /// </summary>
        /// <param name="input">表单数据</param>
        /// <returns>是否成功</returns>
        public virtual bool Init(Dictionary<string, string> input)
        {
            this._input = input;
            return true;
        }

        public new string InvokeProcessor(string channel, string router, string method, Dictionary<string, string> response)
        {
            /*
             * 采用复制品 发送请求。防止修改当前上下文对象Input
             */
            if (response.Equals(Input))
            {
                var responseReplica = new Dictionary<string, string>();
                foreach (var dic in response)
                {
                    responseReplica.Add(dic.Key, dic.Value);
                }
                return InvokeEngine.InvokeProcessor(channel, router, method, responseReplica);
            }

            if (!response.ContainsKey("TraceId") && Input.ContainsKey("TraceId"))
            {
                response.Add("TraceId", RequestString("TraceId"));
            }
            if (!response.ContainsKey("PreTraceId") && Input.ContainsKey("PreTraceId"))
            {
                response.Add("PreTraceId", RequestString("PreTraceId"));
            }
            if (!response.ContainsKey("TTL") && Input.ContainsKey("TTL"))
            {
                response.Add("TTL", RequestString("TTL"));
            }
            if (!response.ContainsKey("GlobalTraceId") && Input.ContainsKey("GlobalTraceId"))
            {
                response.Add("GlobalTraceId", RequestString("GlobalTraceId"));
            }
            return InvokeEngine.InvokeProcessor(channel, router, method, response);
        }
        public new Task<string> InvokeProcessorAsync(string channel, string router, string method, Dictionary<string, string> response)
        {
            /*
             * 采用复制品 发送请求。防止修改当前上下文对象Input
             */
            if (response.Equals(_input))
            {
                var responseReplica = new Dictionary<string, string>();
                foreach (var dic in response)
                {
                    responseReplica.Add(dic.Key, dic.Value);
                }
                return InvokeEngine.InvokeProcessorAsync(channel, router, method, responseReplica);
            }

            if (!response.ContainsKey("TraceId") && Input.ContainsKey("TraceId"))
            {
                response.Add("TraceId", RequestString("TraceId"));
            }
            if (!response.ContainsKey("PreTraceId") && Input.ContainsKey("PreTraceId"))
            {
                response.Add("PreTraceId", RequestString("PreTraceId"));
            }
            if (!response.ContainsKey("TTL") && Input.ContainsKey("TTL"))
            {
                response.Add("TTL", RequestString("TTL"));
            }
            if (!response.ContainsKey("GlobalTraceId") && Input.ContainsKey("GlobalTraceId"))
            {
                response.Add("GlobalTraceId", RequestString("GlobalTraceId"));
            }
            return InvokeEngine.InvokeProcessorAsync(channel, router, method, response);
        }

        #region AutoFac+Resolve
        /// <summary>
        /// AutoFac 获取实例对象
        /// </summary>
        /// <typeparam name="T">实例对象</typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            return Loader.IocLoader.Resolve<T>();
        }
        public T Resolve<T>(Type serviceType) where T : class
        {
            return Loader.IocLoader.Resolve<T>(serviceType);
        }
        #endregion

        #region Request+获取请求上下文
        /// <summary>
        /// 获取序列化对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">对象键值</param>
        /// <returns></returns>
        public T Request<T>(string key) where T : class, new()
        {
            if (RequestContainsKey(key))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(RequestString(key));
            }
            return default(T);
        }
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
                return _input[key];
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
            return _input.ContainsKey(key);
        }
        /// <summary>
        /// 获取Request 键值集合
        /// </summary>
        /// <returns></returns>
        public List<string> RequestKeys()
        {
            return _input.Keys.ToList();
        }
        /// <summary>
        /// 获取前端列表的过滤条件
        /// </summary>
        /// <returns></returns>
        public string Filter()
        {
            string where = string.Empty;
            var groups = Request<Group>("where");
            if (groups != null && groups.rules.Count > 0)
            {
                where = ExpressionAnalysis.TransmitFilter(groups, Table);
            }

            if (string.IsNullOrWhiteSpace(where))
            {
                where = " 1=1 ";
            }

            return where;
        }

        #endregion

        /// <summary>
        /// 用户身份令牌
        /// </summary>
        public ProfileToken Profile { get; set; }
        /// <summary>
        /// 执行结果
        /// </summary>
        public ActionResult ActionResult { get; set; }
    }

    public class ProfileToken
    {
        #region 属性

        public long ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Pwd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long Coid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Position { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public short? State { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Profile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? Timespan { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? Rdt { get; set; }
        #endregion
    }
}
