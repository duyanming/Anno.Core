namespace Thrift.Transport
{
    /// <summary>
    /// From Mark Slee &amp; Aditya Agarwal of Facebook:
    /// Factory class used to create wrapped instance of Transports.
    /// This is used primarily in servers, which get Transports from
    /// a ServerTransport and then may want to mutate them (i.e. create
    /// a BufferedTransport from the underlying base transport)
    /// </summary>
    public class TTransportFactory
    {
        public virtual TTransport GetTransport(TTransport trans) => trans;
    }
}