namespace Thrift.Protocol
{
    public interface TBase : TAbstractBase
    {
        /// <summary>
        /// Reads the TObject from the given input protocol.
        /// </summary>
        void Read(TProtocol tProtocol);
    }
}
