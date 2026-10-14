using CleanOrders.Application;
using CleanOrders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ConfirmOrderUseCase>();

builder.Services.AddSingleton<IOrderRepository,
    InMemoryOrderRepository>();

var app = builder.Build();

app.MapPost("/api/orders/{id:int}/confirm",
    async (int id, ConfirmOrderUseCase useCase) =>
    {
        try
        {
            var confirmed = await useCase.ExecuteAsync(id);

            return confirmed
                ? Results.Ok(new { Message = "Order confirmed" })
                : Results.NotFound(new { Message = "Order not found" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { ex.Message });
        }
    });

app.Run();