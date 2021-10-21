using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Anno.Const;

namespace Anno.Rpc.Client
{
    /// <summary>
    /// 调用追踪
    /// </summary>
    public static class TransmitTrace
    {
        private const string Namespace = "Anno.Plugs.Trace";
        private const string Class = "Trace";
        /// <summary>
        /// 调用链深度默认100
        /// </summary>
        public static int CallChainDepth = 100;

        /// <summary>
        /// 链路追踪请求体字段最大长度默认3000
        /// </summary>
        public static int CallChainCharLength = 3000;

        /// <summary>
        /// 设置调用链 TraceId
        /// </summary>
        /// <param name="input"></param>
        /// <param name="micro">目标服务</param>
        public static sys_trace SetTraceId(Dictionary<string, string> input, Micro micro = null)
        {
            if (!SettingService.TraceOnOff)
            {
                return null;
            }
            #region 分布式追踪不记录自己，防止调用链循环调用 +TraceRecord

            if (input[Const.Enum.Eng.NAMESPACE] == Namespace && input[Const.Enum.Eng.CLASS] == Class)
            {
                return null;
            }
            if (input[Const.Enum.Eng.NAMESPACE] == "Anno.Plugs.Monitor" && input[Const.Enum.Eng.CLASS] == "Resource")
            {
                return null;
            }
            #endregion
            #region 调用链唯一标识+ TraceId
            string TraceId = "TraceId";//调用链唯一标识
            string PreTraceId = "PreTraceId";//上一级调用链唯一标识
            string AppName = "AppName";//调用方AnnoService 名称
            string AppNameTarget = "AppNameTarget";//目标AnnoService 名称
            string TTL = "TTL";//分布式调用计数器 默认0
            string GlobalTraceId = "GlobalTraceId";
            if (input.ContainsKey(TraceId))
            {
                input[PreTraceId] = input[TraceId];//当前TraceId 变为上一级 TraceId（PreTraceId）
                input[TraceId] = Guid.NewGuid().ToString();//生成新的调用链唯一标识
            }
            else
            {
                input.Add(TraceId, Guid.NewGuid().ToString());
                input.Add(PreTraceId, string.Empty);
            }
            //追踪全局标识
            if (!input.ContainsKey(GlobalTraceId))
            {
                input.Add(GlobalTraceId, Guid.NewGuid().ToString());
            }
            #endregion

            #region App名称+AppName
            if (input.ContainsKey(AppName))
            {
                input[AppName] = SettingService.AppName ?? string.Empty;
            }
            else
            {
                input.Add(AppName, SettingService.AppName ?? string.Empty);
            }
            #endregion
            #region 目标APP地址
            if (micro != null)
            {
                string target = "Target";
                if (input.ContainsKey(target))
                {
                    input[target] = micro.Ip + ":" + micro.Port;
                }
                else
                {
                    input.Add(target, micro.Ip + ":" + micro.Port);
                }

                if (input.ContainsKey(AppNameTarget))
                {
                    input[AppNameTarget] = micro.Nickname ?? string.Empty;
                }
                else
                {
                    input.Add(AppNameTarget, micro.Nickname ?? string.Empty);
                }
            }
            #endregion
            #region App+跳转次数
            if (input.ContainsKey(TTL))
            {
                int.TryParse(input[TTL], out int ttl);
                if (ttl >= CallChainDepth)
                {
                    throw new GrpcException($"调用链深度不能超过{CallChainDepth},请检查业务或调整深度：Anno.Rpc.Client.TransmitTrace.CallChainDepth");
                }
                input[TTL] = (ttl + 1).ToString();
            }
            else
            {
                input.Add(TTL, "0");
            }
            #endregion

            return TracePool.CreateTrance(input);
        }
    }

    /// <summary>
    /// 追踪队列池
    /// </summary>
    public static class TracePool
    {
        private static ConcurrentQueue<sys_trace> TraceQueue { get; set; } = new ConcurrentQueue<sys_trace>();

        public static void EnQueue(sys_trace trace, string result)
        {
            if (trace != null)
            {
                trace.UseTimeMs = (DateTime.Now - trace.Timespan).TotalMilliseconds;
                trace.Response = result;
                TraceQueue.Enqueue(trace);
            }
        }
        public static sys_trace CreateTrance(Dictionary<string, string> input)
        {
            return new sys_trace()
            {
                Timespan = DateTime.Now,
                InputDictionary = input
            };
        }
        /// <summary>
        /// 批量发送调用链到 追踪服务器
        /// </summary>
        internal static void TryDequeue()
        {
            if (TraceQueue.IsEmpty)
            {
                return;
            }

            List<sys_trace> traces = new List<sys_trace>();
        ReTryDequeue:
            while (!TraceQueue.IsEmpty && traces.Count < 100)
            {
                TraceQueue.TryDequeue(out sys_trace trace);
                trace.Ip = GetValueByKey(trace.InputDictionary, "X-Original-For");
                trace.TraceId = GetValueByKey(trace.InputDictionary, "TraceId");
                trace.PreTraceId = GetValueByKey(trace.InputDictionary, "PreTraceId");
                trace.AppName = GetValueByKey(trace.InputDictionary, "AppName");
                trace.AppNameTarget = GetValueByKey(trace.InputDictionary, "AppNameTarget");
                trace.TTL = RequestInt32(trace.InputDictionary, "TTL");
                trace.Target = GetValueByKey(trace.InputDictionary, "Target");
                trace.Askchannel = GetValueByKey(trace.InputDictionary, "channel");
                trace.Askrouter = GetValueByKey(trace.InputDictionary, "router");
                trace.Askmethod = GetValueByKey(trace.InputDictionary, "method");
                trace.GlobalTraceId = GetValueByKey(trace.InputDictionary, "GlobalTraceId");
                trace.Uname = GetValueByKey(trace.InputDictionary, "uname");
                trace.Rlt = trace.Response?.IndexOf("tatus\":true") > 0;

                /**
                 * 成功的请求清空链路追踪响应值
                 */
                if (trace.Rlt)
                {
                    trace.Response = null;
                }
                /**
                 * 请求内容默认只记录3000字符
                 */
                Dictionary<string, string> requestBody = new Dictionary<string, string>();
                var keys = trace.InputDictionary.Keys;
                foreach (var key in keys)
                {
                    if (key.Equals("channel")
                        || key.Equals("router")
                        || key.Equals("method")
                        || key.Equals("X-Original-For")
                        || key.Equals("TraceId")
                        || key.Equals("PreTraceId")
                        || key.Equals("AppName")
                        || key.Equals("AppNameTarget")
                        || key.Equals("TTL")
                        || key.Equals("Target")
                        || key.Equals("GlobalTraceId")
                        || key.Equals("uname")
                        )
                    {
                        continue;
                    }
                    if (trace.InputDictionary.TryGetValue(key, out string value))
                    {
                        if (!string.IsNullOrEmpty(value) && value.Length > TransmitTrace.CallChainCharLength)
                        {
                            value = value.Substring(0, TransmitTrace.CallChainCharLength);
                        }
                        requestBody.Add(key, value);
                    }
                }
                trace.Request = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);

                traces.Add(trace);
            }
            if (traces.Count <= 0)
            {
                return;
            }
            Dictionary<string, string> inputTrace = new Dictionary<string, string>
            {
                {Const.Enum.Eng.NAMESPACE, "Anno.Plugs.Trace"},
                {Const.Enum.Eng.CLASS, "Trace"},
                {Const.Enum.Eng.METHOD, "TraceBatch"},
                {"traces", Newtonsoft.Json.JsonConvert.SerializeObject(traces)}
            };
            Connector.BrokerDns(inputTrace);
            if (!TraceQueue.IsEmpty)
            {
                traces.Clear();
                goto ReTryDequeue;
            }
        }

        #region 工具

        private static string GetStringDic(Dictionary<string, string> input)
        {
            StringBuilder json = new StringBuilder();
            json.AppendFormat("{0}", "{");
            foreach (var key in input.Keys)
            {
                json.AppendFormat("\"{0}\":\"{1}\",", key, input[key]);
            }

            if (json.Length > 1)
            {
                json.Remove(json.Length - 1, 1);
            }
            json.AppendFormat("{0}", "}");
            return json.ToString();
        }

        /// <summary>
        /// 根据键获取值 Int32
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns>Int32 Value</returns>
        private static Int32? RequestInt32(Dictionary<string, string> input, string key)
        {
            if (RequestContainsKey(input, key))
            {
                return Convert.ToInt32(GetValueByKey(input, key));
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据Key 获取字符串值
        /// </summary>
        /// <param name="input">键</param>
        /// <param name="key">键</param>
        /// <returns>字符串值</returns>
        private static string GetValueByKey(Dictionary<string, string> input, string key)
        {
            if (RequestContainsKey(input, key))
            {
                return input[key];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 上下文是否包含 key
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool RequestContainsKey(Dictionary<string, string> input, string key)
        {
            return input.ContainsKey(key);
        }
        #endregion
    }

    /// <summary>
    /// 实体类sys_trace。(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    public partial class sys_trace
    {
        public long ID { get; set; }
        /// <summary>
        /// 调用链全局唯一标识
        /// </summary>
        public string GlobalTraceId { get; set; }
        /// <summary>
        /// 调用链唯一标识
        /// </summary>
        public string TraceId { get; set; }
        /// <summary>
        /// 上级调用链唯一标识
        /// </summary>
        public string PreTraceId { get; set; }
        /// <summary>
        /// 调用方App名称
        /// </summary>
        public string AppName { get; set; }
        /// <summary>
        /// 目标App名称
        /// </summary>
        public string AppNameTarget { get; set; }
        /// <summary>
        /// 跳转次数
        /// </summary>
        public int? TTL { get; set; }
        /// <summary>
        /// 请求参数
        /// </summary>
        public string Request { get; set; }
        /// <summary>
        /// 响应参数
        /// </summary>
        public string Response { get; set; }
        /// <summary>
        /// 处理结果
        /// </summary>
        public bool Rlt { get; set; }
        /// <summary>
        /// 操作人IP
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 目标地址
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 操作人ID
        /// </summary>
        public long? UserId { get; set; }
        /// <summary>
        /// 操作人名称
        /// </summary>
        public string Uname { get; set; }
        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime Timespan { get; set; }
        /// <summary>
        /// 请求管道
        /// </summary>
        public string Askchannel { get; set; }
        /// <summary>
        /// 请求路由
        /// </summary>
        public string Askrouter { get; set; }
        /// <summary>
        /// 业务方法
        /// </summary>
        public string Askmethod { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<string, string> InputDictionary { get; set; }

        /// <summary>
        /// 耗时单位毫秒
        /// </summary>
        public double UseTimeMs { get; set; }
    }
}
