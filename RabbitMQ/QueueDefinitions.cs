using System.Collections.Generic;

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
            this.UserName = "";
            this.Password = "";
            this.HostName = "";
            this.Uri = "";

            this.Exchange = ""; // anahtar kelimeye bakarak ilgili kuyruğa gönderimi sağlıyor
            this.QueueName = ""; // mesaj kuyruk adı
            this.RoutingKey = ""; // anahtar kelime

            this.Message = null;

            this.Durable = false; // queue cache demi yoksa fiziksel olarak mı saklanacak? sunucu restart durumuna göre mesajların kaybolmamması için true edilebilir.
                                  // performans için false yapılabilir.
            this.Exclusive = true; // diğer connection lar ile beraber kullanılmasına izin verilir.
            this.AutoDelete = false; // ilgili queue leri silmek için kullanılır.
            this.Arguments = null; // exchange tipleri ile ilgili parametreler (Header exchange için header bloku)

            this.AutoAck = true;        // mesaj teslim edilidği an silinsin...
        }
    }
}
