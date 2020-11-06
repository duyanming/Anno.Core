#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Anno.Loader
{
    internal static class ServiceCollectionExtensions
    {
        private static bool loader = false;
        /// <summary>
        /// 微软自带DI
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection UseDependencyInjection(this IServiceCollection services)
        {
            //已经加载过的不再重新加载
            if (loader)
            {
                return services;
            }
            Const.AppSettings.IocDll.Distinct().ToList().ForEach(d =>
            {
                RegisterAssembly(services, Const.Assemblys.Dic[d]);
            });
            foreach (var assembly in Const.Assemblys.DependedTypes)
            {
                RegisterAssembly(services, assembly);
            }
            loader = true;
            return services;
        }
        private static void RegisterAssembly(IServiceCollection services, Assembly assembly)
        {
            assembly.GetTypes().Where(x => x.GetTypeInfo().IsClass && !x.GetTypeInfo().IsAbstract && !x.GetTypeInfo().IsInterface).ToList().ForEach(
                    t =>
                    {
                        var interfaces = t.GetInterfaces();
                        if (IsAssignableFrom(t, "Anno.EngineData.BaseModule")
                        || interfaces.ToList().Exists(i => i.Name == "IFilterMetadata")
                        || interfaces.Length <= 0)
                        {
                            services.AddTransient(t);
                        }
                        else if (!interfaces.ToList().Exists(i => i.Name == "IEntity"))
                        {
                            interfaces.ToList().ForEach(_interface =>
                            {
                                services.AddTransient(_interface, t);
                            });
                        }

                    }
                );
        }

        internal static bool IsAssignableFrom(Type type, string baseTypeFullName)
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
                success = IsAssignableFrom(type.BaseType,baseTypeFullName);
            }
            return success;
        }
    }
}
#endif
