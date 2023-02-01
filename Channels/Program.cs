using System.Threading.Channels;

namespace Channels;

class Program
{
    static async Task Main()
    {
        var channel = Channel.CreateBounded<int>(100);

        // Start the consumer
        var consumerTask = Task.Run(async () =>
        {
            while (await channel.Reader.WaitToReadAsync())
            {
                while (channel.Reader.TryRead(out var item))
                {
                    Console.WriteLine("Received item: " + item);
                }
            }
        });

        // Start the producer
        var producerTask = Task.Run(async () =>
        {
            for (int i = 0; i < 1000; i++)
            {
                await channel.Writer.WriteAsync(i);
                Console.WriteLine("Sent item: " + i);
                await Task.Delay(100);
            }
            channel.Writer.Complete();
        });

        // Wait for the producer and consumer to complete
        await Task.WhenAll(producerTask, consumerTask);
    }
}