using System;
using System.Net.Sockets;

namespace Thrift.Transport
{
    public class TSocket : TStreamTransport
    {
        private Int32 timeout = 0;

        public TSocket(TcpClient client)
        {
            TcpClient = client;

            if (IsOpen)
            {
                inputStream = client.GetStream();
                outputStream = client.GetStream();
            }
        }

        public TSocket(String host, Int32 port)
            : this(host, port, 0)
        {
        }

        public TSocket(String host, Int32 port, Int32 timeout)
        {
            Host = host;
            Port = port;
            this.timeout = timeout;

            InitSocket();
        }

        private void InitSocket()
        {
            TcpClient = TSocketVersionizer.CreateTcpClient();
            TcpClient.ReceiveTimeout = TcpClient.SendTimeout = timeout;
            TcpClient.Client.NoDelay = true;
        }

        public Int32 Timeout
        {
            set => TcpClient.ReceiveTimeout = TcpClient.SendTimeout = timeout = value;
        }

        public TcpClient TcpClient { get; private set; } = null;

        public String Host { get; private set; } = null;

        public Int32 Port { get; private set; } = 0;

        public override Boolean IsOpen
        {
            get
            {
                try
                {
                    if (TcpClient == null)
                    {
                        return false;
                    }

                    return TcpClient.Connected;
                }
                catch
                {
                    return false;
                }
            }
        }

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

            inputStream = TcpClient.GetStream();
            outputStream = TcpClient.GetStream();
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

        public override void Close()
        {
            base.Close();
            if (TcpClient != null)
            {
                TcpClient.Close();
                TcpClient = null;
            }
        }

        #region ����
        private Boolean _IsDisposed;

        /// <summary>����</summary>
        protected override void Dispose(Boolean disposing)
        {
            if (!_IsDisposed)
            {
                if (disposing)
                {
                    if (TcpClient != null)
                        ((IDisposable)TcpClient).Dispose();
                    base.Dispose(disposing);
                }
            }
            _IsDisposed = true;
        }
        #endregion
    }
}
