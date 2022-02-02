using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TaskDataFlowActionBlockDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var ab = new ActionBlock<int>(i =>
                Console.WriteLine($"{i}) Task Id:{Task.CurrentId}"));

            for (int i = 1; i < 10; i++)
            {
                ab.Post(i);
            }
            Thread.Sleep(1000);
            ab.Post(10);
            ab.Complete();
            ab.Completion.Wait();
        }
    }
}
