using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RabbitMQPoc.Service
{
    public class RabbitMqService
    {
        private string _hostName = "localhost";
        private string _userName = "guest";
        private string _password = "guest";

        public static string ExchangeName = "Exchange";
        public static string ResponseQueueName = "ResponseQueue";
        public static string RequestQueueName = "RequestQueue";

        public RabbitMqService()
        {
            SetUpEnv();
        }

        private void SetUpEnv()
        {
            using (IConnection connection = GetRabbitMqConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(RabbitMqService.ExchangeName, ExchangeType.Topic);
                    channel.QueueDeclare(RabbitMqService.RequestQueueName, true, false, false, null);
                    channel.QueueBind(RabbitMqService.RequestQueueName, RabbitMqService.ExchangeName, "request");

                    channel.ExchangeDeclare(RabbitMqService.ExchangeName, ExchangeType.Topic);
                    channel.QueueDeclare(RabbitMqService.ResponseQueueName, true, false, false, null);
                    channel.QueueBind(RabbitMqService.ResponseQueueName, RabbitMqService.ExchangeName, "response");
                }
            }
        }

        public IConnection GetRabbitMqConnection()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            return connectionFactory.CreateConnection();
        }
    }
}
