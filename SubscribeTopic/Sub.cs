using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace SubscribeTopic
{
    internal class Sub
    {
        public static void ReceiveLogs(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "rabbitmq", Password = "rabbitmq" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");
            var queueName = channel.QueueDeclare().QueueName;

            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: {0} [binding_key...]",
                                        Environment.GetCommandLineArgs()[0]);
                Console.ReadLine();
                Environment.ExitCode = 1;
                return;
            }

            foreach (var bindingKey in args)
            {
                channel.QueueBind(queue: queueName,
                                  exchange: "topic_logs",
                                  routingKey: bindingKey);
            }

            Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;
                Console.WriteLine(" [x] Received '{0}':'{1}'",
                                  routingKey,
                                  message);
            };
            
            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);

            Console.ReadLine();
        }
    }
}