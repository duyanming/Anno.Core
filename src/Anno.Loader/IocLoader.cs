using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Anno.Loader
{
    /// <summary>
    /// IOC加载器
    /// </summary>
    public static class IocLoader
    {
        private static IContainer _autofacContainer;
        private static IServiceProvider _dIServiceProvider;

        private static ContainerBuilder _autoFacServicesCollection = null;
#if NETSTANDARD
        private static IServiceCollection _diServicesCollection = null;
        public static IServiceCollection DiServices = new ServiceCollection();
#endif
        private static IocType iocType = IocType.Autofac;
        /// <summary>
        /// 使用的IOC容器
        /// </summary>
        public static IocType IocType => iocType;

        /// <summary>
        /// 初始化IOC
        /// </summary>
        public static void RegisterIoc(IocType iocType = IocType.Autofac)
        {
            if (_autoFacServicesCollection == null
#if NETSTANDARD
                && _diServicesCollection == null
#endif
                )
            {
                IocLoader.iocType = iocType;
                if (iocType == IocType.Autofac)
                {
                    _autoFacServicesCollection = new ContainerBuilder();
                    _autoFacServicesCollection.RegisterModule(new Loader.AutofacModule());
                }
#if NETSTANDARD
                else if (iocType == IocType.DependencyInjection)
                {
                    _diServicesCollection = DiServices;
                }
#endif
                else
                {
                    throw new ArgumentOutOfRangeException("IocType 参数类型不正确！");
                }
            }
        }
        public static ContainerBuilder GetAutoFacContainerBuilder()
        {
            if (iocType == IocType.Autofac)
            {
                if (_autoFacServicesCollection == null)
                {
                    RegisterIoc(IocType.Autofac);
                }
                return _autoFacServicesCollection;
            }
            throw new TypeAccessException("IocType Must Be Autofac!");
        }
#if NETSTANDARD
        public static IServiceCollection GetServiceDescriptors()
        {
            if (iocType == IocType.DependencyInjection)
            {
                if (_diServicesCollection == null)
                {
                    RegisterIoc(IocType.DependencyInjection);
                }
                return _diServicesCollection;
            }
            throw new TypeAccessException("IocType Must Be DependencyInjection!");
        }
#endif
        public static void Build()
        {
            if (iocType == IocType.Autofac)
            {
                _autofacContainer = _autoFacServicesCollection
                    .Build(Autofac.Builder.ContainerBuildOptions.None);
            }
#if NETSTANDARD
            else if (iocType == IocType.DependencyInjection)
            {
                _dIServiceProvider = _diServicesCollection.UseDependencyInjection().BuildServiceProvider();
            }
#endif
            else
            {
                throw new ArgumentOutOfRangeException("IocType 参数类型不正确！");
            }
        }
        //public static IContainer AutoFacContainer => _autofacContainer;

        //public static IServiceProvider DiIServiceProvider => _dIServiceProvider;
        /// <summary>
        /// AutoFac 获取实例对象
        /// </summary>
        /// <typeparam name="T">实例对象</typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
        {
            if (iocType == IocType.Autofac)
            {
                return _autofacContainer.Resolve<T>();
            }
#if NETSTANDARD
            else if (iocType == IocType.DependencyInjection)
            {
                return _dIServiceProvider.GetService<T>();
            }
#endif
            else
            {
                throw new Exception("请先初始化 RegisterIoc！");
            }
        }
        public static T Resolve<T>(Type serviceType) where T : class
        {
            if (iocType == IocType.Autofac)
            {
                return _autofacContainer.Resolve(serviceType) as T;
            }
            else if (iocType == IocType.DependencyInjection)
            {
                return _dIServiceProvider.GetService(serviceType) as T;
            }
            else
            {
                throw new Exception("请先初始化 RegisterIoc！");
            }
        }
        public static dynamic CreateScope()
        {
            if (iocType == IocType.Autofac)
            {
                return _autofacContainer.BeginLifetimeScope();
            }
#if NETSTANDARD
            else if (iocType == IocType.DependencyInjection)
            {
                return _dIServiceProvider.CreateScope();
            }
#endif
            else
            {
                throw new Exception("请先初始化 RegisterIoc！");
            }
        }
        /// <summary>
        /// IOC容器注入自定义过滤器
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static bool AddFilter(Func<Type, bool> filter)
        {
            bool success = false;
            if (filter != null)
            {
                IocFilter.Filters.Add(filter);
            }
            return success;
        }
    }
    /// <summary>
    /// Ioc类型
    /// </summary>
    public enum IocType
    {
        Autofac,
        DependencyInjection
    }
}
