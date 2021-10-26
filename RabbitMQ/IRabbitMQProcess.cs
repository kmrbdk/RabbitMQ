using System;
using System.Collections.Generic;
using System.Text;
using Modules;
using RabbitMQ.Client.Events;

namespace RabbitMQ
{
    // Tüm rabbit mq mesaj gönderme yöntemleri için kullanılacak interface
    public interface IRabbitMQProcess
    {        
        OperationResult Login(SystemDefinitions Queue);     // RabbitMQ ile kanal (queue / exchange) oluşturma
        OperationResult Send(string Message);               // Oluşturulan kanala (queue / exchange) mesaj gönderme
        OperationResult Listen();                           // Oluşturulan kanaldan (queue / exchange) mesajları almak                
        OperationResult Register(MessageCallBack Object);   // Oluşturulan kanaldan (queue / exchange) mesajları almak veya Event kullanılabilir
    }
}
