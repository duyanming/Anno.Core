using System;

namespace Thrift.Protocol
{
    public struct TField
    {
        public TField(String name, TType type, Int16 id)
            : this()
        {
            Name = name;
            Type = type;
            ID = id;
        }

        public String Name { get; set; }

        public TType Type { get; set; }

        public Int16 ID { get; set; }
    }
}