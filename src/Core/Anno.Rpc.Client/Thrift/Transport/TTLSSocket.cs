using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Thrift.Transport
{
    /// <summary>
    /// SSL Socket Wrapper class
    /// </summary>
    public class TTLSSocket : TStreamTransport
    {
        /// <summary>
        /// The timeout for the connection
        /// </summary>
        private Int32 timeout;

        /// <summary>
        /// Internal SSL Stream for IO
        /// </summary>
        private SslStream secureStream;

        /// <summary>
        /// Defines wheter or not this socket is a server socket<br/>
        /// This is used for the TLS-authentication
        /// </summary>
        private readonly Boolean isServer;

        /// <summary>
        /// The certificate
        /// </summary>
        private readonly X509Certificate certificate;

        /// <summary>
        /// User defined certificate validator.
        /// </summary>
        private readonly RemoteCertificateValidationCallback certValidator;

        /// <summary>
        /// The function to determine which certificate to use.
        /// </summary>
        private readonly LocalCertificateSelectionCallback localCertificateSelectionCallback;

        /// <summary>
        /// The SslProtocols value that represents the protocol used for authentication.SSL protocols to be used.
        /// </summary>
        private readonly SslProtocols sslProtocols;

        /// <summary>
        /// Initializes a new instance of the <see cref="TTLSSocket"/> class.
        /// </summary>
        /// <param name="client">An already created TCP-client</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="isServer">if set to <c>true</c> [is server].</param>
        /// <param name="certValidator">User defined cert validator.</param>
        /// <param name="localCertificateSelectionCallback">The callback to select which certificate to use.</param>
        /// <param name="sslProtocols">The SslProtocols value that represents the protocol used for authentication.</param>
        public TTLSSocket(
            TcpClient client,
            X509Certificate certificate,
            Boolean isServer = false,
            RemoteCertificateValidationCallback certValidator = null,
            LocalCertificateSelectionCallback localCertificateSelectionCallback = null,
            // TODO: Enable Tls11 and Tls12 (TLS 1.1 and 1.2) by default once we start using .NET 4.5+.
            SslProtocols sslProtocols = SslProtocols.Tls)
        {
            TcpClient = client;
            this.certificate = certificate;
            this.certValidator = certValidator;
            this.localCertificateSelectionCallback = localCertificateSelectionCallback;
            this.sslProtocols = sslProtocols;
            this.isServer = isServer;
            if (isServer && certificate == null)
            {
                throw new ArgumentException("TTLSSocket needs certificate to be used for server", "certificate");
            }

            if (IsOpen)
            {
                base.inputStream = client.GetStream();
                base.outputStream = client.GetStream();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TTLSSocket"/> class.
        /// </summary>
        /// <param name="host">The host, where the socket should connect to.</param>
        /// <param name="port">The port.</param>
        /// <param name="certificatePath">The certificate path.</param>
        /// <param name="certValidator">User defined cert validator.</param>
        /// <param name="localCertificateSelectionCallback">The callback to select which certificate to use.</param>
        /// <param name="sslProtocols">The SslProtocols value that represents the protocol used for authentication.</param>
        public TTLSSocket(
            String host,
            Int32 port,
            String certificatePath,
            RemoteCertificateValidationCallback certValidator = null,
            LocalCertificateSelectionCallback localCertificateSelectionCallback = null,
            SslProtocols sslProtocols = SslProtocols.Tls)
            : this(host, port, 0, X509Certificate.CreateFromCertFile(certificatePath), certValidator, localCertificateSelectionCallback, sslProtocols)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TTLSSocket"/> class.
        /// </summary>
        /// <param name="host">The host, where the socket should connect to.</param>
        /// <param name="port">The port.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="certValidator">User defined cert validator.</param>
        /// <param name="localCertificateSelectionCallback">The callback to select which certificate to use.</param>
        /// <param name="sslProtocols">The SslProtocols value that represents the protocol used for authentication.</param>
        public TTLSSocket(
            String host,
            Int32 port,
            X509Certificate certificate = null,
            RemoteCertificateValidationCallback certValidator = null,
            LocalCertificateSelectionCallback localCertificateSelectionCallback = null,
            SslProtocols sslProtocols = SslProtocols.Tls)
            : this(host, port, 0, certificate, certValidator, localCertificateSelectionCallback, sslProtocols)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TTLSSocket"/> class.
        /// </summary>
        /// <param name="host">The host, where the socket should connect to.</param>
        /// <param name="port">The port.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="certValidator">User defined cert validator.</param>
        /// <param name="localCertificateSelectionCallback">The callback to select which certificate to use.</param>
        /// <param name="sslProtocols">The SslProtocols value that represents the protocol used for authentication.</param>
        public TTLSSocket(
            String host,
            Int32 port,
            Int32 timeout,
            X509Certificate certificate,
            RemoteCertificateValidationCallback certValidator = null,
            LocalCertificateSelectionCallback localCertificateSelectionCallback = null,
            SslProtocols sslProtocols = SslProtocols.Tls)
        {
            Host = host;
            Port = port;
            this.timeout = timeout;
            this.certificate = certificate;
            this.certValidator = certValidator;
            this.localCertificateSelectionCallback = localCertificateSelectionCallback;
            this.sslProtocols = sslProtocols;

            InitSocket();
        }

        /// <summary>
        /// Creates the TcpClient and sets the timeouts
        /// </summary>
        private void InitSocket()
        {
            TcpClient = TSocketVersionizer.CreateTcpClient();
            TcpClient.ReceiveTimeout = TcpClient.SendTimeout = timeout;
            TcpClient.Client.NoDelay = true;
        }

        /// <summary>
        /// Sets Send / Recv Timeout for IO
        /// </summary>
        public Int32 Timeout
        {
            set => TcpClient.ReceiveTimeout = TcpClient.SendTimeout = timeout = value;
        }

        /// <summary>
        /// Gets the TCP client.
        /// </summary>
        public TcpClient TcpClient { get; private set; }

        /// <summary>
        /// Gets the host.
        /// </summary>
        public String Host { get; private set; }

        /// <summary>
        /// Gets the port.
        /// </summary>
        public Int32 Port { get; private set; }

        /// <summary>
        /// Gets a value indicating whether TCP Client is Cpen
        /// </summary>
        public override Boolean IsOpen
        {
            get
            {
                if (TcpClient == null)
                {
                    return false;
                }

                return TcpClient.Connected;
            }
        }

        /// <summary>
        /// Validates the certificates!<br/>
        /// </summary>
        /// <param name="sender">The sender-object.</param>
        /// <param name="certificate">The used certificate.</param>
        /// <param name="chain">The certificate chain.</param>
        /// <param name="sslValidationErrors">An enum, which lists all the errors from the .NET certificate check.</param>
        /// <returns></returns>
        private Boolean DefaultCertificateValidator(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslValidationErrors)
        {
            return (sslValidationErrors == SslPolicyErrors.None);
        }

        /// <summary>
        /// Connects to the host and starts the routine, which sets up the TLS
        /// </summary>
        public override void Open()
        {
            if (IsOpen)
            {
                throw new TTransportException(TTransportException.ExceptionType.AlreadyOpen, "Socket already connected");
            }

            if (String.IsNullOrEmpty(Host))
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen, "Cannot open null host");
            }

            if (Port <= 0)
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen, "Cannot open without port");
            }

            if (TcpClient == null)
            {
                InitSocket();
            }

            if (timeout == 0)            // no timeout -> infinite
            {
                TcpClient.Connect(Host, Port);
            }
            else                        // we have a timeout -> use it
            {
                var hlp = new ConnectHelper(TcpClient);
                var asyncres = TcpClient.BeginConnect(Host, Port, new AsyncCallback(ConnectCallback), hlp);
                var bConnected = asyncres.AsyncWaitHandle.WaitOne(timeout) && TcpClient.Connected;
                if (!bConnected)
                {
                    lock (hlp.Mutex)
                    {
                        if (hlp.CallbackDone)
                        {
                            asyncres.AsyncWaitHandle.Close();
                            TcpClient.Close();
                        }
                        else
                        {
                            hlp.DoCleanup = true;
                            TcpClient = null;
                        }
                    }
                    throw new TTransportException(TTransportException.ExceptionType.TimedOut, "Connect timed out");
                }
            }

            setupTLS();
        }

        /// <summary>
        /// Creates a TLS-stream and lays it over the existing socket
        /// </summary>
        public void setupTLS()
        {
            var validator = certValidator ?? DefaultCertificateValidator;

            if (localCertificateSelectionCallback != null)
            {
                secureStream = new SslStream(
                    TcpClient.GetStream(),
                    false,
                    validator,
                    localCertificateSelectionCallback
                );
            }
            else
            {
                secureStream = new SslStream(
                    TcpClient.GetStream(),
                    false,
                    validator
                );
            }

            try
            {
                if (isServer)
                {
                    // Server authentication
                    secureStream.AuthenticateAsServer(certificate, certValidator != null, sslProtocols, true);
                }
                else
                {
                    // Client authentication
                    var certs = certificate != null ? new X509CertificateCollection { certificate } : new X509CertificateCollection();
                    secureStream.AuthenticateAsClient(Host, certs, sslProtocols, true);
                }
            }
            catch (Exception)
            {
                Close();
                throw;
            }

            inputStream = secureStream;
            outputStream = secureStream;
        }

        static void ConnectCallback(IAsyncResult asyncres)
        {
            var hlp = asyncres.AsyncState as ConnectHelper;
            lock (hlp.Mutex)
            {
                hlp.CallbackDone = true;

                try
                {
                    if (hlp.Client.Client != null)
                        hlp.Client.EndConnect(asyncres);
                }
                catch (Exception)
                {
                    // catch that away
                }

                if (hlp.DoCleanup)
                {
                    try
                    {
                        asyncres.AsyncWaitHandle.Close();
                    }
                    catch (Exception) { }

                    try
                    {
                        if (hlp.Client is IDisposable)
                            ((IDisposable)hlp.Client).Dispose();
                    }
                    catch (Exception) { }
                    hlp.Client = null;
                }
            }
        }

        private class ConnectHelper
        {
            public Object Mutex = new Object();
            public Boolean DoCleanup = false;
            public Boolean CallbackDone = false;
            public TcpClient Client;
            public ConnectHelper(TcpClient client)
            {
                Client = client;
            }
        }

        /// <summary>
        /// Closes the SSL Socket
        /// </summary>
        public override void Close()
        {
            base.Close();
            if (TcpClient != null)
            {
                TcpClient.Close();
                TcpClient = null;
            }

            if (secureStream != null)
            {
                secureStream.Close();
                secureStream = null;
            }
        }
    }
}
