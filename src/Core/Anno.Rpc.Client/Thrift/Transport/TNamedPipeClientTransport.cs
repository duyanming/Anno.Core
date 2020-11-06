using System;
using System.IO.Pipes;
using System.Threading;

namespace Thrift.Transport
{
    public class TNamedPipeClientTransport : TTransport
    {
        private NamedPipeClientStream client;
        private readonly String ServerName;
        private readonly String PipeName;
        private readonly Int32 ConnectTimeout;

        public TNamedPipeClientTransport(String pipe, Int32 timeout = Timeout.Infinite)
        {
            ServerName = ".";
            PipeName = pipe;
            ConnectTimeout = timeout;
        }

        public TNamedPipeClientTransport(String server, String pipe, Int32 timeout = Timeout.Infinite)
        {
            ServerName = (server != "") ? server : ".";
            PipeName = pipe;
            ConnectTimeout = timeout;
        }

        public override Boolean IsOpen
        {
            get { return client != null && client.IsConnected; }
        }

        public override void Open()
        {
            if (IsOpen)
            {
                throw new TTransportException(TTransportException.ExceptionType.AlreadyOpen);
            }
            client = new NamedPipeClientStream(ServerName, PipeName, PipeDirection.InOut, PipeOptions.None);
            client.Connect(ConnectTimeout);
        }

        public override void Close()
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }

        public override Int32 Read(Byte[] buf, Int32 off, Int32 len)
        {
            if (client == null)
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen);
            }

            return client.Read(buf, off, len);
        }

        public override void Write(Byte[] buf, Int32 off, Int32 len)
        {
            if (client == null)
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen);
            }

            // if necessary, send the data in chunks
            // there's a system limit around 0x10000 bytes that we hit otherwise
            // MSDN: "Pipe write operations across a network are limited to 65,535 bytes per write. For more information regarding pipes, see the Remarks section."
            var nBytes = Math.Min(len, 15 * 4096);  // 16 would exceed the limit
            while (nBytes > 0)
            {
                client.Write(buf, off, nBytes);

                off += nBytes;
                len -= nBytes;
                nBytes = Math.Min(len, nBytes);
            }
        }

        protected override void Dispose(Boolean disposing)
        {
            client.Dispose();
        }
    }
}
