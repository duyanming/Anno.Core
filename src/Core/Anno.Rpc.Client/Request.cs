using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Thrift.Protocol;
using Thrift.Transport;

namespace Anno.Rpc.Client
{
    public class Request : IDisposable
    {
        private bool disposed;
        private TTransportExt transport;
        private string id;
        public Request(string host, int port)
        {
            this.id = $"{host}:{port}";
            disposed = false;
            transport = ThriftFactory.BorrowInstance(id);
        }
        ~Request()
        {
            Dispose(false);
        }
        /// <summary>
        /// 处理器
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Invoke(Dictionary<string, string> input)
        {
            string output;
            //失败计数器 N次 就直接返回异常
            int error = 0;
            reStart:
            try
            {
                if (!transport.Transport.IsOpen)
                {
                    transport.Transport.Open();
                }

                output = transport.Client.broker(input); //如果连接不可用，会报IO异常(重试)
            }
            catch (TTransportException)
            {
                /*
                 * 连接打开出错
                 */
                ThriftFactory.RemoveServicePool(id);
                throw;
            }
            catch (AggregateException)
            {
                ThriftFactory.RemoveServicePool(id);
                throw;
            }
            catch (Exception ex)
            {
                if (ex is IOException)//连接不可用的时候 直接销毁 从新从连接池拿
                {
                    var sEx = (SocketException)ex.InnerException;
                    if (sEx?.SocketErrorCode == SocketError.TimedOut)
                    {
                        if (transport.Transport.IsOpen)
                        {
                            transport.Transport.Close();
                        }
                        return Connector.FailMessage(SocketError.TimedOut.ToString());
                    }
                    if (sEx?.SocketErrorCode == SocketError.ConnectionReset|| sEx?.SocketErrorCode == SocketError.Shutdown)
                    {
                        if (error == 100) //消耗完 线程池里面的 失效连接（此值 只是一个参考）
                        {
                            return Connector.FailMessage(ex.Message);
                        }

                        if (transport.Transport.IsOpen)
                        {
                            transport.Transport.Close();
                        }
                        ThriftFactory.ReturnInstance(transport,id); //归还有问题链接

                        transport = ThriftFactory.BorrowInstance(id);
                        error++;
                        goto reStart;
                    }
                }
                output = Connector.FailMessage(ex.Message);
            }
            return output;
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    try
                    {
                        ThriftFactory.ReturnInstance(transport,id);
                    }
                    catch (Exception)
                    {
                        //var x = ex;
                    }
                }
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
