using GrpcDemo.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Kestrel (HTTP/2 required for gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5228, o =>
    {
        o.Protocols = HttpProtocols.Http2;
    });
});

// Services
builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();

// ✅ gRPC mapping (IMPORTANT)
app.MapGrpcService<GreeterService>();

// REST controllers
app.MapControllers();

// Reflection (ONLY ONCE)
app.MapGrpcReflectionService();

app.Run();