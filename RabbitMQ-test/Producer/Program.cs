using RabbitMQ.Client;
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
var message = "this is my first message";
var encodedmessage  = Encoding.UTF8.GetBytes(message);
channel.BasicPublish("", "letterbox", null, encodedmessage);
Console.WriteLine("published messagge    "+message);