﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQPoc.Service;
using RabbitMQPoC.Model;

namespace WpfRabbitMQConsumer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RabbitMqService rabbitMqService = new RabbitMqService();
        private IConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            connection = rabbitMqService.GetRabbitMqConnection();
            SetUpRequestConsumer();
        }

        #region Handle request

        private void SetUpRequestConsumer()
        {
            IModel channel = connection.CreateModel();
            EventingBasicConsumer eventingBasicConsumer = new EventingBasicConsumer(channel);
            eventingBasicConsumer.Received += RequestConsumerOnReceived(channel);
            channel.BasicConsume(RabbitMqService.RequestQueueName, false, eventingBasicConsumer);
        }

        private EventHandler<BasicDeliverEventArgs> RequestConsumerOnReceived(IModel channel)
        {
            return (sender, basicDeliveryEventArgs) =>
            {
                var message = string.Format("{0} {1}", DateTime.Now.ToString("h:mm:ss"), Encoding.UTF8.GetString(basicDeliveryEventArgs.Body));

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    this.RequestTextBox.Text += string.Format("{0}Request - {1}", Environment.NewLine, message);
                }));
                channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);

                var requestMessage = JsonConvert.DeserializeObject<RequestMessage>(Encoding.UTF8.GetString(basicDeliveryEventArgs.Body));
                PublishResponseMessage(requestMessage);
            };
        }

        #endregion

        #region Response
        
        private void PublishResponseMessage(RequestMessage requestMessage)
        {
            IModel channel = connection.CreateModel();

            PublicationAddress address = new PublicationAddress(ExchangeType.Topic, RabbitMqService.ExchangeName, "response");

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.SetPersistent(false);

            byte[] messageBuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ResponseMessage
            {
                Id = requestMessage.Id,
                Handled = true
            }));
            
            channel.BasicPublish(address, basicProperties, messageBuffer);
        }

        #endregion
    }
}
