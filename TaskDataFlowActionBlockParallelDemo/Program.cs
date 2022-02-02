using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TaskDataFlowActionBlockParallelDemo
{
    class Program
    {
        static void Main()
        {
            var ab = new ActionBlock<int>(i =>
            {
                Console.Write($"{i}) Task Id:{Task.CurrentId}\t");
                Thread.Sleep(10);
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

            for (int i = 1; i <= 10; i++)
            {
                ab.Post(i);
            }
            ab.Complete();
            ab.Completion.Wait();
        }
    }
}
