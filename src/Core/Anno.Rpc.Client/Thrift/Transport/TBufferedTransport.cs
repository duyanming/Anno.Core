using System;
using System.IO;

namespace Thrift.Transport
{
    public class TBufferedTransport : TTransport, IDisposable
    {
        private readonly Int32 bufSize;
        private readonly MemoryStream inputBuffer = new MemoryStream(0);
        private readonly MemoryStream outputBuffer = new MemoryStream(0);
        private readonly TTransport transport;

        public TBufferedTransport(TTransport transport, Int32 bufSize = 1024)
        {
            if (transport == null)
                throw new ArgumentNullException("transport");
            if (bufSize <= 0)
                throw new ArgumentException("bufSize", "Buffer size must be a positive number.");
            this.transport = transport;
            this.bufSize = bufSize;
        }

        public TTransport UnderlyingTransport
        {
            get
            {
                CheckNotDisposed();
                return transport;
            }
        }

        public override Boolean IsOpen =>
                // We can legitimately throw here but be nice a bit.
                // CheckNotDisposed();
                !_IsDisposed && transport.IsOpen;

        public override void Open()
        {
            CheckNotDisposed();
            transport.Open();
        }

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

            if (inputBuffer.Capacity < bufSize)
                inputBuffer.Capacity = bufSize;

            while (true)
            {
                var got = inputBuffer.Read(buf, off, len);
                if (got > 0)
                    return got;

                inputBuffer.Seek(0, SeekOrigin.Begin);
                inputBuffer.SetLength(inputBuffer.Capacity);
                var filled = transport.Read(inputBuffer.GetBuffer(), 0, (Int32)inputBuffer.Length);
                inputBuffer.SetLength(filled);
                if (filled == 0)
                    return 0;
            }
        }

        public override void Write(Byte[] buf, Int32 off, Int32 len)
        {
            CheckNotDisposed();
            ValidateBufferArgs(buf, off, len);
            if (!IsOpen)
                throw new TTransportException(TTransportException.ExceptionType.NotOpen);
            // Relative offset from "off" argument
            var offset = 0;
            if (outputBuffer.Length > 0)
            {
                var capa = (Int32)(outputBuffer.Capacity - outputBuffer.Length);
                var writeSize = capa <= len ? capa : len;
                outputBuffer.Write(buf, off, writeSize);
                offset += writeSize;
                if (writeSize == capa)
                {
                    transport.Write(outputBuffer.GetBuffer(), 0, (Int32)outputBuffer.Length);
                    outputBuffer.SetLength(0);
                }
            }
            while (len - offset >= bufSize)
            {
                transport.Write(buf, off + offset, bufSize);
                offset += bufSize;
            }
            var remain = len - offset;
            if (remain > 0)
            {
                if (outputBuffer.Capacity < bufSize)
                    outputBuffer.Capacity = bufSize;
                outputBuffer.Write(buf, off + offset, remain);
            }
        }

        private void InternalFlush()
        {
            if (!IsOpen)
                throw new TTransportException(TTransportException.ExceptionType.NotOpen);
            if (outputBuffer.Length > 0)
            {
                transport.Write(outputBuffer.GetBuffer(), 0, (Int32)outputBuffer.Length);
                outputBuffer.SetLength(0);
            }
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

        protected void CheckNotDisposed()
        {
            if (_IsDisposed)
                throw new ObjectDisposedException("TBufferedTransport");
        }

        #region ����
        protected Boolean _IsDisposed { get; private set; }

        /// <summary>����</summary>
        protected override void Dispose(Boolean disposing)
        {
            if (!_IsDisposed)
            {
                if (disposing)
                {
                    if (inputBuffer != null)
                        inputBuffer.Dispose();
                    if (outputBuffer != null)
                        outputBuffer.Dispose();
                    if (transport != null)
                        transport.Dispose();
                }
            }
            _IsDisposed = true;
        }
        #endregion
    }
}
