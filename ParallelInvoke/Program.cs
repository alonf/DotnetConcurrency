using System;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelInvoke
{
    class Program
    {
        static void Main()
        {
            Parallel.Invoke(
                new ParallelOptions { MaxDegreeOfParallelism = 2 },
                () => { Thread.Sleep(1000); Console.WriteLine(1); },
                () => { Thread.Sleep(2000); Console.WriteLine(2); }
                );
            Console.WriteLine("3");
        }
    }
}
