using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQPoc.Service;
using RabbitMQPoC.Model;

namespace WpfRabbitMQConsumer
{
    //    /// <summary>
    //    /// Interaction logic for MainWindow.xaml
    //    /// </summary>
    //    public partial class MainWindow : Window
    //    {
    //        DispatcherTimer dt = new DispatcherTimer();

    //        public MainWindow()
    //        {
    //            InitializeComponent();
    //            dt.Interval = TimeSpan.FromSeconds(1);
    //            dt.IsEnabled = true;
    //            dt.Tick += Dt_Tick;
    //        }

    //        private void Dt_Tick(object sender, EventArgs e)
    //        {
    //            this.TextBlock.Text += Environment.NewLine;
    //            this.TextBlock.Text += GetMessage();
    //        }

    //        private string GetMessage()
    //        {
    //            RabbitMqService rabbitMqService = new RabbitMqService();
    //            IConnection connection = rabbitMqService.GetRabbitMqConnection();
    //            IModel model = connection.CreateModel();

    //            model.BasicQos(0, 1, false);
    //            QueueingBasicConsumer consumer = new QueueingBasicConsumer(model);
    //            model.BasicConsume(RabbitMqService.QueueName, false, consumer);

    //            BasicDeliverEventArgs deliveryArguments;
    //            consumer.Queue.Dequeue(500, out deliveryArguments);
    //            if (deliveryArguments != null)
    //            {

    //                String jsonified = Encoding.UTF8.GetString(deliveryArguments.Body);
    //                return jsonified;
    //                //RabbitMqMessage message = JsonConvert.DeserializeObject<RabbitMqMessage>(jsonified);
    //            }

    //            return "";
    //        }

    //    }



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dt = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            RabbitMqService rabbitMqService = new RabbitMqService();
            IConnection connection = rabbitMqService.GetRabbitMqConnection();
            IModel channel = connection.CreateModel();

            channel.BasicQos(0, 1, false);
            EventingBasicConsumer eventingBasicConsumer = new EventingBasicConsumer(channel);

            eventingBasicConsumer.Received += (sender, basicDeliveryEventArgs) =>
            {
                IBasicProperties basicProperties = basicDeliveryEventArgs.BasicProperties;

                Debug.WriteLine(string.Concat("Message received from the exchange ", basicDeliveryEventArgs.Exchange));
                Debug.WriteLine(string.Concat("Message: ", Encoding.UTF8.GetString(basicDeliveryEventArgs.Body)));
                StringBuilder headersBuilder = new StringBuilder();
                headersBuilder.Append("Headers: ").Append(Environment.NewLine);
                foreach (var kvp in basicProperties.Headers)
                {
                    headersBuilder.Append(kvp.Key).Append(": ").Append(Encoding.UTF8.GetString(kvp.Value as byte[])).Append(Environment.NewLine);
                }
                Debug.WriteLine(headersBuilder.ToString());
                channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);


                this.TextBlock.Text += Environment.NewLine;
                this.TextBlock.Text += DateTime.Now.ToString("G");
                this.TextBlock.Text += " ";
                this.TextBlock.Text += headersBuilder.ToString();
            };

            channel.BasicConsume("company.queue.headers", false, eventingBasicConsumer);
        }
    }
}
