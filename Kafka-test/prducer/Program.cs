using Confluent.Kafka;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        while (true)
        {
            Console.Write("Enter message: ");
            var value = Console.ReadLine();

            var result = await producer.ProduceAsync(
                new TopicPartition("test-topic", new Partition(0)), // 👈 FORCE partition 0
                new Message<Null, string> { Value = value }
            );

            Console.WriteLine($"Sent to Partition: {result.Partition}, Offset: {result.Offset}");
        }
    }
}