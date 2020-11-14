using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Subscribe
{
    internal class Sub
    {
        public static void ReceiveLogs(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "rabbitmq", Password = "rabbitmq" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: "direct_logs",
                                    type: "direct");
            var queueName = channel.QueueDeclare().QueueName;

            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: {0} [info] [warning] [error]",
                                        Environment.GetCommandLineArgs()[0]);
                Console.ReadLine();
                Environment.ExitCode = 1;
                return;
            }

            foreach (var severity in args)
            {
                channel.QueueBind(queue: queueName,
                                  exchange: "direct_logs",
                                  routingKey: severity);
            }

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;
                Console.WriteLine(" [x] Received '{0}':'{1}'",
                                  routingKey, message);
            };
            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);

            Console.ReadLine();
        }
    }
}