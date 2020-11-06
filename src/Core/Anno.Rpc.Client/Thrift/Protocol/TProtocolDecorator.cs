using System;

namespace Thrift.Protocol
{
    /// <summary>
    /// <see cref="TProtocolDecorator"/> forwards all requests to an enclosed <see cref="TProtocol"/> instance,
    /// providing a way to author concise concrete decorator subclasses. While it has
    /// no abstract methods, it is marked abstract as a reminder that by itself,
    /// it does not modify the behaviour of the enclosed <see cref="TProtocol"/>.
    /// <para/>
    /// See p.175 of Design Patterns (by Gamma et al.)
    /// </summary>
    /// <seealso cref="TMultiplexedProtocol"/>
    public abstract class TProtocolDecorator : TProtocol
    {
        private TProtocol WrappedProtocol;

        /// <summary>
        /// Encloses the specified protocol.
        /// </summary>
        /// <param name="protocol">All operations will be forward to this protocol.  Must be non-null.</param>
        public TProtocolDecorator(TProtocol protocol)
            : base(protocol.Transport)
        {

            WrappedProtocol = protocol;
        }

        public override void WriteMessageBegin(TMessage tMessage)
        {
            WrappedProtocol.WriteMessageBegin(tMessage);
        }

        public override void WriteMessageEnd()
        {
            WrappedProtocol.WriteMessageEnd();
        }

        public override void WriteStructBegin(TStruct tStruct)
        {
            WrappedProtocol.WriteStructBegin(tStruct);
        }

        public override void WriteStructEnd()
        {
            WrappedProtocol.WriteStructEnd();
        }

        public override void WriteFieldBegin(TField tField)
        {
            WrappedProtocol.WriteFieldBegin(tField);
        }

        public override void WriteFieldEnd()
        {
            WrappedProtocol.WriteFieldEnd();
        }

        public override void WriteFieldStop()
        {
            WrappedProtocol.WriteFieldStop();
        }

        public override void WriteMapBegin(TMap tMap)
        {
            WrappedProtocol.WriteMapBegin(tMap);
        }

        public override void WriteMapEnd()
        {
            WrappedProtocol.WriteMapEnd();
        }

        public override void WriteListBegin(TList tList)
        {
            WrappedProtocol.WriteListBegin(tList);
        }

        public override void WriteListEnd()
        {
            WrappedProtocol.WriteListEnd();
        }

        public override void WriteSetBegin(TSet tSet)
        {
            WrappedProtocol.WriteSetBegin(tSet);
        }

        public override void WriteSetEnd()
        {
            WrappedProtocol.WriteSetEnd();
        }

        public override void WriteBool(Boolean b)
        {
            WrappedProtocol.WriteBool(b);
        }

        public override void WriteByte(SByte b)
        {
            WrappedProtocol.WriteByte(b);
        }

        public override void WriteI16(Int16 i)
        {
            WrappedProtocol.WriteI16(i);
        }

        public override void WriteI32(Int32 i)
        {
            WrappedProtocol.WriteI32(i);
        }

        public override void WriteI64(Int64 l)
        {
            WrappedProtocol.WriteI64(l);
        }

        public override void WriteDouble(Double v)
        {
            WrappedProtocol.WriteDouble(v);
        }

        public override void WriteString(String s)
        {
            WrappedProtocol.WriteString(s);
        }

        public override void WriteBinary(Byte[] bytes)
        {
            WrappedProtocol.WriteBinary(bytes);
        }

        public override TMessage ReadMessageBegin()
        {
            return WrappedProtocol.ReadMessageBegin();
        }

        public override void ReadMessageEnd()
        {
            WrappedProtocol.ReadMessageEnd();
        }

        public override TStruct ReadStructBegin()
        {
            return WrappedProtocol.ReadStructBegin();
        }

        public override void ReadStructEnd()
        {
            WrappedProtocol.ReadStructEnd();
        }

        public override TField ReadFieldBegin()
        {
            return WrappedProtocol.ReadFieldBegin();
        }

        public override void ReadFieldEnd()
        {
            WrappedProtocol.ReadFieldEnd();
        }

        public override TMap ReadMapBegin()
        {
            return WrappedProtocol.ReadMapBegin();
        }

        public override void ReadMapEnd()
        {
            WrappedProtocol.ReadMapEnd();
        }

        public override TList ReadListBegin()
        {
            return WrappedProtocol.ReadListBegin();
        }

        public override void ReadListEnd()
        {
            WrappedProtocol.ReadListEnd();
        }

        public override TSet ReadSetBegin()
        {
            return WrappedProtocol.ReadSetBegin();
        }

        public override void ReadSetEnd()
        {
            WrappedProtocol.ReadSetEnd();
        }

        public override Boolean ReadBool()
        {
            return WrappedProtocol.ReadBool();
        }

        public override SByte ReadByte()
        {
            return WrappedProtocol.ReadByte();
        }

        public override Int16 ReadI16()
        {
            return WrappedProtocol.ReadI16();
        }

        public override Int32 ReadI32()
        {
            return WrappedProtocol.ReadI32();
        }

        public override Int64 ReadI64()
        {
            return WrappedProtocol.ReadI64();
        }

        public override Double ReadDouble()
        {
            return WrappedProtocol.ReadDouble();
        }

        public override String ReadString()
        {
            return WrappedProtocol.ReadString();
        }

        public override Byte[] ReadBinary()
        {
            return WrappedProtocol.ReadBinary();
        }
    }

}
