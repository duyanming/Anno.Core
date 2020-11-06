namespace Thrift.Transport
{
    public abstract class TServerTransport
    {
        public abstract void Listen();
        public abstract void Close();
        protected abstract TTransport AcceptImpl();

        public TTransport Accept()
        {
            var transport = AcceptImpl();
            if (transport == null)
            {
                throw new TTransportException("accept() may not return NULL");
            }
            return transport;
        }
    }
}