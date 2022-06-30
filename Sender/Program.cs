using System;
using System.Text;
using RabbitMQ.Client;

namespace Sender
{
    class Program
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using(var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                string message = message = Console.ReadLine();
                while (!string.Equals(message, ""))
                {
                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "",
                        routingKey: "task_queue",
                        basicProperties: properties,
                        body: body);

                    Console.WriteLine(" [x] Sent {0}", message);
                    Console.WriteLine(" Enter a word to send and press [enter]. To exit just press [enter]:");
                    message = Console.ReadLine();
                }
            }

            //Console.WriteLine(" Press [enter] to exit.");
            //Console.ReadLine();
        }
    }
}
