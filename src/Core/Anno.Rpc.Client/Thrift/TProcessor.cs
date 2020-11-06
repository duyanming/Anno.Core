using System;
using Thrift.Protocol;

namespace Thrift
{
    public interface TProcessor
    {
        Boolean Process(TProtocol iprot, TProtocol oprot);
    }
}