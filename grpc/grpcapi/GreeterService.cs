using Grpc.Core;

namespace GrpcDemo.Services;

public class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request,
        ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {request.Name}"
        });
    }

    // Bidirectional streaming example
    public override async Task Chat(
        IAsyncStreamReader<HelloRequest> requestStream,
        IServerStreamWriter<HelloReply> responseStream,
        ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            var reply = new HelloReply
            {
                Message = $"Hello {request.Name}"
            };

            await responseStream.WriteAsync(reply);
        }
    }
}