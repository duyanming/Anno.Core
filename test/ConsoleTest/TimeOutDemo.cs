using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.Timeout;

namespace ConsoleTest
{
    public class TimeOutDemo
    {
        public void Handle() {
            var _timeOut = Policy.Timeout(TimeSpan.FromMilliseconds(1000*1),TimeoutStrategy.Pessimistic,onTimeout:(context, timespan, task) => {
                Console.WriteLine(($"{context.PolicyKey} at {context.PolicyWrapKey}: execution timed out after {timespan.TotalSeconds} seconds."));
            });
            try
            {
                _timeOut.Execute(TimeOut);
            }
            catch (Exception ex) {
                Console.WriteLine($"Ex:{ex.Message}");
            }
        }

        void TimeOut() {
            Console.WriteLine("timeout");
            Task.Delay(1000*2).Wait();
            //throw new Exception();
        }
    }
}
