using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/chat")
    .Build();

connection.On<string, string>("ReceiveMessage", (user, message) =>
{
    Console.WriteLine($"{user}: {message}");
});

await connection.StartAsync();

Console.WriteLine("Client1 connected");

while (true)
{
    var msg = Console.ReadLine();

    await connection.InvokeAsync("SendMessage", "Client1", msg);
}