using Confluent.Kafka;
using System;

class Program
{
    static void Main(string[] args)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();

        consumer.Subscribe("test-topic");

        Console.WriteLine("Listening...");

        while (true)
        {
            var result = consumer.Consume();

            try
            {
                // simulate failure condition
                if (result.Message.Value.Contains("fail"))
                    throw new Exception("Processing failed");

                Console.WriteLine($"Processed: {result.Message.Value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                // send to dead letter topic
                producer.Produce(
                    "dead-letter-topic",
                    new Message<Null, string>
                    {
                        Value = result.Message.Value
                    });

                Console.WriteLine("Sent to dead-letter-topic");
            }
        }
    }
}