using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Thrift.Transport
{
    /// <summary>
    /// SSL Server Socket Wrapper Class
    /// </summary>
    public class TTLSServerSocket : TServerTransport
    {
        /// <summary>
        /// Underlying tcp server
        /// </summary>
        private TcpListener server = null;

        /// <summary>
        /// The port where the socket listen
        /// </summary>
        private readonly Int32 port = 0;

        /// <summary>
        /// Timeout for the created server socket
        /// </summary>
        private readonly Int32 clientTimeout;

        /// <summary>
        /// Whether or not to wrap new TSocket connections in buffers
        /// </summary>
        private readonly Boolean useBufferedSockets = false;

        /// <summary>
        /// The servercertificate with the private- and public-key
        /// </summary>
        private readonly X509Certificate serverCertificate;

        /// <summary>
        /// The function to validate the client certificate.
        /// </summary>
        private readonly RemoteCertificateValidationCallback clientCertValidator;

        /// <summary>
        /// The function to determine which certificate to use.
        /// </summary>
        private readonly LocalCertificateSelectionCallback localCertificateSelectionCallback;

        /// <summary>
        /// The SslProtocols value that represents the protocol used for authentication.
        /// </summary>
        private readonly SslProtocols sslProtocols;

        /// <summary>
        /// Initializes a new instance of the <see cref="TTLSServerSocket" /> class.
        /// </summary>
        /// <param name="port">The port where the server runs.</param>
        /// <param name="certificate">The certificate object.</param>
        public TTLSServerSocket(Int32 port, X509Certificate2 certificate)
            : this(port, 0, certificate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TTLSServerSocket" /> class.
        /// </summary>
        /// <param name="port">The port where the server runs.</param>
        /// <param name="clientTimeout">Send/receive timeout.</param>
        /// <param name="certificate">The certificate object.</param>
        public TTLSServerSocket(Int32 port, Int32 clientTimeout, X509Certificate2 certificate)
            : this(port, clientTimeout, false, certificate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TTLSServerSocket" /> class.
        /// </summary>
        /// <param name="port">The port where the server runs.</param>
        /// <param name="clientTimeout">Send/receive timeout.</param>
        /// <param name="useBufferedSockets">If set to <c>true</c> [use buffered sockets].</param>
        /// <param name="certificate">The certificate object.</param>
        /// <param name="clientCertValidator">The certificate validator.</param>
        /// <param name="localCertificateSelectionCallback">The callback to select which certificate to use.</param>
        /// <param name="sslProtocols">The SslProtocols value that represents the protocol used for authentication.</param>
        public TTLSServerSocket(
            Int32 port,
            Int32 clientTimeout,
            Boolean useBufferedSockets,
            X509Certificate2 certificate,
            RemoteCertificateValidationCallback clientCertValidator = null,
            LocalCertificateSelectionCallback localCertificateSelectionCallback = null,
            // TODO: Enable Tls11 and Tls12 (TLS 1.1 and 1.2) by default once we start using .NET 4.5+.
            SslProtocols sslProtocols = SslProtocols.Tls)
        {
            if (!certificate.HasPrivateKey)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown, "Your server-certificate needs to have a private key");
            }

            this.port = port;
            this.clientTimeout = clientTimeout;
            serverCertificate = certificate;
            this.useBufferedSockets = useBufferedSockets;
            this.clientCertValidator = clientCertValidator;
            this.localCertificateSelectionCallback = localCertificateSelectionCallback;
            this.sslProtocols = sslProtocols;
            try
            {
                // Create server socket
                server = TSocketVersionizer.CreateTcpListener(this.port);
                server.Server.NoDelay = true;
            }
            catch (Exception ex)
            {
                server = null;
                throw new TTransportException("Could not create ServerSocket on port " + this.port + ".", ex);
            }
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public override void Listen()
        {
            // Make sure accept is not blocking
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

        /// <summary>
        /// Callback for Accept Implementation
        /// </summary>
        /// <returns>
        /// TTransport-object.
        /// </returns>
        protected override TTransport AcceptImpl()
        {
            if (server == null)
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen, "No underlying server socket.");
            }

            try
            {
                var client = server.AcceptTcpClient();
                client.SendTimeout = client.ReceiveTimeout = clientTimeout;

                //wrap the client in an SSL Socket passing in the SSL cert
                var socket = new TTLSSocket(
                    client,
                    serverCertificate,
                    true,
                    clientCertValidator,
                    localCertificateSelectionCallback,
                    sslProtocols);

                socket.setupTLS();

                if (useBufferedSockets)
                {
                    var trans = new TBufferedTransport(socket);
                    return trans;
                }
                else
                {
                    return socket;
                }

            }
            catch (Exception ex)
            {
                throw new TTransportException(ex.ToString(), ex);
            }
        }

        /// <summary>
        /// Stops the Server
        /// </summary>
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
