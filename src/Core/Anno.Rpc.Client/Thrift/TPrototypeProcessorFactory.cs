using System;
using Thrift.Server;
using Thrift.Transport;

namespace Thrift
{
    public class TPrototypeProcessorFactory<P, H> : TProcessorFactory where P : TProcessor
    {
        readonly Object[] handlerArgs = null;

        public TPrototypeProcessorFactory()
        {
            handlerArgs = new Object[0];
        }

        public TPrototypeProcessorFactory(params Object[] handlerArgs)
        {
            this.handlerArgs = handlerArgs;
        }

        public TProcessor GetProcessor(TTransport trans, TServer server = null)
        {
            var handler = (H)Activator.CreateInstance(typeof(H), handlerArgs);

            var handlerServerRef = handler as TControllingHandler;
            if (handlerServerRef != null)
            {
                handlerServerRef.server = server;
            }
            return Activator.CreateInstance(typeof(P), new Object[] { handler }) as TProcessor;
        }
    }
}