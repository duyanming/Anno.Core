using System;

namespace Thrift.Protocol
{
    public struct TSet
    {
        public TSet(TType elementType, Int32 count)
            : this()
        {
            ElementType = elementType;
            Count = count;
        }

        public TSet(TList list)
            : this(list.ElementType, list.Count)
        {
        }

        public TType ElementType { get; set; }

        public Int32 Count { get; set; }
    }
}