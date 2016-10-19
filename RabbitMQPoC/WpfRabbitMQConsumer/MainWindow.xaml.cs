using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            SetUpRabbitMqFirstMessageConsumer();
            SetUpRabbitMqSecondMessageConsumer();
        }

        private void SetUpRabbitMqFirstMessageConsumer()
        {
            RabbitMqService rabbitMqService = new RabbitMqService();
            IConnection connection = rabbitMqService.GetRabbitMqConnection();
            IModel channel = connection.CreateModel();

            EventingBasicConsumer eventingBasicConsumer = new EventingBasicConsumer(channel);
            eventingBasicConsumer.Received += FirstMessageConsumerOnReceived(channel);
            channel.BasicConsume(RabbitMqService.QueueName, false, eventingBasicConsumer);
        }

        private EventHandler<BasicDeliverEventArgs> FirstMessageConsumerOnReceived(IModel channel)
        {
            return (sender, basicDeliveryEventArgs) =>
            {
                var message = string.Format("{0} {1}", DateTime.Now.ToString("h:mm:ss"), Encoding.UTF8.GetString(basicDeliveryEventArgs.Body));

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    this.TextBlock.Text += string.Format("{0}  FirstMessageConsumer  - {1}", Environment.NewLine, message);
                }));

                if (basicDeliveryEventArgs.BasicProperties.ContentType == "FirstMessage")
                {
                    channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);
                }
            };
        }

        private void SetUpRabbitMqSecondMessageConsumer()
        {
            RabbitMqService rabbitMqService = new RabbitMqService();
            IConnection connection = rabbitMqService.GetRabbitMqConnection();
            IModel channel = connection.CreateModel();

            EventingBasicConsumer eventingBasicConsumer = new EventingBasicConsumer(channel);
            eventingBasicConsumer.Received += SecondMessageConsumerOnReceived(channel);
            channel.BasicConsume(RabbitMqService.QueueName, false, eventingBasicConsumer);
        }

        private EventHandler<BasicDeliverEventArgs> SecondMessageConsumerOnReceived(IModel channel)
        {
            return (sender, basicDeliveryEventArgs) =>
            {
                var message = string.Format("{0} {1}", DateTime.Now.ToString("h:mm:ss"), Encoding.UTF8.GetString(basicDeliveryEventArgs.Body));

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    this.TextBlock.Text += string.Format("{0} SecondMessageConsumer - {1}", Environment.NewLine, message);
                }));

                if (basicDeliveryEventArgs.BasicProperties.ContentType == "SecondMessage")
                {
                    channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);
                }
            };
        }
    }
}
