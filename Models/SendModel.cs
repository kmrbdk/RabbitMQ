using System.Collections.Generic;

namespace RabbitMQforNETCore.Models
{
    public class SendModel
    {
        public string Message { get; set; }
        public Dictionary<string, object> Header { get; set; }
    }
}
