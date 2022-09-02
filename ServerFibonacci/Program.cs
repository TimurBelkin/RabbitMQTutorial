using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace ServerFibonacci
{
    class Program
    {
        private const string HostName = "localhost";
        private const string RpcQueueName = "rpc_queue";

        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = HostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: RpcQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: RpcQueueName,
                autoAck: false,
                consumer: consumer);
            Console.WriteLine(" [x] Awaiting RPC requests");

            consumer.Received += (model, ea) =>
            {
                string response = null;
                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    int number = int.Parse(message);
                    Console.WriteLine(" [.] fib({0})", message);
                    response = Fib(number).ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" [.] " + ex.Message);
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish(exchange: "",
                        routingKey: props.ReplyTo,
                        basicProperties: replyProps,
                        body: responseBytes);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag,
                        multiple: false);
                }
            };

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
            //string message = Console.ReadLine();
            //while (!string.IsNullOrEmpty(message))
            //{
            //    string[] input = message?.Split(' ');

            //    Console.WriteLine(" [x] Sent {0}, routing key: {1}", input[0], input[1]);
            //    Console.WriteLine(" Enter a word to send and press [enter]. To exit just press [enter]:");
            //    message = Console.ReadLine();
            //}
        }

        private static int Fib(int number)
        {
            if (number == 0 || number == 1)
            {
                return number;
            }

            return Fib(number - 1) + Fib(number - 2);
        }
    }
}
