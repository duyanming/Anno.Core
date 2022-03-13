#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;

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
                        if (t.GetCustomAttribute<NotInInjectAttribute>() != null)
                        {
                            return;
                        }
                        if (IocFilter.Filters.Count > 0)
                        {
                            foreach (var filter in IocFilter.Filters)
                            {
                                if (!filter(t))
                                {
                                    return;
                                }
                            }
                        }

                        ServiceLifetime lifetime = ServiceLifetime.Transient;
                        if (t.GetCustomAttribute<SingletonAttribute>() != null)
                        {
                            lifetime = ServiceLifetime.Singleton;
                        }
                        if (t.GetCustomAttribute<ScopedAttribute>() != null)
                        {
                            lifetime = ServiceLifetime.Scoped;
                        }
                        var interfaces = t.GetInterfaces();
                        if (IsAssignableFrom(t, "Anno.EngineData.BaseModule")
                        || interfaces.ToList().Exists(i => i.Name == "IFilterMetadata")
                        || interfaces.Length <= 0)
                        {
                            switch (lifetime) { 
                            case ServiceLifetime.Singleton:
                                    services.AddSingleton(t);
                                    break;
                                case ServiceLifetime.Scoped:
                                    services.AddScoped(t);
                                    break;
                                default:
                                    services.AddTransient(t);
                                    break;
                            }
                        }
                        else if (!interfaces.ToList().Exists(i => i.Name == "IEntity"))
                        {
                            interfaces.ToList().ForEach(_interface =>
                            {
                                if (!(interfaces.Length == 1 && interfaces[0].Equals(typeof(IAsyncStateMachine))))
                                {
                                    switch (lifetime)
                                    {
                                        case ServiceLifetime.Singleton:
                                            services.AddSingleton(_interface,t);
                                            break;
                                        case ServiceLifetime.Scoped:
                                            services.AddScoped(_interface,t);
                                            break;
                                        default:
                                            services.AddTransient(_interface, t);
                                            break;
                                    }
                                }
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
                success = IsAssignableFrom(type.BaseType, baseTypeFullName);
            }
            return success;
        }
    }
}
#endif
