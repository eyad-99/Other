using GrapghQLDemo.Client;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddGrapghQLClient() // matches the "name" in .graphqlrc.json
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5036/graphql/");
                });

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
    }
}
