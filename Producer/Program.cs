using RabbitMQ.Client;
using System;
using System.Text;

namespace Producer
{
    class Program
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "direct_logs", type: ExchangeType.Direct);

            string message = Console.ReadLine();
            while (!string.IsNullOrEmpty(message))
            {
                string[] input = message?.Split(' ');
                var body = Encoding.UTF8.GetBytes(input[0]);
                channel.BasicPublish(exchange: "direct_logs",
                    routingKey: input[1],
                    basicProperties: null,
                    body: body);

                Console.WriteLine(" [x] Sent {0}, routing key: {1}", input[0], input[1]);
                Console.WriteLine(" Enter a word to send and press [enter]. To exit just press [enter]:");
                message = Console.ReadLine();
            }
        }
    }
}
