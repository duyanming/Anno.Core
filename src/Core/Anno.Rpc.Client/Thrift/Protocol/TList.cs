using System;

namespace Thrift.Protocol
{
    public struct TList
    {
        public TList(TType elementType, Int32 count)
            : this()
        {
            ElementType = elementType;
            Count = count;
        }

        public TType ElementType { get; set; }

        public Int32 Count { get; set; }
    }
}