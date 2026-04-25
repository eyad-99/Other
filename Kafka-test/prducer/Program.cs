using Confluent.Kafka;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        Console.WriteLine("Enter message (type 'exit' to quit):");

        while (true)
        {
            var message = Console.ReadLine();

            if (message == "exit")
                break;

            var result = await producer.ProduceAsync(
                "test-topic",
                new Message<Null, string> { Value = message });

            Console.WriteLine($"Sent to Partition: {result.Partition}, Offset: {result.Offset}");
        }
    }
}