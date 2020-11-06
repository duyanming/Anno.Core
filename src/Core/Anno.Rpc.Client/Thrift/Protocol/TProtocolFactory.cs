using Thrift.Transport;

namespace Thrift.Protocol
{
    public interface TProtocolFactory
    {
        TProtocol GetProtocol(TTransport trans);
    }
}