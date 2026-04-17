using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Text;
var factory = new ConnectionFactory { HostName = "localhost" };
using var Connection = factory.CreateConnection();
using var channel = Connection.CreateModel();
channel.QueueDeclare(
    queue: "letterbox",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null
    );
var consumer= new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message= Encoding.UTF8.GetString(body);
    Console.WriteLine("message received"+message);


};
channel.BasicConsume(queue: "letterbox", autoAck: true, consumer: consumer);
Console.ReadKey();