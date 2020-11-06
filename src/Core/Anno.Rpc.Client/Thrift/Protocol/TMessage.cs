using System;

namespace Thrift.Protocol
{
    public struct TMessage
    {
        public TMessage(String name, TMessageType type, Int32 seqid)
            : this()
        {
            Name = name;
            Type = type;
            SeqID = seqid;
        }

        public String Name { get; set; }

        public TMessageType Type { get; set; }

        public Int32 SeqID { get; set; }
    }
}