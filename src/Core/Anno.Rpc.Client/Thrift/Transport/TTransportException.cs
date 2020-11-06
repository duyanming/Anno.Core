using System;

namespace Thrift.Transport
{
    public class TTransportException : TException
    {
        protected ExceptionType type;

        public TTransportException()
            : base()
        {
        }

        public TTransportException(ExceptionType type)
            : this()
        {
            this.type = type;
        }

        public TTransportException(ExceptionType type, String message, Exception inner = null)
            : base(message, inner)
        {
            this.type = type;
        }

        public TTransportException(String message, Exception inner = null)
            : base(message, inner)
        {
        }

        public ExceptionType Type => type;

        public enum ExceptionType
        {
            Unknown,
            NotOpen,
            AlreadyOpen,
            TimedOut,
            EndOfFile,
            Interrupted
        }
    }
}