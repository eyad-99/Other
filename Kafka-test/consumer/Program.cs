using Confluent.Kafka;
using System;

class Program
{
    static void Main()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "my-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        // 👇 Assign specific partition manually
        var topicPartition = new TopicPartition("test-topic", new Partition(0));

        consumer.Assign(topicPartition);

        Console.WriteLine("Consuming from partition 0...");

        while (true)
        {
            var result = consumer.Consume();

            Console.WriteLine($"Partition: {result.Partition}, Offset: {result.Offset}, Value: {result.Message.Value}");
        }
    }
}