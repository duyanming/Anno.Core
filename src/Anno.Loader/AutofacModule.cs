using System;
using System.Linq;
using Autofac;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Anno.Loader
{
    public class AutofacModule : Autofac.Module
    {
        //注意以下写法
        //builder.RegisterType<GuidTransientAnnoService>().As<IGuidTransientAnnoService>();
        //builder.RegisterType<GuidScopedAnnoService>().As<IGuidScopedAnnoService>().InstancePerLifetimeScope();
        //builder.RegisterType<GuidSingletonAnnoService>().As<IGuidSingletonAnnoService>().SingleInstance();

        protected override void Load(ContainerBuilder builder)
        {
            // The generic ILogger<TCategoryName> service was added to the ServiceCollection by ASP.NET Core.
            // It was then registered with Autofac using the Populate method in ConfigureServices.

            //builder.Register(c => new ValuesService(c.Resolve<ILogger<ValuesService>>()))
            //    .As<IValuesService>()
            //    .InstancePerLifetimeScope();
            // builder.RegisterType<BaseRepository>().As<IBaseRepository>();
            Const.AppSettings.IocDll.Distinct().ToList().ForEach(d =>
            {
                RegisterAssembly(builder, Const.Assemblys.Dic[d]);
            });
            foreach (var assembly in Const.Assemblys.DependedTypes)
            {
                RegisterAssembly(builder, assembly);
            }
        }
        private void RegisterAssembly(ContainerBuilder builder, Assembly assembly)
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
                       string lifetime = "Transient";
                       if (t.GetCustomAttribute<SingletonAttribute>() != null)
                       {
                           lifetime = "Singleton";
                       }
                       if (t.GetCustomAttribute<ScopedAttribute>() != null)
                       {
                           lifetime = "Scoped";
                       }
                       var interfaces = t.GetInterfaces();
                       if (IsAssignableFrom(t, "Anno.EngineData.BaseModule")
                       || interfaces.ToList().Exists(i => i.Name == "IFilterMetadata")
                       || interfaces.Length <= 0)
                       {
                           if (t.IsGenericType)
                           {
                               switch (lifetime)
                               {
                                   case "Singleton":
                                       builder.RegisterGeneric(t).SingleInstance();
                                       break;
                                   case "Scoped":
                                       builder.RegisterGeneric(t).InstancePerLifetimeScope();
                                       break;
                                   default:
                                       builder.RegisterGeneric(t);
                                       break;
                               }
                           }
                           else
                           {
                               switch (lifetime)
                               {
                                   case "Singleton":
                                       builder.RegisterType(t).SingleInstance();
                                       break;
                                   case "Scoped":
                                       builder.RegisterType(t).InstancePerLifetimeScope();
                                       break;
                                   default:
                                       builder.RegisterType(t);
                                       break;
                               }
                           }
                       }
                       else if (!interfaces.ToList().Exists(i => i.Name == "IEntity"))
                       {
                           if (t.IsGenericType)
                           {
                               if (!(interfaces.Length == 1 && interfaces[0].Equals(typeof(IAsyncStateMachine))))
                               {
                                   switch (lifetime)
                                   {
                                       case "Singleton":
                                           builder.RegisterGeneric(t).As(interfaces).SingleInstance();
                                           break;
                                       case "Scoped":
                                           builder.RegisterGeneric(t).As(interfaces).InstancePerLifetimeScope();
                                           break;
                                       default:
                                           builder.RegisterGeneric(t).As(interfaces);
                                           break;
                                   }
                               }
                           }
                           else
                           {
                               switch (lifetime)
                               {
                                   case "Singleton":
                                       builder.RegisterType(t).As(interfaces).SingleInstance();
                                       break;
                                   case "Scoped":
                                       builder.RegisterType(t).As(interfaces).InstancePerLifetimeScope();
                                       break;
                                   default:
                                       builder.RegisterType(t).As(interfaces);
                                       break;
                               }
                           }
                       }
                   });
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
        private static bool CheckIfAnonymousType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.Name.StartsWith("<>c__"))
            {
                return true;
            }
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}
