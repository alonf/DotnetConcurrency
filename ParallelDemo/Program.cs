using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelDemo
{
    class Program
    {
        private static readonly object Lock = new object();

        static void Measure(Action action, string message)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            action();
            Console.WriteLine($"{message} {stopwatch.ElapsedMilliseconds} milliseconds");
        }

        private static long SequentialTotal(long from, long to)
        {
            long result = 0;
            for (var l = from; l < to; ++l)
            {
                result += l;
            }
            return result;
        }

        private static long ParallelLockTotal(long from, long to)
        {
            long result = 0;
            Parallel.For(from, to, l =>
            {
                lock (Lock)
                {
                    result += l;
                }
            });

            return result;
        }

        private static long ParallelInterlockedTotal(long from, long to)
        {
            long result = 0;
            Parallel.For(from, to, l => Interlocked.Add(ref result, l));
            return result;
        }

        private static long ParallelTLSTotal(long from, long to)
        {
            long result = 0;
            Parallel.For(from, to, () => 0L, (l, loopState, subTotal) => subTotal + l,
                subTotal => Interlocked.Add(ref result, subTotal));

            return result;
        }

        private static long ParallelTLSPartitionerTotal(long from, long to)
        {
            long result = 0;
            var partitioner = Partitioner.Create(from, to, (to - from) / (Environment.ProcessorCount));

            Parallel.ForEach(partitioner, () => 0L, (range, loopState, subTotal) =>
            {
                for (var l = range.Item1; l < range.Item2; ++l)
                    subTotal += l;
                return subTotal;
            }, subTotal => Interlocked.Add(ref result, subTotal));
            return result;
        }

        static void Main()
        {
            const long from = 0;
            const long to = (long)1E8;

            Measure(() => Console.WriteLine("Sum:" + SequentialTotal(from, to)), "Sequential times:");
            Measure(() => Console.WriteLine("Sum:" + ParallelLockTotal(from, to)), "Parallel with lock times:");
            Measure(() => Console.WriteLine("Sum:" + ParallelInterlockedTotal(from, to)), "Parallel with interlocked times:");
            Measure(() => Console.WriteLine("Sum:" + ParallelTLSTotal(from, to)), "Parallel with TLS times:");
            Measure(() => Console.WriteLine("Sum:" + ParallelTLSPartitionerTotal(from, to)), "Parallel with TLS and Partitioner times:");
        }
    }
}
