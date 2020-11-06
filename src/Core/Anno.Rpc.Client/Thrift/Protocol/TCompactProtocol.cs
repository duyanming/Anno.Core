using System;
using System.Collections.Generic;
using System.Text;
using Thrift.Transport;

namespace Thrift.Protocol
{
    public class TCompactProtocol : TProtocol
    {
        private static TStruct ANONYMOUS_STRUCT = new TStruct("");
        private static TField TSTOP = new TField("", TType.Stop, 0);

        private static Byte[] ttypeToCompactType = new Byte[16];

        private const Byte PROTOCOL_ID = 0x82;
        private const Byte VERSION = 1;
        private const Byte VERSION_MASK = 0x1f; // 0001 1111
        private const Byte TYPE_MASK = 0xE0; // 1110 0000
        private const Byte TYPE_BITS = 0x07; // 0000 0111
        private const Int32 TYPE_SHIFT_AMOUNT = 5;

        /// <summary>
        /// All of the on-wire type codes.
        /// </summary>
        private static class Types
        {
            public const Byte STOP = 0x00;
            public const Byte BOOLEAN_TRUE = 0x01;
            public const Byte BOOLEAN_FALSE = 0x02;
            public const Byte BYTE = 0x03;
            public const Byte I16 = 0x04;
            public const Byte I32 = 0x05;
            public const Byte I64 = 0x06;
            public const Byte DOUBLE = 0x07;
            public const Byte BINARY = 0x08;
            public const Byte LIST = 0x09;
            public const Byte SET = 0x0A;
            public const Byte MAP = 0x0B;
            public const Byte STRUCT = 0x0C;
        }

        /// <summary>
        /// Used to keep track of the last field for the current and previous structs,
        /// so we can do the delta stuff.
        /// </summary>
        private Stack<Int16> lastField_ = new Stack<Int16>(15);

        private Int16 lastFieldId_ = 0;

        /// <summary>
        /// If we encounter a boolean field begin, save the TField here so it can
        /// have the value incorporated.
        /// </summary>
        private TField? booleanField_;

        /// <summary>
        /// If we Read a field header, and it's a boolean field, save the boolean
        /// value here so that ReadBool can use it.
        /// </summary>
        private Boolean? boolValue_;


        #region CompactProtocol Factory

        public class Factory : TProtocolFactory
        {
            public Factory() { }

            public TProtocol GetProtocol(TTransport trans)
            {
                return new TCompactProtocol(trans);
            }
        }

        #endregion

        public TCompactProtocol(TTransport trans)
            : base(trans)
        {
            ttypeToCompactType[(Int32)TType.Stop] = Types.STOP;
            ttypeToCompactType[(Int32)TType.Bool] = Types.BOOLEAN_TRUE;
            ttypeToCompactType[(Int32)TType.Byte] = Types.BYTE;
            ttypeToCompactType[(Int32)TType.I16] = Types.I16;
            ttypeToCompactType[(Int32)TType.I32] = Types.I32;
            ttypeToCompactType[(Int32)TType.I64] = Types.I64;
            ttypeToCompactType[(Int32)TType.Double] = Types.DOUBLE;
            ttypeToCompactType[(Int32)TType.String] = Types.BINARY;
            ttypeToCompactType[(Int32)TType.List] = Types.LIST;
            ttypeToCompactType[(Int32)TType.Set] = Types.SET;
            ttypeToCompactType[(Int32)TType.Map] = Types.MAP;
            ttypeToCompactType[(Int32)TType.Struct] = Types.STRUCT;
        }

        public void reset()
        {
            lastField_.Clear();
            lastFieldId_ = 0;
        }

        #region Write Methods

        /// <summary>
        /// Writes a byte without any possibility of all that field header nonsense.
        /// Used internally by other writing methods that know they need to Write a byte.
        /// </summary>
        private readonly Byte[] byteDirectBuffer = new Byte[1];

        private void WriteByteDirect(Byte b)
        {
            byteDirectBuffer[0] = b;
            Transport.Write(byteDirectBuffer);
        }

        /// <summary>
        /// Writes a byte without any possibility of all that field header nonsense.
        /// </summary>
        private void WriteByteDirect(Int32 n)
        {
            WriteByteDirect((Byte)n);
        }

        /// <summary>
        /// Write an i32 as a varint. Results in 1-5 bytes on the wire.
        /// TODO: make a permanent buffer like WriteVarint64?
        /// </summary>
        readonly Byte[] i32buf = new Byte[5];

        private void WriteVarint32(UInt32 n)
        {
            var idx = 0;
            while (true)
            {
                if ((n & ~0x7F) == 0)
                {
                    i32buf[idx++] = (Byte)n;
                    // WriteByteDirect((byte)n);
                    break;
                    // return;
                }
                else
                {
                    i32buf[idx++] = (Byte)((n & 0x7F) | 0x80);
                    // WriteByteDirect((byte)((n & 0x7F) | 0x80));
                    n >>= 7;
                }
            }
            Transport.Write(i32buf, 0, idx);
        }

        /// <summary>
        /// Write a message header to the wire. Compact Protocol messages contain the
        /// protocol version so we can migrate forwards in the future if need be.
        /// </summary>
        public override void WriteMessageBegin(TMessage message)
        {
            WriteByteDirect(PROTOCOL_ID);
            WriteByteDirect((Byte)((VERSION & VERSION_MASK) | ((((UInt32)message.Type) << TYPE_SHIFT_AMOUNT) & TYPE_MASK)));
            WriteVarint32((UInt32)message.SeqID);
            WriteString(message.Name);
        }

        /// <summary>
        /// Write a struct begin. This doesn't actually put anything on the wire. We
        /// use it as an opportunity to put special placeholder markers on the field
        /// stack so we can get the field id deltas correct.
        /// </summary>
        public override void WriteStructBegin(TStruct strct)
        {
            lastField_.Push(lastFieldId_);
            lastFieldId_ = 0;
        }

        /// <summary>
        /// Write a struct end. This doesn't actually put anything on the wire. We use
        /// this as an opportunity to pop the last field from the current struct off
        /// of the field stack.
        /// </summary>
        public override void WriteStructEnd()
        {
            lastFieldId_ = lastField_.Pop();
        }

        /// <summary>
        /// Write a field header containing the field id and field type. If the
        /// difference between the current field id and the last one is small (&lt; 15),
        /// then the field id will be encoded in the 4 MSB as a delta. Otherwise, the
        /// field id will follow the type header as a zigzag varint.
        /// </summary>
        public override void WriteFieldBegin(TField field)
        {
            if (field.Type == TType.Bool)
            {
                // we want to possibly include the value, so we'll wait.
                booleanField_ = field;
            }
            else
            {
                WriteFieldBeginInternal(field, 0xFF);
            }
        }

        /// <summary>
        /// The workhorse of WriteFieldBegin. It has the option of doing a
        /// 'type override' of the type header. This is used specifically in the
        /// boolean field case.
        /// </summary>
        private void WriteFieldBeginInternal(TField field, Byte typeOverride)
        {
            // short lastField = lastField_.Pop();

            // if there's a type override, use that.
            var typeToWrite = typeOverride == 0xFF ? getCompactType(field.Type) : typeOverride;

            // check if we can use delta encoding for the field id
            if (field.ID > lastFieldId_ && field.ID - lastFieldId_ <= 15)
            {
                // Write them together
                WriteByteDirect((field.ID - lastFieldId_) << 4 | typeToWrite);
            }
            else
            {
                // Write them separate
                WriteByteDirect(typeToWrite);
                WriteI16(field.ID);
            }

            lastFieldId_ = field.ID;
            // lastField_.push(field.id);
        }

        /// <summary>
        /// Write the STOP symbol so we know there are no more fields in this struct.
        /// </summary>
        public override void WriteFieldStop()
        {
            WriteByteDirect(Types.STOP);
        }

        /// <summary>
        /// Write a map header. If the map is empty, omit the key and value type
        /// headers, as we don't need any additional information to skip it.
        /// </summary>
        public override void WriteMapBegin(TMap map)
        {
            if (map.Count == 0)
            {
                WriteByteDirect(0);
            }
            else
            {
                WriteVarint32((UInt32)map.Count);
                WriteByteDirect(getCompactType(map.KeyType) << 4 | getCompactType(map.ValueType));
            }
        }

        /// <summary>
        /// Write a list header.
        /// </summary>
        public override void WriteListBegin(TList list)
        {
            WriteCollectionBegin(list.ElementType, list.Count);
        }

        /// <summary>
        /// Write a set header.
        /// </summary>
        public override void WriteSetBegin(TSet set)
        {
            WriteCollectionBegin(set.ElementType, set.Count);
        }

        /// <summary>
        /// Write a boolean value. Potentially, this could be a boolean field, in
        /// which case the field header info isn't written yet. If so, decide what the
        /// right type header is for the value and then Write the field header.
        /// Otherwise, Write a single byte.
        /// </summary>
        public override void WriteBool(Boolean b)
        {
            if (booleanField_ != null)
            {
                // we haven't written the field header yet
                WriteFieldBeginInternal(booleanField_.Value, b ? Types.BOOLEAN_TRUE : Types.BOOLEAN_FALSE);
                booleanField_ = null;
            }
            else
            {
                // we're not part of a field, so just Write the value.
                WriteByteDirect(b ? Types.BOOLEAN_TRUE : Types.BOOLEAN_FALSE);
            }
        }

        /// <summary>
        /// Write a byte. Nothing to see here!
        /// </summary>
        public override void WriteByte(SByte b)
        {
            WriteByteDirect((Byte)b);
        }

        /// <summary>
        /// Write an I16 as a zigzag varint.
        /// </summary>
        public override void WriteI16(Int16 i16)
        {
            WriteVarint32(intToZigZag(i16));
        }

        /// <summary>
        /// Write an i32 as a zigzag varint.
        /// </summary>
        public override void WriteI32(Int32 i32)
        {
            WriteVarint32(intToZigZag(i32));
        }

        /// <summary>
        /// Write an i64 as a zigzag varint.
        /// </summary>
        public override void WriteI64(Int64 i64)
        {
            WriteVarint64(longToZigzag(i64));
        }

        /// <summary>
        /// Write a double to the wire as 8 bytes.
        /// </summary>
        public override void WriteDouble(Double dub)
        {
            var data = new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            fixedLongToBytes(BitConverter.DoubleToInt64Bits(dub), data, 0);
            Transport.Write(data);
        }

        /// <summary>
        /// Write a string to the wire with a varint size preceding.
        /// </summary>
        public override void WriteString(String str)
        {
            var bytes = UTF8Encoding.UTF8.GetBytes(str);
            WriteBinary(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Write a byte array, using a varint for the size.
        /// </summary>
        public override void WriteBinary(Byte[] bin)
        {
            WriteBinary(bin, 0, bin.Length);
        }

        private void WriteBinary(Byte[] buf, Int32 offset, Int32 length)
        {
            WriteVarint32((UInt32)length);
            Transport.Write(buf, offset, length);
        }

        //
        // These methods are called by structs, but don't actually have any wire
        // output or purpose.
        //

        public override void WriteMessageEnd() { }
        public override void WriteMapEnd() { }
        public override void WriteListEnd() { }
        public override void WriteSetEnd() { }
        public override void WriteFieldEnd() { }

        //
        // Internal writing methods
        //

        /// <summary>
        /// Abstract method for writing the start of lists and sets. List and sets on
        /// the wire differ only by the type indicator.
        /// </summary>
        protected void WriteCollectionBegin(TType elemType, Int32 size)
        {
            if (size <= 14)
            {
                WriteByteDirect(size << 4 | getCompactType(elemType));
            }
            else
            {
                WriteByteDirect(0xf0 | getCompactType(elemType));
                WriteVarint32((UInt32)size);
            }
        }

        /// <summary>
        /// Write an i64 as a varint. Results in 1-10 bytes on the wire.
        /// </summary>
        readonly Byte[] varint64out = new Byte[10];
        private void WriteVarint64(UInt64 n)
        {
            var idx = 0;
            while (true)
            {
                if ((n & ~(UInt64)0x7FL) == 0)
                {
                    varint64out[idx++] = (Byte)n;
                    break;
                }
                else
                {
                    varint64out[idx++] = ((Byte)((n & 0x7F) | 0x80));
                    n >>= 7;
                }
            }
            Transport.Write(varint64out, 0, idx);
        }

        /// <summary>
        /// Convert l into a zigzag long. This allows negative numbers to be
        /// represented compactly as a varint.
        /// </summary>
        private UInt64 longToZigzag(Int64 n)
        {
            return (UInt64)(n << 1) ^ (UInt64)(n >> 63);
        }

        /// <summary>
        /// Convert n into a zigzag int. This allows negative numbers to be
        /// represented compactly as a varint.
        /// </summary>
        private UInt32 intToZigZag(Int32 n)
        {
            return (UInt32)(n << 1) ^ (UInt32)(n >> 31);
        }

        /// <summary>
        /// Convert a long into little-endian bytes in buf starting at off and going
        /// until off+7.
        /// </summary>
        private void fixedLongToBytes(Int64 n, Byte[] buf, Int32 off)
        {
            buf[off + 0] = (Byte)(n & 0xff);
            buf[off + 1] = (Byte)((n >> 8) & 0xff);
            buf[off + 2] = (Byte)((n >> 16) & 0xff);
            buf[off + 3] = (Byte)((n >> 24) & 0xff);
            buf[off + 4] = (Byte)((n >> 32) & 0xff);
            buf[off + 5] = (Byte)((n >> 40) & 0xff);
            buf[off + 6] = (Byte)((n >> 48) & 0xff);
            buf[off + 7] = (Byte)((n >> 56) & 0xff);
        }

        #endregion

        #region ReadMethods

        /// <summary>
        /// Read a message header.
        /// </summary>
        public override TMessage ReadMessageBegin()
        {
            var protocolId = (Byte)ReadByte();
            if (protocolId != PROTOCOL_ID)
            {
                throw new TProtocolException("Expected protocol id " + PROTOCOL_ID.ToString("X") + " but got " + protocolId.ToString("X"));
            }
            var versionAndType = (Byte)ReadByte();
            var version = (Byte)(versionAndType & VERSION_MASK);
            if (version != VERSION)
            {
                throw new TProtocolException("Expected version " + VERSION + " but got " + version);
            }
            var type = (Byte)((versionAndType >> TYPE_SHIFT_AMOUNT) & TYPE_BITS);
            var seqid = (Int32)ReadVarint32();
            var messageName = ReadString();
            return new TMessage(messageName, (TMessageType)type, seqid);
        }

        /// <summary>
        /// Read a struct begin. There's nothing on the wire for this, but it is our
        /// opportunity to push a new struct begin marker onto the field stack.
        /// </summary>
        public override TStruct ReadStructBegin()
        {
            lastField_.Push(lastFieldId_);
            lastFieldId_ = 0;
            return ANONYMOUS_STRUCT;
        }

        /// <summary>
        /// Doesn't actually consume any wire data, just removes the last field for
        /// this struct from the field stack.
        /// </summary>
        public override void ReadStructEnd()
        {
            // consume the last field we Read off the wire.
            lastFieldId_ = lastField_.Pop();
        }

        /// <summary>
        /// Read a field header off the wire.
        /// </summary>
        public override TField ReadFieldBegin()
        {
            var type = (Byte)ReadByte();

            // if it's a stop, then we can return immediately, as the struct is over.
            if (type == Types.STOP)
            {
                return TSTOP;
            }

            Int16 fieldId;

            // mask off the 4 MSB of the type header. it could contain a field id delta.
            var modifier = (Int16)((type & 0xf0) >> 4);
            if (modifier == 0)
            {
                // not a delta. look ahead for the zigzag varint field id.
                fieldId = ReadI16();
            }
            else
            {
                // has a delta. add the delta to the last Read field id.
                fieldId = (Int16)(lastFieldId_ + modifier);
            }

            var field = new TField("", getTType((Byte)(type & 0x0f)), fieldId);

            // if this happens to be a boolean field, the value is encoded in the type
            if (isBoolType(type))
            {
                // save the boolean value in a special instance variable.
                boolValue_ = (Byte)(type & 0x0f) == Types.BOOLEAN_TRUE ? true : false;
            }

            // push the new field onto the field stack so we can keep the deltas going.
            lastFieldId_ = field.ID;
            return field;
        }

        /// <summary>
        /// Read a map header off the wire. If the size is zero, skip Reading the key
        /// and value type. This means that 0-length maps will yield TMaps without the
        /// "correct" types.
        /// </summary>
        public override TMap ReadMapBegin()
        {
            var size = (Int32)ReadVarint32();
            var keyAndValueType = size == 0 ? (Byte)0 : (Byte)ReadByte();
            return new TMap(getTType((Byte)(keyAndValueType >> 4)), getTType((Byte)(keyAndValueType & 0xf)), size);
        }

        /// <summary>
        /// Read a list header off the wire. If the list size is 0-14, the size will
        /// be packed into the element type header. If it's a longer list, the 4 MSB
        /// of the element type header will be 0xF, and a varint will follow with the
        /// true size.
        /// </summary>
        public override TList ReadListBegin()
        {
            var size_and_type = (Byte)ReadByte();
            var size = (size_and_type >> 4) & 0x0f;
            if (size == 15)
            {
                size = (Int32)ReadVarint32();
            }
            var type = getTType(size_and_type);
            return new TList(type, size);
        }

        /// <summary>
        /// Read a set header off the wire. If the set size is 0-14, the size will
        /// be packed into the element type header. If it's a longer set, the 4 MSB
        /// of the element type header will be 0xF, and a varint will follow with the
        /// true size.
        /// </summary>
        public override TSet ReadSetBegin()
        {
            return new TSet(ReadListBegin());
        }

        /// <summary>
        /// Read a boolean off the wire. If this is a boolean field, the value should
        /// already have been Read during ReadFieldBegin, so we'll just consume the
        /// pre-stored value. Otherwise, Read a byte.
        /// </summary>
        public override Boolean ReadBool()
        {
            if (boolValue_ != null)
            {
                var result = boolValue_.Value;
                boolValue_ = null;
                return result;
            }
            return ReadByte() == Types.BOOLEAN_TRUE;
        }

        readonly Byte[] byteRawBuf = new Byte[1];
        /// <summary>
        /// Read a single byte off the wire. Nothing interesting here.
        /// </summary>
        public override SByte ReadByte()
        {
            Transport.ReadAll(byteRawBuf, 0, 1);
            return (SByte)byteRawBuf[0];
        }

        /// <summary>
        /// Read an i16 from the wire as a zigzag varint.
        /// </summary>
        public override Int16 ReadI16()
        {
            return (Int16)zigzagToInt(ReadVarint32());
        }

        /// <summary>
        /// Read an i32 from the wire as a zigzag varint.
        /// </summary>
        public override Int32 ReadI32()
        {
            return zigzagToInt(ReadVarint32());
        }

        /// <summary>
        /// Read an i64 from the wire as a zigzag varint.
        /// </summary>
        public override Int64 ReadI64()
        {
            return zigzagToLong(ReadVarint64());
        }

        /// <summary>
        /// No magic here - just Read a double off the wire.
        /// </summary>
        public override Double ReadDouble()
        {
            var longBits = new Byte[8];
            Transport.ReadAll(longBits, 0, 8);
            return BitConverter.Int64BitsToDouble(bytesToLong(longBits));
        }

        /// <summary>
        /// Reads a byte[] (via ReadBinary), and then UTF-8 decodes it.
        /// </summary>
        public override String ReadString()
        {
            var length = (Int32)ReadVarint32();

            if (length == 0)
            {
                return "";
            }

            return Encoding.UTF8.GetString(ReadBinary(length));
        }

        /// <summary>
        /// Read a byte[] from the wire.
        /// </summary>
        public override Byte[] ReadBinary()
        {
            var length = (Int32)ReadVarint32();
            if (length == 0) return new Byte[0];

            var buf = new Byte[length];
            Transport.ReadAll(buf, 0, length);
            return buf;
        }

        /// <summary>
        /// Read a byte[] of a known length from the wire.
        /// </summary>
        private Byte[] ReadBinary(Int32 length)
        {
            if (length == 0) return new Byte[0];

            var buf = new Byte[length];
            Transport.ReadAll(buf, 0, length);
            return buf;
        }

        //
        // These methods are here for the struct to call, but don't have any wire
        // encoding.
        //
        public override void ReadMessageEnd() { }
        public override void ReadFieldEnd() { }
        public override void ReadMapEnd() { }
        public override void ReadListEnd() { }
        public override void ReadSetEnd() { }

        //
        // Internal Reading methods
        //

        /// <summary>
        /// Read an i32 from the wire as a varint. The MSB of each byte is set
        /// if there is another byte to follow. This can Read up to 5 bytes.
        /// </summary>
        private UInt32 ReadVarint32()
        {
            UInt32 result = 0;
            var shift = 0;
            while (true)
            {
                var b = (Byte)ReadByte();
                result |= (UInt32)(b & 0x7f) << shift;
                if ((b & 0x80) != 0x80) break;
                shift += 7;
            }
            return result;
        }

        /// <summary>
        /// Read an i64 from the wire as a proper varint. The MSB of each byte is set
        /// if there is another byte to follow. This can Read up to 10 bytes.
        /// </summary>
        private UInt64 ReadVarint64()
        {
            var shift = 0;
            UInt64 result = 0;
            while (true)
            {
                var b = (Byte)ReadByte();
                result |= (UInt64)(b & 0x7f) << shift;
                if ((b & 0x80) != 0x80) break;
                shift += 7;
            }

            return result;
        }

        #endregion

        //
        // encoding helpers
        //

        /// <summary>
        /// Convert from zigzag int to int.
        /// </summary>
        private Int32 zigzagToInt(UInt32 n)
        {
            return (Int32)(n >> 1) ^ (-(Int32)(n & 1));
        }

        /// <summary>
        /// Convert from zigzag long to long.
        /// </summary>
        private Int64 zigzagToLong(UInt64 n)
        {
            return (Int64)(n >> 1) ^ (-(Int64)(n & 1));
        }

        /// <summary>
        /// Note that it's important that the mask bytes are long literals,
        /// otherwise they'll default to ints, and when you shift an int left 56 bits,
        /// you just get a messed up int.
        /// </summary>
        private Int64 bytesToLong(Byte[] bytes)
        {
            return
              ((bytes[7] & 0xffL) << 56) |
              ((bytes[6] & 0xffL) << 48) |
              ((bytes[5] & 0xffL) << 40) |
              ((bytes[4] & 0xffL) << 32) |
              ((bytes[3] & 0xffL) << 24) |
              ((bytes[2] & 0xffL) << 16) |
              ((bytes[1] & 0xffL) << 8) |
              ((bytes[0] & 0xffL));
        }

        //
        // type testing and converting
        //

        private Boolean isBoolType(Byte b)
        {
            var lowerNibble = b & 0x0f;
            return lowerNibble == Types.BOOLEAN_TRUE || lowerNibble == Types.BOOLEAN_FALSE;
        }

        /// <summary>
        /// Given a TCompactProtocol.Types constant, convert it to its corresponding
        /// TType value.
        /// </summary>
        private TType getTType(Byte type)
        {
            switch ((Byte)(type & 0x0f))
            {
                case Types.STOP:
                    return TType.Stop;
                case Types.BOOLEAN_FALSE:
                case Types.BOOLEAN_TRUE:
                    return TType.Bool;
                case Types.BYTE:
                    return TType.Byte;
                case Types.I16:
                    return TType.I16;
                case Types.I32:
                    return TType.I32;
                case Types.I64:
                    return TType.I64;
                case Types.DOUBLE:
                    return TType.Double;
                case Types.BINARY:
                    return TType.String;
                case Types.LIST:
                    return TType.List;
                case Types.SET:
                    return TType.Set;
                case Types.MAP:
                    return TType.Map;
                case Types.STRUCT:
                    return TType.Struct;
                default:
                    throw new TProtocolException("don't know what type: " + (Byte)(type & 0x0f));
            }
        }

        /// <summary>
        /// Given a TType value, find the appropriate TCompactProtocol.Types constant.
        /// </summary>
        private Byte getCompactType(TType ttype) => ttypeToCompactType[(Int32)ttype];
    }
}