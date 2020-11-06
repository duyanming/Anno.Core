using System;

namespace Thrift.Server
{
    /// <summary>
    /// Interface implemented by server users to handle events from the server.
    /// </summary>
    public interface TServerEventHandler
    {
        /// <summary>
        /// Called before the server begins.
        /// </summary>
        void preServe();

        /// <summary>
        /// Called when a new client has connected and is about to being processing.
        /// </summary>
        Object createContext(Protocol.TProtocol input, Protocol.TProtocol output);

        /// <summary>
        /// Called when a client has finished request-handling to delete server context.
        /// </summary>
        void deleteContext(Object serverContext, Protocol.TProtocol input, Protocol.TProtocol output);

        /// <summary>
        /// Called when a client is about to call the processor.
        /// </summary>
        void processContext(Object serverContext, Transport.TTransport transport);
    };
}