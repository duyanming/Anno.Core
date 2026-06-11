using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anno.Loader
{
    /// <summary>
    /// 默认IOC过滤器
    /// </summary>
    public static class IocDefaultFilter
    {
        /// <summary>
        /// 组合过滤器：不是BaseModule、IFilterMetadata、IEntity类型且无public无参构造函数的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns>false 表示类型被过滤，true 表示类型通过过滤</returns>
        public static bool Filter(Type type)
        {
            var interfaces = type.GetInterfaces();
            if (!IsAssignableFrom(type, "Anno.EngineData.BaseModule")
                && !interfaces.ToList().Exists(i => i.Name == "IFilterMetadata")
                && !interfaces.ToList().Exists(i => i.Name == "IEntity"))
            {
                if (!HasPublicParameterlessCtor(type))
                {
                    return false;
                }
                if (!IsNormalClass(type))
                {
                    return false;
                }
            }
            return true;
        }

        // 标准判断：是实例类（排除委托、值类型、接口、抽象类）
        public static bool IsNormalClass(Type type)
        {
            return type.IsClass
                && !type.IsValueType
                && !type.IsInterface
                && !type.IsAbstract
                && !typeof(Delegate).IsAssignableFrom(type);
        }


        // 存在 public 无参构造函数
        public static bool HasPublicParameterlessCtor(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
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
