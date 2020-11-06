using System;

namespace Thrift.Protocol
{
    public struct TMap
    {
        public TMap(TType keyType, TType valueType, Int32 count)
            : this()
        {
            KeyType = keyType;
            ValueType = valueType;
            Count = count;
        }

        public TType KeyType { get; set; }

        public TType ValueType { get; set; }

        public Int32 Count { get; set; }
    }
}