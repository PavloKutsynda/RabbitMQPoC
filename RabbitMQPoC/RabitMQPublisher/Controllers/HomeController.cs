using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQPoc.Service;
using RabbitMQPoC.Model;

namespace RabitMQPublisher.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            RabbitMqService rabbitMqService = new RabbitMqService();
            IConnection connection = rabbitMqService.GetRabbitMqConnection();
            IModel model = connection.CreateModel();
            
            SetupSerialisationMessageQueue(model);
            PublishMessage(model);

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
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

        private void PublishMessage(IModel model)
        {
            var message = new RabbitMqMessage()
            {
                Text = "Message"
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