using RabbitMQ.Client;
using System.Text;
using AuthenticationService.Config;
using Microsoft.Extensions.Options;

namespace AuthenticationService.Services;

public class RabbitMqPublisher
{
    private readonly RabbitMqSettings _rabbitMqSettings;

    public RabbitMqPublisher(IOptions<RabbitMqSettings> rabbitMqSettings)
    {
        _rabbitMqSettings = rabbitMqSettings.Value;
    }

    public void PublishMessage(string message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.Host,
            UserName = _rabbitMqSettings.Username,
            Password = _rabbitMqSettings.Password
        };

        // Create a connection
        using var connection = factory.CreateConnection();
        // Create a channel
        using var channel = connection.CreateModel();

        // Declare a queue (if it doesn't already exist)
        channel.QueueDeclare(
            queue: "messages",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Convert the message to bytes
        var body = Encoding.UTF8.GetBytes(message);

        // Publish the message
        channel.BasicPublish(
            exchange: "",
            routingKey: "messages",
            basicProperties: null,
            body: body);

        Console.WriteLine($"Message Published: {message}");
    }
}
