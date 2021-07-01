using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Anno.Const.Enum;
using Anno.EngineData.Filters;

namespace Anno.EngineData
{
    /// <summary>
    /// 数据集散中心
    /// </summary>
    public static class Engine
    {
        private readonly static Type enumType = typeof(Enum);
        /// <summary>
        /// 转发器
        /// </summary>
        /// <param name="input">表单数据</param>
        /// <returns></returns>
        public static ActionResult Transmit(Dictionary<string, string> input)
        {
            #region 查找路由信息RoutInfo
            var key = $"{input[Eng.NAMESPACE]}Service.{input[Eng.CLASS]}Module/{input[Eng.METHOD]}";
            if (Routing.Routing.Router.TryGetValue(key, out Routing.RoutInfo routInfo))
            {
                try
                {
                    return Transmit(input, routInfo);
                }
                catch (Exception ex)
                {
                    //记录日志
                    Log.Log.Error(ex, routInfo.RoutModuleType);
                    return new ActionResult()
                    {
                        Status = false,
                        OutputData = null,
                        Msg = ex.InnerException?.Message ?? ex.Message
                    };
                }
            }
            else
            {
                return new ActionResult()
                {
                    Status = false,
                    OutputData = null,
                    Msg = $"在【{input[Eng.NAMESPACE]}】中找不到【{input[Eng.CLASS]}.{input[Eng.METHOD]}】！"
                };
            }
            #endregion
        }
        /// <summary>
        /// 转发器异步
        /// </summary>
        /// <param name="input">表单数据</param>
        /// <returns></returns>
        public static async Task<ActionResult> TransmitAsync(Dictionary<string, string> input)
        {
            return await Task.Run(() => Transmit(input)).ConfigureAwait(false);
        }
        /// <summary>
        /// 根据服务转发
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type">表示类型声明：类类型、接口类型、数组类型、值类型、枚举类型、类型参数、泛型类型定义，以及开放或封闭构造的泛型类型</param>
        /// <returns></returns>
        public static ActionResult Transmit(Dictionary<string, string> input, Routing.RoutInfo routInfo)
        {
            BaseModule module = null;
            try
            {
                #region Cache
                string key = string.Empty;
                if (routInfo.CacheMiddleware.Count > 0)
                {
                    key = GetDicStrHashCode(input);
                    if (TryGetCache(routInfo, key, out ActionResult rltCache))
                    {
                        return rltCache;
                    }
                }
                #endregion
                List<object> lo = new List<object>() { input };
                module = Loader.IocLoader.Resolve<BaseModule>(routInfo.RoutModuleType);
                var init = module.Init(input);
                if (!init)
                {
                    return new ActionResult()
                    {
                        Status = false,
                        Msg = "Init拦截！"
                    };
                }
                if (routInfo.RoutMethod == null)
                {
                    return new ActionResult()
                    {
                        Status = false,
                        OutputData = null,
                        Msg = $"在【{input[Eng.NAMESPACE]}】中找不到【{input[Eng.CLASS]}.{input[Eng.METHOD]}】！"
                    };
                }
                #region Authorization
                for (int i = 0; i < routInfo.AuthorizationFilters.Count; i++)
                {
                    routInfo.AuthorizationFilters[i].OnAuthorization(module);
                    if (!module.Authorized)
                    {
                        return module.ActionResult == null ? new ActionResult()
                        {
                            Status = false,
                            OutputData = 401,
                            Msg = "401,Unauthrized"
                        } : module.ActionResult
                        ;
                    }
                }
                #endregion
                for (int i = 0; i < routInfo.ActionFilters.Count; i++)
                {
                    routInfo.ActionFilters[i].OnActionExecuting(module);
                }
                #region 调用业务方法
                object rltCustomize = null;
                if (routInfo.ReturnTypeIsTask)
                {
                    var rlt = (routInfo.RoutMethod.Invoke(module, DicToParameters(routInfo.RoutMethod, input).ToArray()) as Task);
                    rltCustomize = routInfo.RoutMethod.ReturnType.GetProperty("Result").GetValue(rlt, null);
                }
                else
                {
                    rltCustomize = routInfo.RoutMethod.Invoke(module, DicToParameters(routInfo.RoutMethod, input).ToArray());
                }

                if (routInfo.ReturnTypeIsIActionResult && rltCustomize != null)
                {
                    module.ActionResult = rltCustomize as ActionResult;
                }
                else
                {
                    module.ActionResult = new ActionResult(true, rltCustomize);
                }
                #endregion
                for (int i = (routInfo.ActionFilters.Count - 1); i >= 0; i--)
                {
                    routInfo.ActionFilters[i].OnActionExecuted(module);
                }
                if (routInfo.CacheMiddleware.Count > 0)
                {
                    AddCache(routInfo, key, module.ActionResult);
                }
                return module.ActionResult;
            }
            catch (Exception ex)
            {
                if (routInfo.RoutMethod != null)
                {
                    foreach (var ef in routInfo.ExceptionFilters)
                    {
                        ef.OnException(ex, module);
                    }
                }
#if DEBUG
                //记录日志
                Log.Log.Error(ex, routInfo.RoutModuleType);
#endif
                return new ActionResult()
                {
                    Status = false,
                    OutputData = null,
                    Msg = ex.InnerException?.Message ?? ex.Message
                };
            }
        }

        /// <summary>
        /// 根据服务转发
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type">表示类型声明：类类型、接口类型、数组类型、值类型、枚举类型、类型参数、泛型类型定义，以及开放或封闭构造的泛型类型</param>
        /// <returns></returns>
        public static async Task<ActionResult> TransmitAsync(Dictionary<string, string> input, Routing.RoutInfo routInfo)
        {
            return await Task.Run(() => Transmit(input, routInfo)).ConfigureAwait(false);
        }
        /// <summary>
        /// 扩展属性校验
        /// </summary>
        /// <param name="method"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private static List<object> DicToParameters(MethodInfo method, Dictionary<string, string> input)
        {
            List<object> parameters = new List<object>();
            foreach (var p in method.GetParameters())
            {
                if (p.GetCustomAttributes<FromBodyAttribute>().Any())
                {
                    parameters.Add(Newtonsoft.Json.JsonConvert.DeserializeObject(Newtonsoft.Json.JsonConvert.SerializeObject(input), p.ParameterType));
                    continue;
                }
                else if (input.ContainsKey(p.Name))
                {
                    if (p.ParameterType.FullName.StartsWith("System.Collections.Generic.List`1["))
                    {
                        parameters.Add(Newtonsoft.Json.JsonConvert.DeserializeObject(input[p.Name], p.ParameterType));
                    }
                    else if (p.ParameterType.FullName.StartsWith("System."))//系统基础数据类型
                    {
                        parameters.Add(Convert.ChangeType(input[p.Name], p.ParameterType));//枚举
                    }
                    else if (p.ParameterType.BaseType == enumType)
                    {

                        parameters.Add(Enum.Parse(p.ParameterType, input[p.Name]));
                    }
                    else // 系统基础数据类型、枚举 之外。例如 结构体、类、匿名对象
                    {
                        parameters.Add(Newtonsoft.Json.JsonConvert.DeserializeObject(input[p.Name], p.ParameterType));
                    }
                }
                else
                {
                    parameters.Add(default);
                }
            }
            return parameters;
        }

        private static bool TryGetCache(Routing.RoutInfo routInfo, string key, out ActionResult actionResult)
        {
            actionResult = null;
            for (int i = 0; i < routInfo.CacheMiddleware.Count; i++)
            {
                var cm = routInfo.CacheMiddleware[i];
                if (cm.TryGetCache(key, out actionResult))
                {
                    return true;
                }
            }
            return false;
        }
        private static void AddCache(Routing.RoutInfo routInfo, string key, ActionResult actionResult)
        {
            for (int i = 0; i < routInfo.CacheMiddleware.Count; i++)
            {
                var cm = routInfo.CacheMiddleware[i];
                cm.SetCache(key, actionResult);
            }
        }

        private static string GetDicStrHashCode(Dictionary<string, string> input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in input)
            {
                if (item.Key == "X-Original-For"
                    || item.Key == "profile"
                      || item.Key == "TraceId"
                        || item.Key == "PreTraceId"
                          || item.Key == "GlobalTraceId"
                            || item.Key == "AppName"
                              || item.Key == "Target"
                                || item.Key == "AppNameTarget"
                                 || item.Key == "TTL"
                                  || item.Key == "t"
                    )//排除系统内置参数
                {
                    continue;
                }
                stringBuilder.Append(item.Key);
                stringBuilder.Append(item.Value);
            }
            return stringBuilder.ToString().HashCode();
        }

        /// <summary>
        /// 计算文件的哈希值
        /// </summary>
        /// <param name="buffer">被操作的源数据流</param>
        /// <param name="algo">加密算法</param>
        /// <returns>哈希值16进制字符串</returns>
        private static string HashCode(this string str, string algo = "md5")
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            HashAlgorithm hashAlgorithm = null;
            switch (algo)
            {
                case "sha1":
                    hashAlgorithm = new SHA1CryptoServiceProvider();
                    break;
                case "md5":
                    hashAlgorithm = new MD5CryptoServiceProvider();
                    break;
                default:
                    hashAlgorithm = new MD5CryptoServiceProvider();
                    break;
            }

            var hash = hashAlgorithm.ComputeHash(buffer);
            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
