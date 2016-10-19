using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQPoc.Service;
using RabbitMQPoC.Model;

namespace WpfRabbitMqPublisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Publish_FirstMessage(object sender, RoutedEventArgs e)
        {
            RabbitMqService rabbitMqService = new RabbitMqService();
            IConnection connection = rabbitMqService.GetRabbitMqConnection();
            IModel model = connection.CreateModel();

            SetupSerialisationMessageQueue(model);
            PublishFirstMessage(model);
        }

        private void Publish_SecondMessage(object sender, RoutedEventArgs e)
        {
            RabbitMqService rabbitMqService = new RabbitMqService();
            IConnection connection = rabbitMqService.GetRabbitMqConnection();
            IModel model = connection.CreateModel();

            SetupSerialisationMessageQueue(model);
            PublishSecondMessage(model);
        }


        #region Private methods

        private void SetupInitialTopicQueue(IModel model)
        {
            model.QueueDeclare("queueFromVisualStudio", true, false, false, null);
            model.ExchangeDeclare("exchangeFromVisualStudio", ExchangeType.Topic);
            model.QueueBind("queueFromVisualStudio", "exchangeFromVisualStudio", "superstars");
        }

        private void SetupSerialisationMessageQueue(IModel model)
        {
            model.QueueDeclare(RabbitMqService.QueueName, true, false, false, null);
        }

        private void PublishFirstMessage(IModel model)
        {
            var message = new RabbitMqMessage()
            {
                Text = "First Message"
            };

            IBasicProperties basicProperties = model.CreateBasicProperties();
            basicProperties.SetPersistent(true);
            String jsonified = JsonConvert.SerializeObject(message);
            byte[] messageBuffer = Encoding.UTF8.GetBytes(jsonified);
            model.BasicPublish("", RabbitMqService.QueueName, basicProperties, messageBuffer);
        }

        private void PublishSecondMessage(IModel model)
        {
            var message = new RabbitMqMessage()
            {
                Text = "Second Message"
            };

            IBasicProperties basicProperties = model.CreateBasicProperties();
            basicProperties.SetPersistent(true);
            String jsonified = JsonConvert.SerializeObject(message);
            byte[] messageBuffer = Encoding.UTF8.GetBytes(jsonified);
            model.BasicPublish("", RabbitMqService.QueueName, basicProperties, messageBuffer);
        }
        #endregion
    }
}
