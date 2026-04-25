using Confluent.Kafka;
using System;

class Program
{
    static void Main(string[] args)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        consumer.Subscribe("test-topic");

        Console.WriteLine("Listening...");

        while (true)
        {
            try
            {
                var result = consumer.Consume();

                Console.WriteLine(
                    $"Received: {result.Message.Value} | " +
                    $"Partition: {result.Partition} | Offset: {result.Offset}");
            }
            catch (ConsumeException ex)
            {
                Console.WriteLine($"Error: {ex.Error.Reason}");
            }
        }
    }
}