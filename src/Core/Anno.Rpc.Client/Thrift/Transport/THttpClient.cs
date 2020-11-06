using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Thrift.Transport
{
    public class THttpClient : TTransport, IDisposable
    {
        private readonly Uri uri;
        private readonly X509Certificate[] certificates;
        private Stream inputStream;
        private MemoryStream outputStream = new MemoryStream();

        // Timeouts in milliseconds
        private Int32 connectTimeout = 30000;

        private Int32 readTimeout = 30000;

        private IWebProxy proxy = WebRequest.DefaultWebProxy;

        public THttpClient(Uri u)
            : this(u, Enumerable.Empty<X509Certificate>())
        {
        }

        public THttpClient(Uri u, IEnumerable<X509Certificate> certificates)
        {
            uri = u;
            this.certificates = (certificates ?? Enumerable.Empty<X509Certificate>()).ToArray();
        }

        public Int32 ConnectTimeout
        {
            set
            {
                connectTimeout = value;
            }
        }

        public Int32 ReadTimeout
        {
            set
            {
                readTimeout = value;
            }
        }

        public IDictionary<String, String> CustomHeaders { get; } = new Dictionary<String, String>();

#if !SILVERLIGHT
        public IWebProxy Proxy
        {
            set
            {
                proxy = value;
            }
        }
#endif

        public override Boolean IsOpen
        {
            get
            {
                return true;
            }
        }

        public override void Open()
        {
        }

        public override void Close()
        {
            if (inputStream != null)
            {
                inputStream.Close();
                inputStream = null;
            }
            if (outputStream != null)
            {
                outputStream.Close();
                outputStream = null;
            }
        }

        public override Int32 Read(Byte[] buf, Int32 off, Int32 len)
        {
            if (inputStream == null)
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen, "No request has been sent");
            }

            try
            {
                var ret = inputStream.Read(buf, off, len);

                if (ret == -1)
                {
                    throw new TTransportException(TTransportException.ExceptionType.EndOfFile, "No more data available");
                }

                return ret;
            }
            catch (IOException iox)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown, iox.ToString(), iox);
            }
        }

        public override void Write(Byte[] buf, Int32 off, Int32 len)
        {
            outputStream.Write(buf, off, len);
        }

#if !SILVERLIGHT
        public override void Flush()
        {
            try
            {
                SendRequest();
            }
            finally
            {
                outputStream = new MemoryStream();
            }
        }

        private void SendRequest()
        {
            try
            {
                var connection = CreateRequest();
                connection.Headers.Add("Accept-Encoding", "gzip, deflate");

                var data = outputStream.ToArray();
                connection.ContentLength = data.Length;

                using (var requestStream = connection.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);

                    // Resolve HTTP hang that can happens after successive calls by making sure
                    // that we release the response and response stream. To support this, we copy
                    // the response to a memory stream.
                    using (var response = connection.GetResponse())
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            // Copy the response to a memory stream so that we can
                            // cleanly close the response and response stream.
                            inputStream = new MemoryStream();
                            var buffer = new Byte[8192];  // multiple of 4096
                            Int32 bytesRead;
                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                inputStream.Write(buffer, 0, bytesRead);
                            }
                            inputStream.Seek(0, 0);
                        }

                        var encodings = response.Headers.GetValues("Content-Encoding");
                        if (encodings != null)
                        {
                            foreach (var encoding in encodings)
                            {
                                switch (encoding)
                                {
                                    case "gzip":
                                        DecompressGZipped(ref inputStream);
                                        break;
                                    case "deflate":
                                        DecompressDeflated(ref inputStream);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException iox)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown, iox.ToString(), iox);
            }
            catch (WebException wx)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown, "Couldn't connect to server: " + wx, wx);
            }
        }

        private void DecompressDeflated(ref Stream inputStream)
        {
            var tmp = new MemoryStream();
            using (var decomp = new DeflateStream(inputStream, CompressionMode.Decompress))
            {
                decomp.CopyTo(tmp);
            }
            inputStream.Dispose();
            inputStream = tmp;
            inputStream.Seek(0, 0);
        }

        private void DecompressGZipped(ref Stream inputStream)
        {
            var tmp = new MemoryStream();
            using (var decomp = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                decomp.CopyTo(tmp);
            }
            inputStream.Dispose();
            inputStream = tmp;
            inputStream.Seek(0, 0);
        }
#endif
        private HttpWebRequest CreateRequest()
        {
            var connection = (HttpWebRequest)WebRequest.Create(uri);


#if !SILVERLIGHT
            // Adding certificates through code is not supported with WP7 Silverlight
            // see "Windows Phone 7 and Certificates_FINAL_121610.pdf"
            connection.ClientCertificates.AddRange(certificates);

            if (connectTimeout > 0)
            {
                connection.Timeout = connectTimeout;
            }
            if (readTimeout > 0)
            {
                connection.ReadWriteTimeout = readTimeout;
            }
#endif
            // Make the request
            connection.ContentType = "application/x-thrift";
            connection.Accept = "application/x-thrift";
            connection.UserAgent = "C#/THttpClient";
            connection.Method = "POST";
#if !SILVERLIGHT
            connection.ProtocolVersion = HttpVersion.Version10;
#endif

            //add custom headers here
            foreach (var item in CustomHeaders)
            {
#if !SILVERLIGHT
                connection.Headers.Add(item.Key, item.Value);
#else
                connection.Headers[item.Key] = item.Value;
#endif
            }

#if !SILVERLIGHT
            connection.Proxy = proxy;
#endif

            return connection;
        }

        public override IAsyncResult BeginFlush(AsyncCallback callback, Object state)
        {
            // Extract request and reset buffer
            var data = outputStream.ToArray();

            //requestBuffer_ = new MemoryStream();

            try
            {
                // Create connection object
                var flushAsyncResult = new FlushAsyncResult(callback, state)
                {
                    Connection = CreateRequest(),

                    Data = data
                };


                flushAsyncResult.Connection.BeginGetRequestStream(GetRequestStreamCallback, flushAsyncResult);
                return flushAsyncResult;

            }
            catch (IOException iox)
            {
                throw new TTransportException(iox.ToString(), iox);
            }
        }

        public override void EndFlush(IAsyncResult asyncResult)
        {
            try
            {
                var flushAsyncResult = (FlushAsyncResult)asyncResult;

                if (!flushAsyncResult.IsCompleted)
                {
                    var waitHandle = flushAsyncResult.AsyncWaitHandle;
                    waitHandle.WaitOne();  // blocking INFINITEly
                    waitHandle.Close();
                }

                if (flushAsyncResult.AsyncException != null)
                {
                    throw flushAsyncResult.AsyncException;
                }
            }
            finally
            {
                outputStream = new MemoryStream();
            }

        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            var flushAsyncResult = (FlushAsyncResult)asynchronousResult.AsyncState;
            try
            {
                var reqStream = flushAsyncResult.Connection.EndGetRequestStream(asynchronousResult);
                reqStream.Write(flushAsyncResult.Data, 0, flushAsyncResult.Data.Length);
                reqStream.Flush();
                reqStream.Close();

                // Start the asynchronous operation to get the response
                flushAsyncResult.Connection.BeginGetResponse(GetResponseCallback, flushAsyncResult);
            }
            catch (Exception exception)
            {
                flushAsyncResult.AsyncException = new TTransportException(exception.ToString(), exception);
                flushAsyncResult.UpdateStatusToComplete();
                flushAsyncResult.NotifyCallbackWhenAvailable();
            }
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            var flushAsyncResult = (FlushAsyncResult)asynchronousResult.AsyncState;
            try
            {
                inputStream = flushAsyncResult.Connection.EndGetResponse(asynchronousResult).GetResponseStream();
            }
            catch (Exception exception)
            {
                flushAsyncResult.AsyncException = new TTransportException(exception.ToString(), exception);
            }
            flushAsyncResult.UpdateStatusToComplete();
            flushAsyncResult.NotifyCallbackWhenAvailable();
        }

        // Based on http://msmvps.com/blogs/luisabreu/archive/2009/06/15/multithreading-implementing-the-iasyncresult-interface.aspx
        class FlushAsyncResult : IAsyncResult
        {
            private volatile Boolean _isCompleted;
            private ManualResetEvent _evt;
            private readonly AsyncCallback _cbMethod;
            private readonly Object _state;

            public FlushAsyncResult(AsyncCallback cbMethod, Object state)
            {
                _cbMethod = cbMethod;
                _state = state;
            }

            internal Byte[] Data { get; set; }
            internal HttpWebRequest Connection { get; set; }
            internal TTransportException AsyncException { get; set; }

            public Object AsyncState
            {
                get { return _state; }
            }
            public WaitHandle AsyncWaitHandle
            {
                get { return GetEvtHandle(); }
            }
            public Boolean CompletedSynchronously
            {
                get { return false; }
            }
            public Boolean IsCompleted
            {
                get { return _isCompleted; }
            }
            private readonly Object _locker = new Object();
            private ManualResetEvent GetEvtHandle()
            {
                lock (_locker)
                {
                    if (_evt == null)
                    {
                        _evt = new ManualResetEvent(false);
                    }
                    if (_isCompleted)
                    {
                        _evt.Set();
                    }
                }
                return _evt;
            }
            internal void UpdateStatusToComplete()
            {
                _isCompleted = true; //1. set _iscompleted to true
                lock (_locker)
                {
                    if (_evt != null)
                    {
                        _evt.Set(); //2. set the event, when it exists
                    }
                }
            }

            internal void NotifyCallbackWhenAvailable()
            {
                if (_cbMethod != null)
                {
                    _cbMethod(this);
                }
            }
        }

        #region " IDisposable Support "
        private Boolean _IsDisposed;

        // IDisposable
        protected override void Dispose(Boolean disposing)
        {
            if (!_IsDisposed)
            {
                if (disposing)
                {
                    if (inputStream != null)
                        inputStream.Dispose();
                    if (outputStream != null)
                        outputStream.Dispose();
                }
            }
            _IsDisposed = true;
        }
        #endregion
    }
}
