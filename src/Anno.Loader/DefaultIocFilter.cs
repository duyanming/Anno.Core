using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Loader
{
    /// <summary>
    /// 默认IOC过滤器
    /// </summary>
    public static class DefaultIocFilter
    {
        /// <summary>
        /// 具有公共无参构造函数的过滤器
        /// </summary>
        public static bool HasPublicParameterlessCtorFilter(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }


        /// <summary>
        /// 标准判断：是实例类（排除委托、值类型、接口、抽象类）
        /// </summary>
        public static bool IsNormalClass(Type type)
        {
            return type.IsClass
                && !type.IsValueType
                && !type.IsInterface
                && !type.IsAbstract
                && !typeof(Delegate).IsAssignableFrom(type);
        }
    }
}
