using System;
using System.Text;
using Thrift.Transport;

namespace Thrift.Protocol
{
    public class TBinaryProtocol : TProtocol
    {
        protected const UInt32 VERSION_MASK = 0xffff0000;
        protected const UInt32 VERSION_1 = 0x80010000;

        protected Boolean strictRead_ = false;
        protected Boolean strictWrite_ = true;

        #region BinaryProtocol Factory

        public class Factory : TProtocolFactory
        {
            protected Boolean strictRead_ = false;
            protected Boolean strictWrite_ = true;

            public Factory()
                : this(false, true)
            {
            }

            public Factory(Boolean strictRead, Boolean strictWrite)
            {
                strictRead_ = strictRead;
                strictWrite_ = strictWrite;
            }

            public TProtocol GetProtocol(TTransport trans)
            {
                return new TBinaryProtocol(trans, strictRead_, strictWrite_);
            }
        }

        #endregion

        public TBinaryProtocol(TTransport trans)
            : this(trans, false, true)
        {
        }

        public TBinaryProtocol(TTransport trans, Boolean strictRead, Boolean strictWrite)
            : base(trans)
        {
            strictRead_ = strictRead;
            strictWrite_ = strictWrite;
        }

        #region Write Methods

        public override void WriteMessageBegin(TMessage message)
        {
            if (strictWrite_)
            {
                var version = VERSION_1 | (UInt32)(message.Type);
                WriteI32((Int32)version);
                WriteString(message.Name);
                WriteI32(message.SeqID);
            }
            else
            {
                WriteString(message.Name);
                WriteByte((SByte)message.Type);
                WriteI32(message.SeqID);
            }
        }

        public override void WriteMessageEnd()
        {
        }

        public override void WriteStructBegin(TStruct struc)
        {
        }

        public override void WriteStructEnd()
        {
        }

        public override void WriteFieldBegin(TField field)
        {
            WriteByte((SByte)field.Type);
            WriteI16(field.ID);
        }

        public override void WriteFieldEnd()
        {
        }

        public override void WriteFieldStop()
        {
            WriteByte((SByte)TType.Stop);
        }

        public override void WriteMapBegin(TMap map)
        {
            WriteByte((SByte)map.KeyType);
            WriteByte((SByte)map.ValueType);
            WriteI32(map.Count);
        }

        public override void WriteMapEnd()
        {
        }

        public override void WriteListBegin(TList list)
        {
            WriteByte((SByte)list.ElementType);
            WriteI32(list.Count);
        }

        public override void WriteListEnd()
        {
        }

        public override void WriteSetBegin(TSet set)
        {
            WriteByte((SByte)set.ElementType);
            WriteI32(set.Count);
        }

        public override void WriteSetEnd()
        {
        }

        public override void WriteBool(Boolean b)
        {
            WriteByte(b ? (SByte)1 : (SByte)0);
        }

        private readonly Byte[] bout = new Byte[1];
        public override void WriteByte(SByte b)
        {
            bout[0] = (Byte)b;
            Transport.Write(bout, 0, 1);
        }

        private readonly Byte[] i16out = new Byte[2];
        public override void WriteI16(Int16 s)
        {
            i16out[0] = (Byte)(0xff & (s >> 8));
            i16out[1] = (Byte)(0xff & s);
            Transport.Write(i16out, 0, 2);
        }

        private readonly Byte[] i32out = new Byte[4];
        public override void WriteI32(Int32 i32)
        {
            i32out[0] = (Byte)(0xff & (i32 >> 24));
            i32out[1] = (Byte)(0xff & (i32 >> 16));
            i32out[2] = (Byte)(0xff & (i32 >> 8));
            i32out[3] = (Byte)(0xff & i32);
            Transport.Write(i32out, 0, 4);
        }

        private readonly Byte[] i64out = new Byte[8];
        public override void WriteI64(Int64 i64)
        {
            i64out[0] = (Byte)(0xff & (i64 >> 56));
            i64out[1] = (Byte)(0xff & (i64 >> 48));
            i64out[2] = (Byte)(0xff & (i64 >> 40));
            i64out[3] = (Byte)(0xff & (i64 >> 32));
            i64out[4] = (Byte)(0xff & (i64 >> 24));
            i64out[5] = (Byte)(0xff & (i64 >> 16));
            i64out[6] = (Byte)(0xff & (i64 >> 8));
            i64out[7] = (Byte)(0xff & i64);
            Transport.Write(i64out, 0, 8);
        }

        public override void WriteDouble(Double d)
        {
            WriteI64(BitConverter.DoubleToInt64Bits(d));
        }

        public override void WriteBinary(Byte[] b)
        {
            WriteI32(b.Length);
            Transport.Write(b, 0, b.Length);
        }

        #endregion

        #region ReadMethods

        public override TMessage ReadMessageBegin()
        {
            var message = new TMessage();
            var size = ReadI32();
            if (size < 0)
            {
                var version = (UInt32)size & VERSION_MASK;
                if (version != VERSION_1)
                {
                    throw new TProtocolException(TProtocolException.BAD_VERSION, "Bad version in ReadMessageBegin: " + version);
                }
                message.Type = (TMessageType)(size & 0x000000ff);
                message.Name = ReadString();
                message.SeqID = ReadI32();
            }
            else
            {
                if (strictRead_)
                {
                    throw new TProtocolException(TProtocolException.BAD_VERSION, "Missing version in readMessageBegin, old client?");
                }
                message.Name = ReadStringBody(size);
                message.Type = (TMessageType)ReadByte();
                message.SeqID = ReadI32();
            }
            return message;
        }

        public override void ReadMessageEnd()
        {
        }

        public override TStruct ReadStructBegin()
        {
            return new TStruct();
        }

        public override void ReadStructEnd()
        {
        }

        public override TField ReadFieldBegin()
        {
            var field = new TField
            {
                Type = (TType)ReadByte()
            };

            if (field.Type != TType.Stop)
            {
                field.ID = ReadI16();
            }

            return field;
        }

        public override void ReadFieldEnd()
        {
        }

        public override TMap ReadMapBegin()
        {
            var map = new TMap
            {
                KeyType = (TType)ReadByte(),
                ValueType = (TType)ReadByte(),
                Count = ReadI32()
            };

            return map;
        }

        public override void ReadMapEnd()
        {
        }

        public override TList ReadListBegin()
        {
            var list = new TList
            {
                ElementType = (TType)ReadByte(),
                Count = ReadI32()
            };

            return list;
        }

        public override void ReadListEnd()
        {
        }

        public override TSet ReadSetBegin()
        {
            var set = new TSet
            {
                ElementType = (TType)ReadByte(),
                Count = ReadI32()
            };

            return set;
        }

        public override void ReadSetEnd()
        {
        }

        public override Boolean ReadBool()
        {
            return ReadByte() == 1;
        }

        private readonly Byte[] bin = new Byte[1];
        public override SByte ReadByte()
        {
            ReadAll(bin, 0, 1);
            return (SByte)bin[0];
        }

        private readonly Byte[] i16in = new Byte[2];
        public override Int16 ReadI16()
        {
            ReadAll(i16in, 0, 2);
            return (Int16)(((i16in[0] & 0xff) << 8) | ((i16in[1] & 0xff)));
        }

        private readonly Byte[] i32in = new Byte[4];
        public override Int32 ReadI32()
        {
            ReadAll(i32in, 0, 4);
            return ((i32in[0] & 0xff) << 24) | ((i32in[1] & 0xff) << 16) | ((i32in[2] & 0xff) << 8) | ((i32in[3] & 0xff));
        }

#pragma warning disable 675

        private readonly Byte[] i64in = new Byte[8];
        public override Int64 ReadI64()
        {
            ReadAll(i64in, 0, 8);
            unchecked
            {
                return
                    ((Int64)(i64in[0] & 0xff) << 56) |
                    ((Int64)(i64in[1] & 0xff) << 48) |
                    ((Int64)(i64in[2] & 0xff) << 40) |
                    ((Int64)(i64in[3] & 0xff) << 32) |
                    ((Int64)(i64in[4] & 0xff) << 24) |
                    ((Int64)(i64in[5] & 0xff) << 16) |
                    ((Int64)(i64in[6] & 0xff) << 8) |
                    i64in[7] & 0xff;
            }
        }

#pragma warning restore 675

        public override Double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadI64());
        }

        public override Byte[] ReadBinary()
        {
            var size = ReadI32();
            var buf = new Byte[size];
            Transport.ReadAll(buf, 0, size);
            return buf;
        }
        private String ReadStringBody(Int32 size)
        {
            var buf = new Byte[size];
            Transport.ReadAll(buf, 0, size);
            return Encoding.UTF8.GetString(buf, 0, buf.Length);
        }

        private Int32 ReadAll(Byte[] buf, Int32 off, Int32 len)
        {
            return Transport.ReadAll(buf, off, len);
        }

        #endregion
    }
}