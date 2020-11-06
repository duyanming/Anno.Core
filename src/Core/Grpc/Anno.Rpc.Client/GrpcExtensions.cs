using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;

namespace Anno.Rpc.Client
{
   public static class GrpcExtensions
    {
        public static CallOptions GetCallOptions(this double timeOut) {
            var callOption = new CallOptions();
            callOption.WithDeadline(DateTime.Now.AddMilliseconds(timeOut));
            return callOption;
        }
        public static CallOptions GetCallOptions(this int timeOut)
        {
            var callOption = new CallOptions();
            callOption.WithDeadline(DateTime.Now.AddMilliseconds(timeOut));
            return callOption;
        }
    }
}
