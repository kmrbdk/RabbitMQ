using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ
{
    // RabbitMQ ile bağlantı parametrelerini tanımlayan sınıf...
    public class SystemDefinitions
    {
        #region User variables
        public string UserName { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public string Uri { get; set; }

        public byte[] Message { get; set; }

        public string Exchange { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }

        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }
        public IDictionary<string, object> Arguments { get; set; }

        public bool AutoAck { get; set; }
        #endregion

        public SystemDefinitions()
        {
            this.UserName = "guest";
            this.Password = "guest";
            this.HostName = "localhost";
            this.Uri = "";

            this.Exchange = ""; // anahtar kelimeye bakarak ilgili kuyruğa gönderimi sağlıyor
            this.QueueName = "";
            this.RoutingKey = ""; //anahtar kelime

            this.Message = null;

            this.Durable = true;
            this.Exclusive = true;
            this.AutoDelete = false;
            this.Arguments = null;

            this.AutoAck = true;        //mesaj teslim edilidği an silinsin...
        }
    }
}
