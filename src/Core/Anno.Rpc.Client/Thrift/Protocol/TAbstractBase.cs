namespace Thrift.Protocol
{
    public interface TAbstractBase
    {
        /// <summary>
        /// Writes the objects out to the protocol.
        /// </summary>
        void Write(TProtocol tProtocol);
    }
}