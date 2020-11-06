using System;
using System.Net.Sockets;


namespace Thrift.Transport
{
    public class TServerSocket : TServerTransport
    {
        /// <summary>
        /// Underlying server with socket.
        /// </summary>
        private TcpListener server = null;

        /// <summary>
        /// Port to listen on.
        /// </summary>
        private readonly Int32 port = 0;

        /// <summary>
        /// Timeout for client sockets from accept.
        /// </summary>
        private readonly Int32 clientTimeout = 0;

        /// <summary>
        /// Whether or not to wrap new TSocket connections in buffers.
        /// </summary>
        private readonly Boolean useBufferedSockets = false;

        /// <summary>
        /// Creates a server socket from underlying socket object.
        /// </summary>
        public TServerSocket(TcpListener listener)
            : this(listener, 0)
        {
        }

        /// <summary>
        /// Creates a server socket from underlying socket object.
        /// </summary>
        public TServerSocket(TcpListener listener, Int32 clientTimeout)
        {
            server = listener;
            this.clientTimeout = clientTimeout;
        }

        /// <summary>
        /// Creates just a port listening server socket.
        /// </summary>
        public TServerSocket(Int32 port)
            : this(port, 0)
        {
        }

        /// <summary>
        /// Creates just a port listening server socket.
        /// </summary>
        public TServerSocket(Int32 port, Int32 clientTimeout)
            : this(port, clientTimeout, false)
        {
        }

        public TServerSocket(Int32 port, Int32 clientTimeout, Boolean useBufferedSockets)
        {
            this.port = port;
            this.clientTimeout = clientTimeout;
            this.useBufferedSockets = useBufferedSockets;
            try
            {
                // Make server socket
                server = TSocketVersionizer.CreateTcpListener(this.port);
                server.Server.NoDelay = true;
            }
            catch (Exception ex)
            {
                server = null;
                throw new TTransportException("Could not create ServerSocket on port " + this.port + ".", ex);
            }
        }

        public override void Listen()
        {
            // Make sure not to block on accept
            if (server != null)
            {
                try
                {
                    server.Start();
                }
                catch (SocketException sx)
                {
                    throw new TTransportException("Could not accept on listening socket: " + sx.Message, sx);
                }
            }
        }

        protected override TTransport AcceptImpl()
        {
            if (server == null)
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen, "No underlying server socket.");
            }
            try
            {
                TSocket result2 = null;
                var result = server.AcceptTcpClient();
                try
                {
                    result2 = new TSocket(result)
                    {
                        Timeout = clientTimeout
                    };
                    if (useBufferedSockets)
                    {
                        var result3 = new TBufferedTransport(result2);
                        return result3;
                    }
                    else
                    {
                        return result2;
                    }
                }
                catch (System.Exception)
                {
                    // If a TSocket was successfully created, then let
                    // it do proper cleanup of the TcpClient object.
                    if (result2 != null)
                        result2.Dispose();
                    else //  Otherwise, clean it up ourselves.
                        ((IDisposable)result).Dispose();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new TTransportException(ex.ToString(), ex);
            }
        }

        public override void Close()
        {
            if (server != null)
            {
                try
                {
                    server.Stop();
                }
                catch (Exception ex)
                {
                    throw new TTransportException("WARNING: Could not close server socket: " + ex, ex);
                }
                server = null;
            }
        }
    }
}
