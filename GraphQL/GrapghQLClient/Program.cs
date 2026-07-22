using GrapghQLDemo.Client;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;
using System.Reactive;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddGrapghQLClient()
          .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5036/graphql/"))
          .ConfigureWebSocketClient(c => c.Uri = new Uri("ws://localhost:5036/graphql/"));

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IGrapghQLClient>();

        var result = await client.GetEmployees.ExecuteAsync();

        foreach (var emp in result.Data.GetEmployees)
        {
            Console.WriteLine($"{emp.Id} - {emp.Name} - {emp.Age}");
        }

        var byIdResult = await client.EmployeeById.ExecuteAsync(1); // pass the ID you want
        var emp1 = byIdResult.Data.EmployeeById;
        Console.WriteLine($"Found: {emp1.Id} - {emp1.Name} - {emp1.Age}");

        //mutation
        var createResult = await client.CreateEmployee.ExecuteAsync("Eyad", 30);
        Console.WriteLine($"Created: {createResult.Data.CreateEmployee.Id} - {createResult.Data.CreateEmployee.Name}");


        //subscription
        // Subscribe to OnEmployeeCreated
        var subscription = client.OnEmployeeCreated.Watch();

        using var disposable = subscription.Subscribe(
    Observer.Create<IOperationResult<IOnEmployeeCreatedResult>>(result =>
    {
        var emp = result.Data.OnEmployeeCreated;
        Console.WriteLine($"New employee: {emp.Id} - {emp.Name} - {emp.Age}");
    })
);
        Console.WriteLine("Listening for new employees... Press Enter to exit.");
        Console.ReadLine();


    }
}
