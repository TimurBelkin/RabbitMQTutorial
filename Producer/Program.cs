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

            channel.ExchangeDeclare(exchange: "log", type: ExchangeType.Fanout);

            string message = Console.ReadLine();
            while (!string.IsNullOrEmpty(message))
            {
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "logs",
                    routingKey: "",
                    basicProperties: null,
                    body: body);

                Console.WriteLine(" [x] Sent {0}", message);
                Console.WriteLine(" Enter a word to send and press [enter]. To exit just press [enter]:");
                message = Console.ReadLine();
            }

            //Console.WriteLine(" Press [enter] to exit.");
            //Console.ReadLine();
        }
    }
}
