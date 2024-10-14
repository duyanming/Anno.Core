using Anno.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData
{
    internal static class ObjectExtend
    {
        public static Case<T> Case<T>(this T value, params T[] asserts)
        {
            return new Case<T>(value, asserts);
        }
        /// <summary>
        /// string 转化为对应类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public static object Case(this Type valueType, string value)
        {
            object parameterTypeValue = null;
            if (value == null)
            {
                return parameterTypeValue;
            }
            valueType
                .Case(typeof(Guid)).Do(() => parameterTypeValue = Guid.Parse(value))
                .Case(typeof(string)).Do(() => parameterTypeValue = value)
                .Case(typeof(DateTime)).Do(() => parameterTypeValue = Convert.ToDateTime(value))
                .Case(typeof(bool)).Do(() => parameterTypeValue = Convert.ToBoolean(value))
                .Case(typeof(short)).Do(() => parameterTypeValue = Convert.ToInt16(value))
                .Case(typeof(int)).Do(() => parameterTypeValue = Convert.ToInt32(value))
                .Case(typeof(long)).Do(() => parameterTypeValue = Convert.ToInt64(value))
                .Case(typeof(byte)).Do(() => parameterTypeValue = Convert.ToByte(value))
                .Case(typeof(float)).Do(() => parameterTypeValue = Convert.ToSingle(value))
                .Case(typeof(decimal)).Do(() => parameterTypeValue = Convert.ToDecimal(value))
                .Case(typeof(double)).Do(() => parameterTypeValue = Convert.ToDouble(value))
                .Case(typeof(ushort)).Do(() => parameterTypeValue = Convert.ToUInt16(value))
                .Case(typeof(uint)).Do(() => parameterTypeValue = Convert.ToUInt32(value))
                .Case(typeof(ulong)).Do(() => parameterTypeValue = Convert.ToUInt64(value))
                .Case(typeof(char)).Do(() => parameterTypeValue = Convert.ToChar(value))
                .Case(typeof(sbyte)).Do(() => parameterTypeValue = Convert.ToSByte(value))
                .Case(typeof(object)).Do(() => parameterTypeValue = value)
                ;
            if (parameterTypeValue == null)
            {
                try
                {
                    parameterTypeValue = Convert.ChangeType(value, valueType);
                }
                catch (Exception ex)
                {
                    Log.Log.Error(ex, typeof(ObjectExtend));
                }
            }
            return parameterTypeValue;
        }
    }
    internal class Case<T>
    {
        private T _value;

        private ICollection<T> _asserts;

        public Case(T value, T[] asserts)
        {
            _value = value;
            _asserts = asserts;
        }

        public T Do(Action action)
        {
            if (_asserts.Contains(_value))
            {
                action();
            }
            return _value;
        }
    }
}
