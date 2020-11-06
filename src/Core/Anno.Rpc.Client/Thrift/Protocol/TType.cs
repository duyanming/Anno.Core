using System;

namespace Thrift.Protocol
{
    public enum TType : Byte
    {
        Stop = 0,
        Void = 1,
        Bool = 2,
        Byte = 3,
        Double = 4,
        I16 = 6,
        I32 = 8,
        I64 = 10,
        String = 11,
        Struct = 12,
        Map = 13,
        Set = 14,
        List = 15
    }
}