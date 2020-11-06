using System;
using System.Threading.Tasks;
using Thrift.Protocol;

namespace Thrift
{
    /// <summary>
    /// Processes a message asynchronously.
    /// </summary>
    public interface TAsyncProcessor
    {
        /// <summary>
        /// Processes the next part of the message.
        /// </summary>
        /// <param name="iprot">The input protocol.</param>
        /// <param name="oprot">The output protocol.</param>
        /// <returns>true if there's more to process, false otherwise.</returns>
        Task<Boolean> ProcessAsync(TProtocol iprot, TProtocol oprot);
    }
}