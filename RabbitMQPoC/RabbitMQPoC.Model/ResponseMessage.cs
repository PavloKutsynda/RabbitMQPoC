using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQPoC.Model
{
    public class ResponseMessage
    {
        public int Id { get; set; }

        public bool Handled { get; set; }
    }
}
