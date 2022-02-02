using System;
using System.Linq;
using System.Threading.Tasks;

namespace PLINQ
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
            var result = (from number in Enumerable.Range(1, 1000000).AsParallel()
                          where IsPrime(number)
                          select number).Average();

            Console.WriteLine(result);
        }
    }
}
