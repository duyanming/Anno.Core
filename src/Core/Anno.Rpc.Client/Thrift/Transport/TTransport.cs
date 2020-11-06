using System;
using System.IO;

namespace Thrift.Transport
{
    public abstract class TTransport : IDisposable
    {
        public abstract Boolean IsOpen { get; }

        private readonly Byte[] _peekBuffer = new Byte[1];
        private Boolean _hasPeekByte;

        public Boolean Peek()
        {
            //If we already have a byte read but not consumed, do nothing.
            if (_hasPeekByte)
                return true;

            //If transport closed we can't peek.
            if (!IsOpen)
                return false;

            //Try to read one byte. If succeeds we will need to store it for the next read.
            try
            {
                var bytes = Read(_peekBuffer, 0, 1);
                if (bytes == 0)
                    return false;
            }
            catch (IOException)
            {
                return false;
            }

            _hasPeekByte = true;
            return true;
        }

        public abstract void Open();

        public abstract void Close();

        protected static void ValidateBufferArgs(Byte[] buf, Int32 off, Int32 len)
        {
            if (buf == null)
                throw new ArgumentNullException("buf");
            if (off < 0)
                throw new ArgumentOutOfRangeException("Buffer offset is smaller than zero.");
            if (len < 0)
                throw new ArgumentOutOfRangeException("Buffer length is smaller than zero.");
            if (off + len > buf.Length)
                throw new ArgumentOutOfRangeException("Not enough data.");
        }

        public abstract Int32 Read(Byte[] buf, Int32 off, Int32 len);

        public Int32 ReadAll(Byte[] buf, Int32 off, Int32 len)
        {
            ValidateBufferArgs(buf, off, len);
            var got = 0;

            //If we previously peeked a byte, we need to use that first.
            if (_hasPeekByte)
            {
                buf[off + got++] = _peekBuffer[0];
                _hasPeekByte = false;
            }

            while (got < len)
            {
                var ret = Read(buf, off + got, len - got);
                if (ret <= 0)
                {
                    throw new TTransportException(
                        TTransportException.ExceptionType.EndOfFile,
                        "Cannot read, Remote side has closed");
                }
                got += ret;
            }
            return got;
        }

        public virtual void Write(Byte[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public abstract void Write(Byte[] buf, Int32 off, Int32 len);

        public virtual void Flush()
        {
        }

        public virtual IAsyncResult BeginFlush(AsyncCallback callback, Object state)
        {
            throw new TTransportException(
                TTransportException.ExceptionType.Unknown,
                "Asynchronous operations are not supported by this transport.");
        }

        public virtual void EndFlush(IAsyncResult asyncResult)
        {
            throw new TTransportException(
                TTransportException.ExceptionType.Unknown,
                "Asynchronous operations are not supported by this transport.");
        }

        #region ����
        /// <summary>����</summary>
        protected abstract void Dispose(Boolean disposing);

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
