using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace UserManagementService.Services
{
    public class RabbitMqConsumer
    {
        private readonly RabbitMqSettings _rabbitMqSettings;

        public RabbitMqConsumer(IOptions<RabbitMqSettings> rabbitMqSettings)
        {
            _rabbitMqSettings = rabbitMqSettings.Value;
        }

        public void StartListening()
        {
            var factory = new ConnectionFactory() { HostName = _rabbitMqSettings.Host };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _rabbitMqSettings.QueueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received: {0}", message);

                    // Processar a mensagem (exemplo: atualizar a lista de usuários)
                };

                channel.BasicConsume(queue: _rabbitMqSettings.QueueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" [*] Waiting for messages...");
                Console.ReadLine();  // Keep the listener running
            }
        }
    }
}
