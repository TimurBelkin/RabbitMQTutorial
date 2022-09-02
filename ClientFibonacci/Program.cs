using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClientFibonacci
{
    public class RpcClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;
        private const string HostName = "localhost";
        private const string RpcQueueName = "rpc_queue";

        public RpcClient()
        {
            var factory = new ConnectionFactory() { HostName = HostName};
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            var correlatedId = Guid.NewGuid().ToString();
            props.CorrelationId = correlatedId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlatedId)
                {
                    respQueue.Add(response);
                }
            };

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true
            );
        }

        public string Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: "",
                routingKey: RpcQueueName,
                basicProperties: props,
                body: messageBytes);

            return respQueue.Take();
        }

        public void Close()
        {
            connection.Close();
        }
    }

    public class Rpc
    {
        static void Main(string[] args)
        {
            DoFibanacciRequests();
        }

        private static void DoFibanacciRequests()
        {
            var rpcClient = new RpcClient();
            while (true)
            {
                Console.Write("Input a number: ");
                string number = Console.ReadLine();
                if (!Int32.TryParse(number, out int _))
                {
                    Console.Write(" [x] Failed to parse input number");
                    break;
                }

                Console.WriteLine(" [x] Requesting fib({0})", number);
                var response = rpcClient.Call(number);

                Console.WriteLine(" [.] Got '{0}'", response); 
            }

            rpcClient.Close();
        }
    }
}
