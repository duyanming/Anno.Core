using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Thrift.Transport;

namespace Thrift.Protocol
{
    /// <summary>
    /// JSON protocol implementation for thrift.
    /// <para/>
    /// This is a full-featured protocol supporting Write and Read.
    /// <para/>
    /// Please see the C++ class header for a detailed description of the
    /// protocol's wire format.
    /// <para/>
    /// Adapted from the Java version.
    /// </summary>
    public class TJSONProtocol : TProtocol
    {
        /// <summary>
        /// Factory for JSON protocol objects.
        /// </summary>
        public class Factory : TProtocolFactory
        {
            public TProtocol GetProtocol(TTransport trans)
            {
                return new TJSONProtocol(trans);
            }
        }

        private static readonly Byte[] COMMA = new Byte[] { (Byte)',' };
        private static readonly Byte[] COLON = new Byte[] { (Byte)':' };
        private static readonly Byte[] LBRACE = new Byte[] { (Byte)'{' };
        private static readonly Byte[] RBRACE = new Byte[] { (Byte)'}' };
        private static readonly Byte[] LBRACKET = new Byte[] { (Byte)'[' };
        private static readonly Byte[] RBRACKET = new Byte[] { (Byte)']' };
        private static readonly Byte[] QUOTE = new Byte[] { (Byte)'"' };
        private static readonly Byte[] BACKSLASH = new Byte[] { (Byte)'\\' };

        private readonly Byte[] ESCSEQ = new Byte[] { (Byte)'\\', (Byte)'u', (Byte)'0', (Byte)'0' };

        private const Int64 VERSION = 1;
        private readonly Byte[] JSON_CHAR_TABLE = {
    0,  0,  0,  0,  0,  0,  0,  0,(Byte)'b',(Byte)'t',(Byte)'n',  0,(Byte)'f',(Byte)'r',  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    1,  1,(Byte)'"',  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
  };

        private readonly Char[] ESCAPE_CHARS = "\"\\/bfnrt".ToCharArray();

        private readonly Byte[] ESCAPE_CHAR_VALS = {
    (Byte)'"', (Byte)'\\', (Byte)'/', (Byte)'\b', (Byte)'\f', (Byte)'\n', (Byte)'\r', (Byte)'\t',
  };

        private const Int32 DEF_STRING_SIZE = 16;

        private static readonly Byte[] NAME_BOOL = new Byte[] { (Byte)'t', (Byte)'f' };
        private static readonly Byte[] NAME_BYTE = new Byte[] { (Byte)'i', (Byte)'8' };
        private static readonly Byte[] NAME_I16 = new Byte[] { (Byte)'i', (Byte)'1', (Byte)'6' };
        private static readonly Byte[] NAME_I32 = new Byte[] { (Byte)'i', (Byte)'3', (Byte)'2' };
        private static readonly Byte[] NAME_I64 = new Byte[] { (Byte)'i', (Byte)'6', (Byte)'4' };
        private static readonly Byte[] NAME_DOUBLE = new Byte[] { (Byte)'d', (Byte)'b', (Byte)'l' };
        private static readonly Byte[] NAME_STRUCT = new Byte[] { (Byte)'r', (Byte)'e', (Byte)'c' };
        private static readonly Byte[] NAME_STRING = new Byte[] { (Byte)'s', (Byte)'t', (Byte)'r' };
        private static readonly Byte[] NAME_MAP = new Byte[] { (Byte)'m', (Byte)'a', (Byte)'p' };
        private static readonly Byte[] NAME_LIST = new Byte[] { (Byte)'l', (Byte)'s', (Byte)'t' };
        private static readonly Byte[] NAME_SET = new Byte[] { (Byte)'s', (Byte)'e', (Byte)'t' };

        private static Byte[] GetTypeNameForTypeID(TType typeID)
        {
            switch (typeID)
            {
                case TType.Bool:
                    return NAME_BOOL;
                case TType.Byte:
                    return NAME_BYTE;
                case TType.I16:
                    return NAME_I16;
                case TType.I32:
                    return NAME_I32;
                case TType.I64:
                    return NAME_I64;
                case TType.Double:
                    return NAME_DOUBLE;
                case TType.String:
                    return NAME_STRING;
                case TType.Struct:
                    return NAME_STRUCT;
                case TType.Map:
                    return NAME_MAP;
                case TType.Set:
                    return NAME_SET;
                case TType.List:
                    return NAME_LIST;
                default:
                    throw new TProtocolException(TProtocolException.NOT_IMPLEMENTED,
                                                 "Unrecognized type");
            }
        }

        private static TType GetTypeIDForTypeName(Byte[] name)
        {
            var result = TType.Stop;
            if (name.Length > 1)
            {
                switch (name[0])
                {
                    case (Byte)'d':
                        result = TType.Double;
                        break;
                    case (Byte)'i':
                        switch (name[1])
                        {
                            case (Byte)'8':
                                result = TType.Byte;
                                break;
                            case (Byte)'1':
                                result = TType.I16;
                                break;
                            case (Byte)'3':
                                result = TType.I32;
                                break;
                            case (Byte)'6':
                                result = TType.I64;
                                break;
                        }
                        break;
                    case (Byte)'l':
                        result = TType.List;
                        break;
                    case (Byte)'m':
                        result = TType.Map;
                        break;
                    case (Byte)'r':
                        result = TType.Struct;
                        break;
                    case (Byte)'s':
                        if (name[1] == (Byte)'t')
                        {
                            result = TType.String;
                        }
                        else if (name[1] == (Byte)'e')
                        {
                            result = TType.Set;
                        }
                        break;
                    case (Byte)'t':
                        result = TType.Bool;
                        break;
                }
            }
            if (result == TType.Stop)
            {
                throw new TProtocolException(TProtocolException.NOT_IMPLEMENTED,
                                             "Unrecognized type");
            }
            return result;
        }

        /// <summary>
        /// Base class for tracking JSON contexts that may require
        /// inserting/Reading additional JSON syntax characters
        /// This base context does nothing.
        /// </summary>
        protected class JSONBaseContext
        {
            protected TJSONProtocol proto;

            public JSONBaseContext(TJSONProtocol proto)
            {
                this.proto = proto;
            }

            public virtual void Write() { }

            public virtual void Read() { }

            public virtual Boolean EscapeNumbers() { return false; }
        }

        /// <summary>
        /// Context for JSON lists. Will insert/Read commas before each item except
        /// for the first one
        /// </summary>
        protected class JSONListContext : JSONBaseContext
        {
            public JSONListContext(TJSONProtocol protocol)
                : base(protocol)
            {

            }

            private Boolean first = true;

            public override void Write()
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    proto.Transport.Write(COMMA);
                }
            }

            public override void Read()
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    proto.ReadJSONSyntaxChar(COMMA);
                }
            }
        }

        /// <summary>
        /// Context for JSON records. Will insert/Read colons before the value portion
        /// of each record pair, and commas before each key except the first. In
        /// addition, will indicate that numbers in the key position need to be
        /// escaped in quotes (since JSON keys must be strings).
        /// </summary>
        protected class JSONPairContext : JSONBaseContext
        {
            public JSONPairContext(TJSONProtocol proto)
                : base(proto)
            {

            }

            private Boolean first = true;
            private Boolean colon = true;

            public override void Write()
            {
                if (first)
                {
                    first = false;
                    colon = true;
                }
                else
                {
                    proto.Transport.Write(colon ? COLON : COMMA);
                    colon = !colon;
                }
            }

            public override void Read()
            {
                if (first)
                {
                    first = false;
                    colon = true;
                }
                else
                {
                    proto.ReadJSONSyntaxChar(colon ? COLON : COMMA);
                    colon = !colon;
                }
            }

            public override Boolean EscapeNumbers()
            {
                return colon;
            }
        }

        /// <summary>
        /// Holds up to one byte from the transport
        /// </summary>
        protected class LookaheadReader
        {
            protected TJSONProtocol proto;

            public LookaheadReader(TJSONProtocol proto)
            {
                this.proto = proto;
            }

            private Boolean hasData;
            private readonly Byte[] data = new Byte[1];

            /// <summary>
            /// Return and consume the next byte to be Read, either taking it from the
            /// data buffer if present or getting it from the transport otherwise.
            /// </summary>
            public Byte Read()
            {
                if (hasData)
                {
                    hasData = false;
                }
                else
                {
                    proto.Transport.ReadAll(data, 0, 1);
                }
                return data[0];
            }

            /// <summary>
            /// Return the next byte to be Read without consuming, filling the data
            /// buffer if it has not been filled alReady.
            /// </summary>
            public Byte Peek()
            {
                if (!hasData)
                {
                    proto.Transport.ReadAll(data, 0, 1);
                }
                hasData = true;
                return data[0];
            }
        }

        // Default encoding
        protected Encoding utf8Encoding = UTF8Encoding.UTF8;

        // Stack of nested contexts that we may be in
        protected Stack<JSONBaseContext> contextStack = new Stack<JSONBaseContext>();

        // Current context that we are in
        protected JSONBaseContext context;

        // Reader that manages a 1-byte buffer
        protected LookaheadReader reader;

        /// <summary>
        /// Push a new JSON context onto the stack.
        /// </summary>
        protected void PushContext(JSONBaseContext c)
        {
            contextStack.Push(context);
            context = c;
        }

        /// <summary>
        /// Pop the last JSON context off the stack
        /// </summary>
        protected void PopContext()
        {
            context = contextStack.Pop();
        }

        /// <summary>
        /// TJSONProtocol Constructor
        /// </summary>
        public TJSONProtocol(TTransport trans)
            : base(trans)
        {
            context = new JSONBaseContext(this);
            reader = new LookaheadReader(this);
        }

        // Temporary buffer used by several methods
        private readonly Byte[] tempBuffer = new Byte[4];

        /// <summary>
        /// Read a byte that must match b[0]; otherwise an exception is thrown.
        /// Marked protected to avoid synthetic accessor in JSONListContext.Read
        /// and JSONPairContext.Read
        /// </summary>
        protected void ReadJSONSyntaxChar(Byte[] b)
        {
            var ch = reader.Read();
            if (ch != b[0])
            {
                throw new TProtocolException(TProtocolException.INVALID_DATA,
                                             "Unexpected character:" + (Char)ch);
            }
        }

        /// <summary>
        /// Convert a byte containing a hex char ('0'-'9' or 'a'-'f') into its
        /// corresponding hex value
        /// </summary>
        private static Byte HexVal(Byte ch)
        {
            if ((ch >= '0') && (ch <= '9'))
            {
                return (Byte)((Char)ch - '0');
            }
            else if ((ch >= 'a') && (ch <= 'f'))
            {
                ch += 10;
                return (Byte)((Char)ch - 'a');
            }
            else
            {
                throw new TProtocolException(TProtocolException.INVALID_DATA,
                                             "Expected hex character");
            }
        }

        /// <summary>
        /// Convert a byte containing a hex value to its corresponding hex character
        /// </summary>
        private static Byte HexChar(Byte val)
        {
            val &= 0x0F;
            if (val < 10)
            {
                return (Byte)((Char)val + '0');
            }
            else
            {
                val -= 10;
                return (Byte)((Char)val + 'a');
            }
        }

        /// <summary>
        /// Write the bytes in array buf as a JSON characters, escaping as needed
        /// </summary>
        private void WriteJSONString(Byte[] b)
        {
            context.Write();
            Transport.Write(QUOTE);
            var len = b.Length;
            for (var i = 0; i < len; i++)
            {
                if ((b[i] & 0x00FF) >= 0x30)
                {
                    if (b[i] == BACKSLASH[0])
                    {
                        Transport.Write(BACKSLASH);
                        Transport.Write(BACKSLASH);
                    }
                    else
                    {
                        Transport.Write(b, i, 1);
                    }
                }
                else
                {
                    tempBuffer[0] = JSON_CHAR_TABLE[b[i]];
                    if (tempBuffer[0] == 1)
                    {
                        Transport.Write(b, i, 1);
                    }
                    else if (tempBuffer[0] > 1)
                    {
                        Transport.Write(BACKSLASH);
                        Transport.Write(tempBuffer, 0, 1);
                    }
                    else
                    {
                        Transport.Write(ESCSEQ);
                        tempBuffer[0] = HexChar((Byte)(b[i] >> 4));
                        tempBuffer[1] = HexChar(b[i]);
                        Transport.Write(tempBuffer, 0, 2);
                    }
                }
            }
            Transport.Write(QUOTE);
        }

        /// <summary>
        /// Write out number as a JSON value. If the context dictates so, it will be
        /// wrapped in quotes to output as a JSON string.
        /// </summary>
        private void WriteJSONInteger(Int64 num)
        {
            context.Write();
            var str = num.ToString();

            var escapeNum = context.EscapeNumbers();
            if (escapeNum)
                Transport.Write(QUOTE);

            Transport.Write(utf8Encoding.GetBytes(str));

            if (escapeNum)
                Transport.Write(QUOTE);
        }

        /// <summary>
        /// Write out a double as a JSON value. If it is NaN or infinity or if the
        /// context dictates escaping, Write out as JSON string.
        /// </summary>
        private void WriteJSONDouble(Double num)
        {
            context.Write();
            var str = num.ToString("G17", CultureInfo.InvariantCulture);
            var special = false;

            switch (str[0])
            {
                case 'N': // NaN
                case 'I': // Infinity
                    special = true;
                    break;
                case '-':
                    if (str[1] == 'I')
                    { // -Infinity
                        special = true;
                    }
                    break;
            }

            var escapeNum = special || context.EscapeNumbers();

            if (escapeNum)
                Transport.Write(QUOTE);

            Transport.Write(utf8Encoding.GetBytes(str));

            if (escapeNum)
                Transport.Write(QUOTE);
        }
        /// <summary>
        /// Write out contents of byte array b as a JSON string with base-64 encoded
        /// data
        /// </summary>
        private void WriteJSONBase64(Byte[] b)
        {
            context.Write();
            Transport.Write(QUOTE);

            var len = b.Length;
            var off = 0;

            while (len >= 3)
            {
                // Encode 3 bytes at a time
                TBase64Utils.encode(b, off, 3, tempBuffer, 0);
                Transport.Write(tempBuffer, 0, 4);
                off += 3;
                len -= 3;
            }
            if (len > 0)
            {
                // Encode remainder
                TBase64Utils.encode(b, off, len, tempBuffer, 0);
                Transport.Write(tempBuffer, 0, len + 1);
            }

            Transport.Write(QUOTE);
        }

        private void WriteJSONObjectStart()
        {
            context.Write();
            Transport.Write(LBRACE);
            PushContext(new JSONPairContext(this));
        }

        private void WriteJSONObjectEnd()
        {
            PopContext();
            Transport.Write(RBRACE);
        }

        private void WriteJSONArrayStart()
        {
            context.Write();
            Transport.Write(LBRACKET);
            PushContext(new JSONListContext(this));
        }

        private void WriteJSONArrayEnd()
        {
            PopContext();
            Transport.Write(RBRACKET);
        }

        public override void WriteMessageBegin(TMessage message)
        {
            WriteJSONArrayStart();
            WriteJSONInteger(VERSION);

            var b = utf8Encoding.GetBytes(message.Name);
            WriteJSONString(b);

            WriteJSONInteger((Int64)message.Type);
            WriteJSONInteger(message.SeqID);
        }

        public override void WriteMessageEnd()
        {
            WriteJSONArrayEnd();
        }

        public override void WriteStructBegin(TStruct str)
        {
            WriteJSONObjectStart();
        }

        public override void WriteStructEnd()
        {
            WriteJSONObjectEnd();
        }

        public override void WriteFieldBegin(TField field)
        {
            WriteJSONInteger(field.ID);
            WriteJSONObjectStart();
            WriteJSONString(GetTypeNameForTypeID(field.Type));
        }

        public override void WriteFieldEnd()
        {
            WriteJSONObjectEnd();
        }

        public override void WriteFieldStop() { }

        public override void WriteMapBegin(TMap map)
        {
            WriteJSONArrayStart();
            WriteJSONString(GetTypeNameForTypeID(map.KeyType));
            WriteJSONString(GetTypeNameForTypeID(map.ValueType));
            WriteJSONInteger(map.Count);
            WriteJSONObjectStart();
        }

        public override void WriteMapEnd()
        {
            WriteJSONObjectEnd();
            WriteJSONArrayEnd();
        }

        public override void WriteListBegin(TList list)
        {
            WriteJSONArrayStart();
            WriteJSONString(GetTypeNameForTypeID(list.ElementType));
            WriteJSONInteger(list.Count);
        }

        public override void WriteListEnd()
        {
            WriteJSONArrayEnd();
        }

        public override void WriteSetBegin(TSet set)
        {
            WriteJSONArrayStart();
            WriteJSONString(GetTypeNameForTypeID(set.ElementType));
            WriteJSONInteger(set.Count);
        }

        public override void WriteSetEnd()
        {
            WriteJSONArrayEnd();
        }

        public override void WriteBool(Boolean b)
        {
            WriteJSONInteger(b ? 1 : 0);
        }

        public override void WriteByte(SByte b)
        {
            WriteJSONInteger(b);
        }

        public override void WriteI16(Int16 i16)
        {
            WriteJSONInteger(i16);
        }

        public override void WriteI32(Int32 i32)
        {
            WriteJSONInteger(i32);
        }

        public override void WriteI64(Int64 i64)
        {
            WriteJSONInteger(i64);
        }

        public override void WriteDouble(Double dub)
        {
            WriteJSONDouble(dub);
        }

        public override void WriteString(String str)
        {
            var b = utf8Encoding.GetBytes(str);
            WriteJSONString(b);
        }

        public override void WriteBinary(Byte[] bin)
        {
            WriteJSONBase64(bin);
        }

        /**
         * Reading methods.
         */

        /// <summary>
        /// Read in a JSON string, unescaping as appropriate.. Skip Reading from the
        /// context if skipContext is true.
        /// </summary>
        private Byte[] ReadJSONString(Boolean skipContext)
        {
            var buffer = new MemoryStream();
            var codeunits = new List<Char>();


            if (!skipContext)
            {
                context.Read();
            }
            ReadJSONSyntaxChar(QUOTE);
            while (true)
            {
                var ch = reader.Read();
                if (ch == QUOTE[0])
                {
                    break;
                }

                // escaped?
                if (ch != ESCSEQ[0])
                {
                    buffer.Write(new Byte[] { ch }, 0, 1);
                    continue;
                }

                // distinguish between \uXXXX and \?
                ch = reader.Read();
                if (ch != ESCSEQ[1])  // control chars like \n
                {
                    var off = Array.IndexOf(ESCAPE_CHARS, (Char)ch);
                    if (off == -1)
                    {
                        throw new TProtocolException(TProtocolException.INVALID_DATA,
                                                        "Expected control char");
                    }
                    ch = ESCAPE_CHAR_VALS[off];
                    buffer.Write(new Byte[] { ch }, 0, 1);
                    continue;
                }


                // it's \uXXXX
                Transport.ReadAll(tempBuffer, 0, 4);
                var wch = (Int16)((HexVal(tempBuffer[0]) << 12) +
                                  (HexVal(tempBuffer[1]) << 8) +
                                  (HexVal(tempBuffer[2]) << 4) +
                                   HexVal(tempBuffer[3]));
                if (Char.IsHighSurrogate((Char)wch))
                {
                    if (codeunits.Count > 0)
                    {
                        throw new TProtocolException(TProtocolException.INVALID_DATA,
                                                        "Expected low surrogate char");
                    }
                    codeunits.Add((Char)wch);
                }
                else if (Char.IsLowSurrogate((Char)wch))
                {
                    if (codeunits.Count == 0)
                    {
                        throw new TProtocolException(TProtocolException.INVALID_DATA,
                                                        "Expected high surrogate char");
                    }
                    codeunits.Add((Char)wch);
                    var tmp = utf8Encoding.GetBytes(codeunits.ToArray());
                    buffer.Write(tmp, 0, tmp.Length);
                    codeunits.Clear();
                }
                else
                {
                    var tmp = utf8Encoding.GetBytes(new Char[] { (Char)wch });
                    buffer.Write(tmp, 0, tmp.Length);
                }
            }


            if (codeunits.Count > 0)
            {
                throw new TProtocolException(TProtocolException.INVALID_DATA,
                                                "Expected low surrogate char");
            }

            return buffer.ToArray();
        }

        /// <summary>
        /// Return true if the given byte could be a valid part of a JSON number.
        /// </summary>
        private Boolean IsJSONNumeric(Byte b)
        {
            switch (b)
            {
                case (Byte)'+':
                case (Byte)'-':
                case (Byte)'.':
                case (Byte)'0':
                case (Byte)'1':
                case (Byte)'2':
                case (Byte)'3':
                case (Byte)'4':
                case (Byte)'5':
                case (Byte)'6':
                case (Byte)'7':
                case (Byte)'8':
                case (Byte)'9':
                case (Byte)'E':
                case (Byte)'e':
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Read in a sequence of characters that are all valid in JSON numbers. Does
        /// not do a complete regex check to validate that this is actually a number.
        /// </summary>
        private String ReadJSONNumericChars()
        {
            var strbld = new StringBuilder();
            while (true)
            {
                var ch = reader.Peek();
                if (!IsJSONNumeric(ch))
                {
                    break;
                }
                strbld.Append((Char)reader.Read());
            }
            return strbld.ToString();
        }

        /// <summary>
        /// Read in a JSON number. If the context dictates, Read in enclosing quotes.
        /// </summary>
        private Int64 ReadJSONInteger()
        {
            context.Read();
            if (context.EscapeNumbers())
            {
                ReadJSONSyntaxChar(QUOTE);
            }

            var str = ReadJSONNumericChars();
            if (context.EscapeNumbers())
            {
                ReadJSONSyntaxChar(QUOTE);
            }

            try
            {
                return Int64.Parse(str);
            }
            catch (FormatException fex)
            {
                throw new TProtocolException(TProtocolException.INVALID_DATA,
                                             "Bad data encounted in numeric data", fex);
            }
        }

        /// <summary>
        /// Read in a JSON double value. Throw if the value is not wrapped in quotes
        /// when expected or if wrapped in quotes when not expected.
        /// </summary>
        private Double ReadJSONDouble()
        {
            context.Read();
            if (reader.Peek() == QUOTE[0])
            {
                var arr = ReadJSONString(true);
                var dub = Double.Parse(utf8Encoding.GetString(arr, 0, arr.Length), CultureInfo.InvariantCulture);

                if (!context.EscapeNumbers() && !Double.IsNaN(dub) && !Double.IsInfinity(dub))
                {
                    // Throw exception -- we should not be in a string in this case
                    throw new TProtocolException(TProtocolException.INVALID_DATA,
                                                 "Numeric data unexpectedly quoted");
                }
                return dub;
            }
            else
            {
                if (context.EscapeNumbers())
                {
                    // This will throw - we should have had a quote if escapeNum == true
                    ReadJSONSyntaxChar(QUOTE);
                }
                try
                {
                    return Double.Parse(ReadJSONNumericChars(), CultureInfo.InvariantCulture);
                }
                catch (FormatException fex)
                {
                    throw new TProtocolException(TProtocolException.INVALID_DATA,
                                                 "Bad data encounted in numeric data", fex);
                }
            }
        }

        /// <summary>
        /// Read in a JSON string containing base-64 encoded data and decode it.
        /// </summary>
        private Byte[] ReadJSONBase64()
        {
            var b = ReadJSONString(false);
            var len = b.Length;
            var off = 0;
            var size = 0;
            // reduce len to ignore fill bytes
            while ((len > 0) && (b[len - 1] == '='))
            {
                --len;
            }
            // read & decode full byte triplets = 4 source bytes
            while (len > 4)
            {
                // Decode 4 bytes at a time
                TBase64Utils.decode(b, off, 4, b, size); // NB: decoded in place
                off += 4;
                len -= 4;
                size += 3;
            }
            // Don't decode if we hit the end or got a single leftover byte (invalid
            // base64 but legal for skip of regular string type)
            if (len > 1)
            {
                // Decode remainder
                TBase64Utils.decode(b, off, len, b, size); // NB: decoded in place
                size += len - 1;
            }
            // Sadly we must copy the byte[] (any way around this?)
            var result = new Byte[size];
            Array.Copy(b, 0, result, 0, size);
            return result;
        }

        private void ReadJSONObjectStart()
        {
            context.Read();
            ReadJSONSyntaxChar(LBRACE);
            PushContext(new JSONPairContext(this));
        }

        private void ReadJSONObjectEnd()
        {
            ReadJSONSyntaxChar(RBRACE);
            PopContext();
        }

        private void ReadJSONArrayStart()
        {
            context.Read();
            ReadJSONSyntaxChar(LBRACKET);
            PushContext(new JSONListContext(this));
        }

        private void ReadJSONArrayEnd()
        {
            ReadJSONSyntaxChar(RBRACKET);
            PopContext();
        }

        public override TMessage ReadMessageBegin()
        {
            var message = new TMessage();
            ReadJSONArrayStart();
            if (ReadJSONInteger() != VERSION)
            {
                throw new TProtocolException(TProtocolException.BAD_VERSION,
                                             "Message contained bad version.");
            }

            var buf = ReadJSONString(false);
            message.Name = utf8Encoding.GetString(buf, 0, buf.Length);
            message.Type = (TMessageType)ReadJSONInteger();
            message.SeqID = (Int32)ReadJSONInteger();
            return message;
        }

        public override void ReadMessageEnd()
        {
            ReadJSONArrayEnd();
        }

        public override TStruct ReadStructBegin()
        {
            ReadJSONObjectStart();
            return new TStruct();
        }

        public override void ReadStructEnd()
        {
            ReadJSONObjectEnd();
        }

        public override TField ReadFieldBegin()
        {
            var field = new TField();
            var ch = reader.Peek();
            if (ch == RBRACE[0])
            {
                field.Type = TType.Stop;
            }
            else
            {
                field.ID = (Int16)ReadJSONInteger();
                ReadJSONObjectStart();
                field.Type = GetTypeIDForTypeName(ReadJSONString(false));
            }
            return field;
        }

        public override void ReadFieldEnd()
        {
            ReadJSONObjectEnd();
        }

        public override TMap ReadMapBegin()
        {
            var map = new TMap();
            ReadJSONArrayStart();
            map.KeyType = GetTypeIDForTypeName(ReadJSONString(false));
            map.ValueType = GetTypeIDForTypeName(ReadJSONString(false));
            map.Count = (Int32)ReadJSONInteger();
            ReadJSONObjectStart();
            return map;
        }

        public override void ReadMapEnd()
        {
            ReadJSONObjectEnd();
            ReadJSONArrayEnd();
        }

        public override TList ReadListBegin()
        {
            var list = new TList();
            ReadJSONArrayStart();
            list.ElementType = GetTypeIDForTypeName(ReadJSONString(false));
            list.Count = (Int32)ReadJSONInteger();
            return list;
        }

        public override void ReadListEnd()
        {
            ReadJSONArrayEnd();
        }

        public override TSet ReadSetBegin()
        {
            var set = new TSet();
            ReadJSONArrayStart();
            set.ElementType = GetTypeIDForTypeName(ReadJSONString(false));
            set.Count = (Int32)ReadJSONInteger();
            return set;
        }

        public override void ReadSetEnd()
        {
            ReadJSONArrayEnd();
        }

        public override Boolean ReadBool()
        {
            return (ReadJSONInteger() == 0 ? false : true);
        }

        public override SByte ReadByte()
        {
            return (SByte)ReadJSONInteger();
        }

        public override Int16 ReadI16()
        {
            return (Int16)ReadJSONInteger();
        }

        public override Int32 ReadI32()
        {
            return (Int32)ReadJSONInteger();
        }

        public override Int64 ReadI64()
        {
            return ReadJSONInteger();
        }

        public override Double ReadDouble()
        {
            return ReadJSONDouble();
        }

        public override String ReadString()
        {
            var buf = ReadJSONString(false);
            return utf8Encoding.GetString(buf, 0, buf.Length);
        }

        public override Byte[] ReadBinary()
        {
            return ReadJSONBase64();
        }

    }
}