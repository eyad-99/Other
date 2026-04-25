using Confluent.Kafka;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",

            // 👇 IMPORTANT: ACK level
            Acks = Acks.None   
            
    };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        while (true)
        {
            Console.Write("Enter message: ");
            var value = Console.ReadLine();

            try
            {
                var result = await producer.ProduceAsync(
                    new TopicPartition("test-topic", new Partition(0)),
                    new Message<Null, string> { Value = value }
                );

                Console.WriteLine(
                    $"ACK received ✔ | Partition: {result.Partition}, Offset: {result.Offset}"
                );
            }
            catch (ProduceException<Null, string> ex)
            {
                Console.WriteLine($"Failed ❌: {ex.Error.Reason}");
            }
        }
    }
}