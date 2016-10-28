﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
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

namespace WpfRabbitMqPublisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private RabbitMqService rabbitMqService = new RabbitMqService();
        private IConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            connection = rabbitMqService.GetRabbitMqConnection();
            SetUpResponseConsumer();
        }

        #region Request 

        private static int RequestCounter;

        private void Publish_Request_Message(object sender, RoutedEventArgs e)
        {
            IModel channel = connection.CreateModel();

            PublicationAddress address = new PublicationAddress(ExchangeType.Topic, RabbitMqService.ExchangeName, "request");

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.SetPersistent(false);

            byte[] messageBuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new RequestMessage
            {
                Id = ++RequestCounter,
                Text = this.RequestText.Text
            }));

            channel.BasicPublish(address, basicProperties, messageBuffer);
        }

        #endregion

        #region Response

        private void SetUpResponseConsumer()
        {
            IModel channel = connection.CreateModel();
            EventingBasicConsumer eventingBasicConsumer = new EventingBasicConsumer(channel);
            eventingBasicConsumer.Received += ResponseConsumerOnReceived(channel);
            channel.BasicConsume(RabbitMqService.ResponseQueueName, false, eventingBasicConsumer);
        }

        private EventHandler<BasicDeliverEventArgs> ResponseConsumerOnReceived(IModel channel)
        {
            return (sender, basicDeliveryEventArgs) =>
            {
                var message = string.Format("{0} {1}", DateTime.Now.ToString("h:mm:ss"), Encoding.UTF8.GetString(basicDeliveryEventArgs.Body));

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    this.ResponseTextBox.Text += string.Format("{0}Response - {1}", Environment.NewLine, message);
                }));

                channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);
            };
        }

        #endregion
    }
}
