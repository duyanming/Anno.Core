using System;
using System.IO;
using System.Reflection;
using Thrift.Protocol;

namespace Thrift.Transport
{
    public class TMemoryBuffer : TTransport
    {

        private readonly MemoryStream byteStream;

        public TMemoryBuffer()
        {
            byteStream = new MemoryStream();
        }

        public TMemoryBuffer(Byte[] buf)
        {
            byteStream = new MemoryStream(buf);
        }

        public override void Open()
        {
            /** do nothing **/
        }

        public override void Close()
        {
            /** do nothing **/
        }

        public override Int32 Read(Byte[] buf, Int32 off, Int32 len)
        {
            return byteStream.Read(buf, off, len);
        }

        public override void Write(Byte[] buf, Int32 off, Int32 len)
        {
            byteStream.Write(buf, off, len);
        }

        public Byte[] GetBuffer()
        {
            return byteStream.ToArray();
        }


        public override Boolean IsOpen => true;

        public static Byte[] Serialize(TAbstractBase s)
        {
            var t = new TMemoryBuffer();
            var p = new TBinaryProtocol(t);

            s.Write(p);

            return t.GetBuffer();
        }

        public static T DeSerialize<T>(Byte[] buf) where T : TAbstractBase
        {
            var trans = new TMemoryBuffer(buf);
            var p = new TBinaryProtocol(trans);
            if (typeof(TBase).IsAssignableFrom(typeof(T)))
            {
                var method = typeof(T).GetMethod("Read", BindingFlags.Instance | BindingFlags.Public);
                var t = Activator.CreateInstance<T>();
                method.Invoke(t, new Object[] { p });
                return t;
            }
            else
            {
                var method = typeof(T).GetMethod("Read", BindingFlags.Static | BindingFlags.Public);
                return (T)method.Invoke(null, new Object[] { p });
            }
        }

        private Boolean _IsDisposed;

        /// <summary>销毁</summary>
        protected override void Dispose(Boolean disposing)
        {
            if (!_IsDisposed)
            {
                if (disposing)
                {
                    if (byteStream != null)
                        byteStream.Dispose();
                }
            }
            _IsDisposed = true;
        }
    }
}
