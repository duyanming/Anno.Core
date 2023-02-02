using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Anno.EngineData.Filters;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Anno.EngineData
{
    public static class AnnoBootstrap
    {
        /// <summary>
        /// 插件启动配置
        /// </summary>
        /// <param name="iocAction">用于用户自定义做依赖注入</param>
        public static void Bootstrap(Action iocAction, Loader.IocType iocType
#if NETSTANDARD
            , IServiceCollection services = null
#endif
            )
        {
            Const.SettingService.InitConfig();
            PreConfigurationBootstrap();

            Loader.IocLoader.RegisterIoc(iocType
#if NETSTANDARD
                , services
#endif
                );
            iocAction?.Invoke();
            Loader.IocLoader.Build();
            var bootstraps = Loader.IocLoader.Resolve<IEnumerable<IPlugsConfigurationBootstrap>>();
            if (bootstraps != null)
            {
                foreach (var plugsConfigurationBootstrap in bootstraps)
                {
                    plugsConfigurationBootstrap.ConfigurationBootstrap();
                }
            }
            BuilderRouterInfo();
        }
        /// <summary>
        /// IOC注入之前插件事件
        /// </summary>
        private static void PreConfigurationBootstrap()
        {
            foreach (var svc in Const.Assemblys.Dic)
            {
                GetDependedTypesAssemblies(svc.Value);
            }
        }
        /// <summary>
        /// 查找依赖
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        static void GetDependedTypesAssemblies(Assembly assembly)
        {
            List<Assembly> assemblies = new List<Assembly>();
            var type = assembly.GetTypes().FirstOrDefault(t => typeof(IPlugsConfigurationBootstrap).IsAssignableFrom(t));
            if (type == null)
            {
                return;
            }
            var obj = Activator.CreateInstance(type);
            type.GetMethod("PreConfigurationBootstrap")?.Invoke(obj, null);
            var dependsOn = type.GetCustomAttributes<DependsOnAttribute>().FirstOrDefault();
            if (dependsOn != null)
            {
                foreach (var module in dependsOn.DependedTypes)
                {
                    if (Const.Assemblys.Dic.Values.ToList().Exists(s => s == module.Assembly) || Const.Assemblys.DependedTypes.Exists(s => s == module.Assembly))
                    {
                        continue;
                    }
                    Const.Assemblys.DependedTypes.Add(module.Assembly);
                    assemblies.Add(module.Assembly);
                }
            }
            if (assemblies.Count() > 0)
            {
                foreach (var module in assemblies)
                {
                    GetDependedTypesAssemblies(module);
                }
            }
        }
        /// <summary>
        /// 构建路由信息
        /// </summary>
        static void BuilderRouterInfo()
        {
            var baseModuleType = typeof(BaseModule);
            foreach (var svc in Const.Assemblys.Dic)
            {
                svc.Value.GetTypes().Where(x => x.GetTypeInfo().IsClass && !x.GetTypeInfo().IsAbstract && !x.GetTypeInfo().IsInterface
                && baseModuleType.IsAssignableFrom(x)).ToList().ForEach(t =>
                   {
                       var methods = t.GetMethods().Where(m => !m.DeclaringType.Equals(baseModuleType) && !m.DeclaringType.Equals(typeof(object)) && m.IsPublic && !m.IsAbstract && !m.IsConstructor && !m.IsVirtual);
                       foreach (var method in methods)
                       {
                           Routing.RoutInfo routInfo = new Routing.RoutInfo()
                           {
                               RoutMethod = method,
                               RoutModuleType = t
                           };
                           #region Authorization Filters
                           /*
                          * 全局过滤器
                          */
                           routInfo.AuthorizationFilters.AddRange(Routing.Routing.GlobalAuthorizationFilters);
                           /*
                           * 模块过滤器
                           */
                           routInfo.AuthorizationFilters.AddRange(t.GetAnnoCustomAttributes<IAuthorizationFilter>());
                           /*
                           * 方法过滤器
                           */
                           routInfo.AuthorizationFilters.AddRange(method.GetAnnoCustomAttributes<IAuthorizationFilter>());
                           #endregion
                           #region Action Filters
                           /*
                           * 全局过滤器
                           */
                           routInfo.ActionFilters.AddRange(Routing.Routing.GlobalActionFilters);
                           /*
                           * 模块过滤器
                           */
                           routInfo.ActionFilters.AddRange(routInfo.RoutModuleType.GetAnnoCustomAttributes<IActionFilter>());
                           /*
                           * 方法过滤器
                           */
                           routInfo.ActionFilters.AddRange(routInfo.RoutMethod.GetAnnoCustomAttributes<IActionFilter>());

                           #endregion
                           #region Exception Filters
                           /*
                          * 方法过滤器
                          */
                           routInfo.ExceptionFilters.AddRange(routInfo.RoutMethod.GetAnnoCustomAttributes<IExceptionFilter>());
                           /*
                           * 模块过滤器
                           */
                           routInfo.ExceptionFilters.AddRange(routInfo.RoutModuleType.GetAnnoCustomAttributes<IExceptionFilter>());
                           /*
                            * 全局过滤器
                            */
                           routInfo.ExceptionFilters.AddRange(Routing.Routing.GlobalExceptionFilters);
                           #endregion
                           #region CacheMiddleware 
                           /*
                            * 全局Cache
                           */
                           routInfo.CacheMiddleware.AddRange(Routing.Routing.GlobalCacheMiddleware);
                           /*
                            * 模块Cache
                           */
                           routInfo.CacheMiddleware.AddRange(routInfo.RoutModuleType.GetCustomAttributes<Cache.CacheMiddlewareAttribute>());
                           /*
                            * 方法Cache
                           */
                           routInfo.CacheMiddleware.AddRange(routInfo.RoutMethod.GetCustomAttributes<Cache.CacheMiddlewareAttribute>());
                           #endregion
                           var key = $"{t.FullName}/{method.Name}";
                           if (Routing.Routing.Router.ContainsKey(key))
                           {
                               Routing.Routing.Router[key] = routInfo;
                           }
                           else
                           {
                               Routing.Routing.Router.TryAdd(key, routInfo);
                           }
                       }
                   });
            }
        }
        static bool IsAssignableFrom(Type type, string baseTypeFullName)
        {
            bool success = false;
            if (type == null)
            {
                success = false;
            }
            else if (type.FullName == baseTypeFullName)
            {
                success = true;
            }
            else if (type.BaseType != null)
            {
                success = IsAssignableFrom(type.BaseType, baseTypeFullName);
            }
            return success;
        }

        static List<T> GetAnnoCustomAttributes<T>(this Type type) where T : IFilterMetadata
        {
            var annoObjCustomAttributes = type.GetCustomAttributes(true);
            List<T> annoCustomAttributes = new List<T>();
            if (annoObjCustomAttributes != null && annoObjCustomAttributes.Length > 0)
            {
                for (int i = 0; i < annoObjCustomAttributes.Length; i++)
                {
                    var annoObjCustom = annoObjCustomAttributes[i];
                    if (annoObjCustom.GetType().GetInterface(typeof(T).Name, true) != null)
                    {
                        annoCustomAttributes.Add((T)annoObjCustom);
                    }
                }
            }
            return annoCustomAttributes;
        }

        static List<T> GetAnnoCustomAttributes<T>(this MethodInfo type) where T : IFilterMetadata
        {
            var annoObjCustomAttributes = type.GetCustomAttributes(true);
            List<T> annoCustomAttributes = new List<T>();
            if (annoObjCustomAttributes != null && annoObjCustomAttributes.Length > 0)
            {
                for (int i = 0; i < annoObjCustomAttributes.Length; i++)
                {
                    var annoObjCustom = annoObjCustomAttributes[i];
                    if (annoObjCustom.GetType().GetInterface(typeof(T).Name, true) != null)
                    {
                        annoCustomAttributes.Add((T)annoObjCustom);
                    }
                }
            }
            return annoCustomAttributes;
        }
    }
}
