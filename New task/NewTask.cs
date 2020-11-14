using RabbitMQ.Client;
using System;
using System.Text;

namespace Newtask
{
    internal class NewTaskMQ
    {
        public static void NewTask(string[] args)
        {
            Console.Write("Message: ");
            var quantidade = int.Parse(Console.ReadLine());

            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "rabbitmq", Password = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue_durable_prefetch",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicQos(0, 1, false);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                var originalMessage = GetMessage(args);
                for (int msgId = 1; msgId <= quantidade; msgId++)
                {
                    var message = originalMessage;
                    message = $"{message}_{msgId}";

                    if (msgId % 2 == 0)
                        message = $"{message}_**";

                    if (msgId % 3 == 0)
                        message = $"{message}_******************";

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                     routingKey: "task_queue_durable_prefetch",
                                     basicProperties: properties,
                                     body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
    }
}
