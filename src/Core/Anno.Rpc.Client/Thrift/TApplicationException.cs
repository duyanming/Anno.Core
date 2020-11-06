using System;
using Thrift.Protocol;

namespace Thrift
{
    public class TApplicationException : TException
    {
        public TApplicationException() { }

        public TApplicationException(ExceptionType type) => Type = type;

        public TApplicationException(ExceptionType type, String message)
            : base(message, null) // TApplicationException is serializable, but we never serialize InnerException
        {
            Type = type;
        }

        public static TApplicationException Read(TProtocol iprot)
        {
            TField field;

            String message = null;
            var type = ExceptionType.Unknown;

            iprot.ReadStructBegin();
            while (true)
            {
                field = iprot.ReadFieldBegin();
                if (field.Type == TType.Stop)
                {
                    break;
                }

                switch (field.ID)
                {
                    case 1:
                        if (field.Type == TType.String)
                        {
                            message = iprot.ReadString();
                        }
                        else
                        {
                            TProtocolUtil.Skip(iprot, field.Type);
                        }
                        break;
                    case 2:
                        if (field.Type == TType.I32)
                        {
                            type = (ExceptionType)iprot.ReadI32();
                        }
                        else
                        {
                            TProtocolUtil.Skip(iprot, field.Type);
                        }
                        break;
                    default:
                        TProtocolUtil.Skip(iprot, field.Type);
                        break;
                }

                iprot.ReadFieldEnd();
            }

            iprot.ReadStructEnd();

            return new TApplicationException(type, message);
        }

        public void Write(TProtocol oprot)
        {
            var struc = new TStruct("TApplicationException");
            var field = new TField();

            oprot.WriteStructBegin(struc);

            if (!String.IsNullOrEmpty(Message))
            {
                field.Name = "message";
                field.Type = TType.String;
                field.ID = 1;
                oprot.WriteFieldBegin(field);
                oprot.WriteString(Message);
                oprot.WriteFieldEnd();
            }

            field.Name = "type";
            field.Type = TType.I32;
            field.ID = 2;
            oprot.WriteFieldBegin(field);
            oprot.WriteI32((Int32)Type);
            oprot.WriteFieldEnd();
            oprot.WriteFieldStop();
            oprot.WriteStructEnd();
        }

        public enum ExceptionType
        {
            Unknown,
            UnknownMethod,
            InvalidMessageType,
            WrongMethodName,
            BadSequenceID,
            MissingResult,
            InternalError,
            ProtocolError,
            InvalidTransform,
            InvalidProtocol,
            UnsupportedClientType
        }

        public ExceptionType Type { get; private set; }
    }
}