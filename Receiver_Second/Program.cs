using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Receiver
{
    class Program
    {
        private const string ExchangeName = "topics_logs";
        private const string RoutingKeyFirst = "*.*.rabbit";
        private const string RoutingKeySecond = "lazy.#";
        private const string HostName = "localhost";

        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = HostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Topic);
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                    exchange: ExchangeName,
                    routingKey: RoutingKeyFirst);
                channel.QueueBind(queue: queueName,
                    exchange: ExchangeName,
                    routingKey: RoutingKeySecond);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Received:  {0}, routing key: {1}", message, routingKey);
                };

                channel.BasicConsume(queue: queueName,
                    autoAck: true,
                    consumer: consumer);
                                
                Console.WriteLine(" Press [enter] to exit");
                Console.WriteLine(" Routing key: {0}", RoutingKeyFirst);
                Console.WriteLine(" Routing key: {0}", RoutingKeySecond);
                Console.ReadLine();
            }
        }
    }
}