using Thrift.Server;
using Thrift.Transport;

namespace Thrift
{
    public interface TProcessorFactory
    {
        TProcessor GetProcessor(TTransport trans, TServer server = null);
    }
}