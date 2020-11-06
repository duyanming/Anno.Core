using System;
using Thrift.Protocol;
using Thrift.Transport;

namespace Thrift.Server
{
    public abstract class TServer
    {
        //Attributes
        protected TProcessorFactory processorFactory;
        protected TServerTransport serverTransport;
        protected TTransportFactory inputTransportFactory;
        protected TTransportFactory outputTransportFactory;
        protected TProtocolFactory inputProtocolFactory;
        protected TProtocolFactory outputProtocolFactory;
        protected TServerEventHandler serverEventHandler = null;

        //Methods
        public void setEventHandler(TServerEventHandler seh) => serverEventHandler = seh;
        public TServerEventHandler getEventHandler() => serverEventHandler;

        //Log delegation
        public delegate void LogDelegate(String str);
        private LogDelegate _logDelegate;
        protected LogDelegate logDelegate
        {
            get { return _logDelegate; }
            set { _logDelegate = value ?? DefaultLogDelegate; }
        }
        protected static void DefaultLogDelegate(String s) => Console.Error.WriteLine(s);

        //Construction
        public TServer(TProcessor processor,
                  TServerTransport serverTransport)
          : this(processor, serverTransport,
             new TTransportFactory(),
             new TTransportFactory(),
             new TBinaryProtocol.Factory(),
             new TBinaryProtocol.Factory(),
             DefaultLogDelegate)
        {
        }

        public TServer(TProcessor processor,
                TServerTransport serverTransport,
                LogDelegate logDelegate)
          : this(processor,
             serverTransport,
             new TTransportFactory(),
             new TTransportFactory(),
             new TBinaryProtocol.Factory(),
             new TBinaryProtocol.Factory(),
             logDelegate)
        {
        }

        public TServer(TProcessor processor,
                  TServerTransport serverTransport,
                  TTransportFactory transportFactory)
          : this(processor,
             serverTransport,
             transportFactory,
             transportFactory,
             new TBinaryProtocol.Factory(),
             new TBinaryProtocol.Factory(),
             DefaultLogDelegate)
        {
        }

        public TServer(TProcessor processor,
                  TServerTransport serverTransport,
                  TTransportFactory transportFactory,
                  TProtocolFactory protocolFactory)
          : this(processor,
             serverTransport,
             transportFactory,
             transportFactory,
             protocolFactory,
             protocolFactory,
               DefaultLogDelegate)
        {
        }

        public TServer(TProcessor processor,
            TServerTransport serverTransport,
            TTransportFactory inputTransportFactory,
            TTransportFactory outputTransportFactory,
            TProtocolFactory inputProtocolFactory,
            TProtocolFactory outputProtocolFactory,
            LogDelegate logDelegate)
        {
            this.processorFactory = new TSingletonProcessorFactory(processor);
            this.serverTransport = serverTransport;
            this.inputTransportFactory = inputTransportFactory;
            this.outputTransportFactory = outputTransportFactory;
            this.inputProtocolFactory = inputProtocolFactory;
            this.outputProtocolFactory = outputProtocolFactory;
            this.logDelegate = (logDelegate != null) ? logDelegate : DefaultLogDelegate;
        }

        public TServer(TProcessorFactory processorFactory,
                  TServerTransport serverTransport,
                  TTransportFactory inputTransportFactory,
                  TTransportFactory outputTransportFactory,
                  TProtocolFactory inputProtocolFactory,
                  TProtocolFactory outputProtocolFactory,
                  LogDelegate logDelegate)
        {
            this.processorFactory = processorFactory;
            this.serverTransport = serverTransport;
            this.inputTransportFactory = inputTransportFactory;
            this.outputTransportFactory = outputTransportFactory;
            this.inputProtocolFactory = inputProtocolFactory;
            this.outputProtocolFactory = outputProtocolFactory;
            this.logDelegate = (logDelegate != null) ? logDelegate : DefaultLogDelegate;
        }

        //Abstract Interface
        public abstract void Serve();
        public abstract void Stop();
    }
}