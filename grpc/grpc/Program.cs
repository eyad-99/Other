using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo;

class Program
{
    static async Task Main(string[] args)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5228");
        var client = new Greeter.GreeterClient(channel);

        // Unary call (already working)
        var reply = await client.SayHelloAsync(new HelloRequest { Name = "World" });
        Console.WriteLine("Unary response: " + reply.Message);

        // Bidirectional streaming call
        using var call = client.Chat();

        // Task to read responses from server
        var responseReader = Task.Run(async () =>
        {
            await foreach (var serverReply in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine("Streaming response: " + serverReply.Message);
            }
        });

        // Send multiple requests
        foreach (var name in new[] { "Alice", "Bob", "Charlie" })
        {
            await call.RequestStream.WriteAsync(new HelloRequest { Name = name });
            await Task.Delay(500); // simulate delay between messages
        }

        // Complete the request stream
        await call.RequestStream.CompleteAsync();

        // Wait for the reader to finish
        await responseReader;
    }
}

