using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;

namespace ConsoleTest
{
    public class CircuitBreakerDemo
    {
      private static CircuitBreakerPolicy policy = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreaker(2, TimeSpan.FromSeconds(5));

        private static CircuitBreakerPolicy policyAdvance = Policy
            .Handle<Exception>()
            .AdvancedCircuitBreaker(0.5
                , TimeSpan.FromSeconds(10)
                , 20
                , TimeSpan.FromSeconds(20));
        public void Handle() {
            for (int i = 0; i < 1000000; i++)
            {
                Task.Run(()=> {
                    AdvancedExecute(i);
                });
                Task.Delay(500).Wait();
            }
        }
        void AdvancedExecute(int i) {
            try
            {
                //policy.Isolate();
                //policy.Reset();
                Console.WriteLine(policy.CircuitState);
                policyAdvance.Execute(() =>
                {
                    DivideByZeroException(i);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ":" + e.GetType().Name);
            }
        }

        void Execute(int i)
        {
            try
            {
                //policy.Isolate();
                //policy.Reset();
                Console.WriteLine(policy.CircuitState);
                policy.Execute(() =>
                {
                    DivideByZeroException(i);

                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ":" + e.GetType().Name);
            }
        }

        void DivideByZeroException(int i)
        {
            //Task.Delay(1000).Wait();
            Console.WriteLine($"{i}:哈哈DivideByZeroException");
            throw new DivideByZeroException();
        }

   //     Policy
   //.Handle<TException>(...)
   //.AdvancedCircuitBreaker(
   //     failureThreshold: 0.5,
   //     samplingDuration: TimeSpan.FromSeconds(5),
   //     minimumThroughput: 20, 
   //     durationOfBreak: TimeSpan.FromSeconds(30))
    }
}
