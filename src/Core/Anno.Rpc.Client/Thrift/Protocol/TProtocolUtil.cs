namespace Thrift.Protocol
{
    public static class TProtocolUtil
    {
        public static void Skip(TProtocol prot, TType type)
        {
            prot.IncrementRecursionDepth();
            try
            {
                switch (type)
                {
                    case TType.Bool:
                        prot.ReadBool();
                        break;
                    case TType.Byte:
                        prot.ReadByte();
                        break;
                    case TType.I16:
                        prot.ReadI16();
                        break;
                    case TType.I32:
                        prot.ReadI32();
                        break;
                    case TType.I64:
                        prot.ReadI64();
                        break;
                    case TType.Double:
                        prot.ReadDouble();
                        break;
                    case TType.String:
                        // Don't try to decode the string, just skip it.
                        prot.ReadBinary();
                        break;
                    case TType.Struct:
                        prot.ReadStructBegin();
                        while (true)
                        {
                            var field = prot.ReadFieldBegin();
                            if (field.Type == TType.Stop)
                            {
                                break;
                            }
                            Skip(prot, field.Type);
                            prot.ReadFieldEnd();
                        }
                        prot.ReadStructEnd();
                        break;
                    case TType.Map:
                        var map = prot.ReadMapBegin();
                        for (var i = 0; i < map.Count; i++)
                        {
                            Skip(prot, map.KeyType);
                            Skip(prot, map.ValueType);
                        }
                        prot.ReadMapEnd();
                        break;
                    case TType.Set:
                        var set = prot.ReadSetBegin();
                        for (var i = 0; i < set.Count; i++)
                        {
                            Skip(prot, set.ElementType);
                        }
                        prot.ReadSetEnd();
                        break;
                    case TType.List:
                        var list = prot.ReadListBegin();
                        for (var i = 0; i < list.Count; i++)
                        {
                            Skip(prot, list.ElementType);
                        }
                        prot.ReadListEnd();
                        break;
                    default:
                        throw new TProtocolException(TProtocolException.INVALID_DATA, "Unknown data type " + type.ToString("d"));
                }
            }
            finally
            {
                prot.DecrementRecursionDepth();
            }
        }
    }
}