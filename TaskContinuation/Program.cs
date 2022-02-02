using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TaskContinuation
{
    class Program
    {
        private static readonly ConcurrentBag<int> Results = new ConcurrentBag<int>();

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
            var cancel = new CancellationTokenSource();

            int number = 1;
            while (Results.Count < 10000)
            {
                Task.Factory.StartNew(
                    o =>
                    {
                        var n = (int)o!;
                        return new { Number = n, IsPrime = IsPrime(n) };
                    }, number, cancel.Token).ContinueWith(
                            t => { if (t.Result.IsPrime) Results.Add(t.Result.Number); }, cancel.Token);

                ++number;
            }
            cancel.Cancel();
            foreach (var n in Results.OrderBy(x => x))
            {
                Console.Write("{0},", n);
            }
        }
    }
}
