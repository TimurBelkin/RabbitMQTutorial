using RabbitMQ.Client;
using System;
using System.Text;

namespace Producer
{
    class Program
    {
        private const string ExchangeName = "topics_logs";
        private const string HostName = "localhost";

        // Format  of inputting is %word% %routing key%
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = HostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Topic);

            string message = Console.ReadLine();
            while (!string.IsNullOrEmpty(message))
            {
                string[] input = message?.Split(' ');
                var body = Encoding.UTF8.GetBytes(input[0]);
                channel.BasicPublish(exchange: ExchangeName,
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
