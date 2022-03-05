using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Anno.Loader
{
    internal static class IocFilter
    {
        /// <summary>
        /// Ioc过滤器
        /// </summary>
        internal static List<Func<Type, bool>> Filters = new List<Func<Type, bool>>();
    }
}
