using System;

namespace Thrift.Protocol
{

    /// <summary>
    /// TMultiplexedProtocol is a protocol-independent concrete decorator that allows a Thrift
    /// client to communicate with a multiplexing Thrift server, by prepending the service name
    /// to the function name during function calls.
    /// <para/>
    /// NOTE: THIS IS NOT TO BE USED BY SERVERS.
    /// On the server, use TMultiplexedProcessor to handle requests from a multiplexing client.
    /// <para/>
    /// This example uses a single socket transport to invoke two services:
    /// <code>
    ///     TSocket transport = new TSocket("localhost", 9090);
    ///     transport.open();
    ///
    ///     TBinaryProtocol protocol = new TBinaryProtocol(transport);
    ///
    ///     TMultiplexedProtocol mp = new TMultiplexedProtocol(protocol, "Calculator");
    ///     Calculator.Client service = new Calculator.Client(mp);
    ///
    ///     TMultiplexedProtocol mp2 = new TMultiplexedProtocol(protocol, "WeatherReport");
    ///     WeatherReport.Client service2 = new WeatherReport.Client(mp2);
    ///
    ///     System.out.println(service.add(2,2));
    ///     System.out.println(service2.getTemperature());
    /// </code>
    /// </summary>
    public class TMultiplexedProtocol : TProtocolDecorator
    {

        /// <summary>
        /// Used to delimit the service name from the function name.
        /// </summary>
        public static String SEPARATOR = ":";

        private readonly String ServiceName;

        /// <summary>
        /// Wrap the specified protocol, allowing it to be used to communicate with a
        /// multiplexing server.  The <paramref name="serviceName"/> is required as it is
        /// prepended to the message header so that the multiplexing server can broker
        /// the function call to the proper service.
        /// </summary>
        /// <param name="protocol">Your communication protocol of choice, e.g. <see cref="TBinaryProtocol"/>.</param>
        /// <param name="serviceName">The service name of the service communicating via this protocol.</param>
        public TMultiplexedProtocol(TProtocol protocol, String serviceName)
            : base(protocol)
        {
            ServiceName = serviceName;
        }

        /// <summary>
        /// Prepends the service name to the function name, separated by TMultiplexedProtocol.SEPARATOR.
        /// </summary>
        /// <param name="tMessage">The original message.</param>
        public override void WriteMessageBegin(TMessage tMessage)
        {
            switch (tMessage.Type)
            {
                case TMessageType.Call:
                case TMessageType.Oneway:
                    base.WriteMessageBegin(new TMessage(
                        ServiceName + SEPARATOR + tMessage.Name,
                        tMessage.Type,
                        tMessage.SeqID));
                    break;

                default:
                    base.WriteMessageBegin(tMessage);
                    break;
            }
        }
    }
}