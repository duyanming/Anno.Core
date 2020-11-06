using System;
using System.IO;

namespace Thrift.Transport
{
    public class TFramedTransport : TTransport, IDisposable
    {
        private readonly TTransport transport;
        private readonly MemoryStream writeBuffer = new MemoryStream(1024);
        private readonly MemoryStream readBuffer = new MemoryStream(1024);

        private const Int32 HeaderSize = 4;
        private readonly Byte[] headerBuf = new Byte[HeaderSize];

        public class Factory : TTransportFactory
        {
            public override TTransport GetTransport(TTransport trans)
            {
                return new TFramedTransport(trans);
            }
        }

        public TFramedTransport(TTransport transport)
        {
            if (transport == null)
                throw new ArgumentNullException("transport");
            this.transport = transport;
            InitWriteBuffer();
        }

        public override void Open()
        {
            CheckNotDisposed();
            transport.Open();
        }

        public override Boolean IsOpen =>
                // We can legitimately throw here but be nice a bit.
                // CheckNotDisposed();
                !_IsDisposed && transport.IsOpen;

        public override void Close()
        {
            CheckNotDisposed();
            transport.Close();
        }

        public override Int32 Read(Byte[] buf, Int32 off, Int32 len)
        {
            CheckNotDisposed();
            ValidateBufferArgs(buf, off, len);
            if (!IsOpen)
                throw new TTransportException(TTransportException.ExceptionType.NotOpen);
            var got = readBuffer.Read(buf, off, len);
            if (got > 0)
            {
                return got;
            }

            // Read another frame of data
            ReadFrame();

            return readBuffer.Read(buf, off, len);
        }

        private void ReadFrame()
        {
            transport.ReadAll(headerBuf, 0, HeaderSize);
            var size = DecodeFrameSize(headerBuf);

            readBuffer.SetLength(size);
            readBuffer.Seek(0, SeekOrigin.Begin);
            var buff = readBuffer.GetBuffer();
            transport.ReadAll(buff, 0, size);
        }

        public override void Write(Byte[] buf, Int32 off, Int32 len)
        {
            CheckNotDisposed();
            ValidateBufferArgs(buf, off, len);
            if (!IsOpen)
                throw new TTransportException(TTransportException.ExceptionType.NotOpen);
            if (writeBuffer.Length + len > Int32.MaxValue)
                Flush();
            writeBuffer.Write(buf, off, len);
        }

        private void InternalFlush()
        {
            CheckNotDisposed();
            if (!IsOpen)
                throw new TTransportException(TTransportException.ExceptionType.NotOpen);
            var buf = writeBuffer.GetBuffer();
            var len = (Int32)writeBuffer.Length;
            var data_len = len - HeaderSize;
            if (data_len < 0)
                throw new System.InvalidOperationException(); // logic error actually

            // Inject message header into the reserved buffer space
            EncodeFrameSize(data_len, buf);

            // Send the entire message at once
            transport.Write(buf, 0, len);

            InitWriteBuffer();
        }

        public override void Flush()
        {
            CheckNotDisposed();
            InternalFlush();

            transport.Flush();
        }

        public override IAsyncResult BeginFlush(AsyncCallback callback, Object state)
        {
            CheckNotDisposed();
            InternalFlush();

            return transport.BeginFlush(callback, state);
        }

        public override void EndFlush(IAsyncResult asyncResult)
        {
            transport.EndFlush(asyncResult);
        }

        private void InitWriteBuffer()
        {
            // Reserve space for message header to be put right before sending it out
            writeBuffer.SetLength(HeaderSize);
            writeBuffer.Seek(0, SeekOrigin.End);
        }

        private static void EncodeFrameSize(Int32 frameSize, Byte[] buf)
        {
            buf[0] = (Byte)(0xff & (frameSize >> 24));
            buf[1] = (Byte)(0xff & (frameSize >> 16));
            buf[2] = (Byte)(0xff & (frameSize >> 8));
            buf[3] = (Byte)(0xff & (frameSize));
        }

        private static Int32 DecodeFrameSize(Byte[] buf)
        {
            return
                ((buf[0] & 0xff) << 24) |
                ((buf[1] & 0xff) << 16) |
                ((buf[2] & 0xff) << 8) |
                ((buf[3] & 0xff));
        }


        private void CheckNotDisposed()
        {
            if (_IsDisposed)
                throw new ObjectDisposedException("TFramedTransport");
        }

        #region ����
        private Boolean _IsDisposed;

        /// <summary>����</summary>
        /// <param name="disposing"></param>
        protected override void Dispose(Boolean disposing)
        {
            if (!_IsDisposed)
            {
                if (disposing)
                {
                    if (readBuffer != null)
                        readBuffer.Dispose();
                    if (writeBuffer != null)
                        writeBuffer.Dispose();
                    if (transport != null)
                        transport.Dispose();
                }
            }
            _IsDisposed = true;
        }
        #endregion
    }
}
