using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnwrapAndAwait
{
    class Program
    {
        static async Task<int> Foo()
        {
            int result = await await Task.Factory.StartNew(async ()=>
            {
                await Task.Delay(1000);
                return 42;
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            return result;
        }

        static async Task<int> Bar()
        {
            int result = await Task.Factory.StartNew(async ()=>
            {
                await Task.Delay(1000);
                return 42;
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();
            return result;
        }
        static void Main()
        {
            Console.WriteLine(Foo().Result);
            Console.WriteLine(Bar().Result);
        }
    }
}
