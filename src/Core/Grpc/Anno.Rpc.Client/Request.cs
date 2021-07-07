using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Grpc.Core;

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
            transport = GrpcFactory.BorrowInstance(id);
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
                BrokerRequest brokerRequest = new BrokerRequest();
                brokerRequest.Input.Add(input);
                output = transport.Client.broker(request:brokerRequest,options:transport.TimeOut.GetCallOptions()).Reply; //如果连接不可用，会报IO异常(重试)
            }
            catch (Exception ex)
            {
                if (ex is RpcException)//连接不可用的时候 直接销毁 从新从连接池拿
                {
                    var sEx = (RpcException)ex;
                    if (sEx.StatusCode == StatusCode.Unavailable || sEx.StatusCode == StatusCode.Aborted)
                    {
                        error++;
                        if (error == 3) //累计3 拿不到有效连接 抛出异常 移除（此值 只是一个参考）
                        {
                            GrpcFactory.RemoveServicePool(id);
                            throw sEx;
                        }

                        GrpcFactory.ReturnInstance(transport, id); //归还有问题链接

                        transport = GrpcFactory.BorrowInstance(id);
                        goto reStart;
                    }
                    else if (sEx.StatusCode == StatusCode.DeadlineExceeded) {
                        GrpcFactory.ReturnInstance(transport, id); //归还有问题链接
                    }
                }
                throw ex;
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
                        GrpcFactory.ReturnInstance(transport,id);
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
