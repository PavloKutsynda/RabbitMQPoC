using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
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

        #region Request 

        private static int RequestCounter;

        private void Publish_Request_Message(object sender, RoutedEventArgs e)
        {
            RabbitMqService rabbitMqService = new RabbitMqService();
            IConnection connection = rabbitMqService.GetRabbitMqConnection();
            IModel model = connection.CreateModel();

            SetupRequestQueue(model);
            PublishRequestMessage(model);
        }

        private void SetupRequestQueue(IModel model)
        {
            model.QueueDeclare(RabbitMqService.RequestQueueName, true, false, false, null);
        }

        private void PublishRequestMessage(IModel model)
        {
            var requestMessage = new RequestMessage()
            {
                Id = ++RequestCounter,
                Text = this.RequestText.Text
            };

            IBasicProperties basicProperties = model.CreateBasicProperties();
            basicProperties.SetPersistent(false);

            var messageHeaders = new Dictionary<string, object>
            {
                {"type", "requestMessage"}
            };
            basicProperties.Headers = messageHeaders;
            basicProperties.ContentType = "requestMessage";

            String jsonified = JsonConvert.SerializeObject(requestMessage);
            byte[] messageBuffer = Encoding.UTF8.GetBytes(jsonified);
            model.BasicPublish("", RabbitMqService.RequestQueueName, basicProperties, messageBuffer);
        }

        #endregion
    }
}
