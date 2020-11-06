using Thrift.Server;

namespace Thrift
{
    public interface TControllingHandler
    {
        TServer server { get; set; }
    }
}