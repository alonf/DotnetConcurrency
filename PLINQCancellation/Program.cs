using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PLINQCancellation
{

    class Program
    {
        static bool IsPrime(int number)
        {
            var result = Parallel.For(2, (int)Math.Sqrt(number) + 1,
                                      (i, s) =>
                                      {
                                          if (number % i == 0)
                                              s.Stop();
                                      });
            return result.IsCompleted;
        }

        static void Main()
        {
            var timeoutCancellationTokenSource = new CancellationTokenSource();

            //Set timeout
            Task.Run(
                () =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    timeoutCancellationTokenSource.Cancel();
                }, timeoutCancellationTokenSource.Token);

            try
            {
                var result = (from number in Enumerable.Range(1, 1000000).AsParallel().WithCancellation(timeoutCancellationTokenSource.Token)
                              where IsPrime(number)
                              select number).Average();
                Console.WriteLine(result);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
