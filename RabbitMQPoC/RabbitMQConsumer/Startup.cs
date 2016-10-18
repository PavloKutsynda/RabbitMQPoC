using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RabbitMQConsumer.Startup))]
namespace RabbitMQConsumer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
