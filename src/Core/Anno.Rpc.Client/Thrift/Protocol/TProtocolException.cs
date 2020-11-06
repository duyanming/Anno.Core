using System;

namespace Thrift.Protocol
{
    public class TProtocolException : TException
    {
        public const Int32 UNKNOWN = 0;
        public const Int32 INVALID_DATA = 1;
        public const Int32 NEGATIVE_SIZE = 2;
        public const Int32 SIZE_LIMIT = 3;
        public const Int32 BAD_VERSION = 4;
        public const Int32 NOT_IMPLEMENTED = 5;
        public const Int32 DEPTH_LIMIT = 6;

        protected Int32 type_ = UNKNOWN;

        public TProtocolException()
            : base()
        {
        }

        public TProtocolException(Int32 type, Exception inner = null)
            : base(String.Empty, inner)
        {
            type_ = type;
        }

        public TProtocolException(Int32 type, String message, Exception inner = null)
            : base(message, inner)
        {
            type_ = type;
        }

        public TProtocolException(String message, Exception inner = null)
            : base(message, inner)
        {
        }

        public Int32 getType() => type_;
    }
}