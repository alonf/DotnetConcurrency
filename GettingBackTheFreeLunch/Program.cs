using System;
using System.Threading;
using System.Threading.Tasks;

namespace GettingBackTheFreeLunch
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine($"Number of logical processor: {Environment.ProcessorCount}");

            for (int concurrencyLevel = 1; concurrencyLevel <= Environment.ProcessorCount; ++concurrencyLevel)
            {
                const int timeInterval = 1000 * 5;
                long largetPrimeNumber = FindLargestPrimeNumberInTime(concurrencyLevel, timeInterval);
                Console.WriteLine("Result: {0} in {1} seconds using {2} logical processors",
                                  largetPrimeNumber, timeInterval / 1000, concurrencyLevel);
            }
        }

        static bool IsPrime(long number, long tickCountLimit)
        {
            if ((number & 1) == 0)
                return false;

            var limit = Math.Sqrt(number);

            for (long n = 3; n <= limit; n += 2)
            {
                if ((number % n == 0) || (Environment.TickCount > tickCountLimit))
                    return false;
            }
            return true;
        }


        private static long FindLargestPrimeNumberInTime(int concurrencyLevel, long timeInterval)
        {
            var parallelOption = new ParallelOptions
            {
                MaxDegreeOfParallelism = concurrencyLevel
            };

            long dueTime = Environment.TickCount + timeInterval;

            var maxPrime = new ReductionVariable<long>(() => 3);

            const long slice = 16384;

            for (long range = 3; range < long.MaxValue; range += slice)
            {
                Parallel.For(range, range + slice,
                             parallelOption, (n, loopState) =>
                             {
                                 if (loopState.IsStopped)
                                     return;

                                 if (IsPrime(n, dueTime))
                                     maxPrime.Value =
                                         Math.Max(maxPrime.Value, n);

                                 if (Environment.TickCount > dueTime)
                                     loopState.Stop();
                             });


                if (Environment.TickCount > dueTime)
                    break;
            }
            long result = maxPrime.Reduce(Math.Max);
            return result;
        }
    }
}