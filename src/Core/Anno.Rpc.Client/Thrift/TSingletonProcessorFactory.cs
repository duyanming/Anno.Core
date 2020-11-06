using Thrift.Server;
using Thrift.Transport;

namespace Thrift
{
    public class TSingletonProcessorFactory : TProcessorFactory
    {
        private readonly TProcessor processor_;

        public TSingletonProcessorFactory(TProcessor processor) => processor_ = processor;

        public TProcessor GetProcessor(TTransport trans, TServer server = null) => processor_;
    }
}