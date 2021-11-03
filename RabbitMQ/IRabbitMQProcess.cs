using Modules;
using RabbitMQforNETCore.Models;

namespace RabbitMQ
{
    // Tüm rabbit mq mesaj gönderme yöntemleri için kullanılacak interface
    public interface IRabbitMQProcess
    {
        OperationResult Login(SystemDefinitions Queue);     // RabbitMQ ile kanal (queue / exchange) oluşturma
        OperationResult Close();                            // RabbitMQ kanal kapatma
        OperationResult Send(SendModel sendModel);          // Oluşturulan kanala (queue / exchange) mesaj gönderme
        OperationResult Listen(dynamic model = null);                           // Oluşturulan kanaldan (queue / exchange) mesajları almak                
        //OperationResult Register(MessageCallBack Object);   // Oluşturulan kanaldan (queue / exchange) mesajları almak veya Event kullanılabilir
    }
}
