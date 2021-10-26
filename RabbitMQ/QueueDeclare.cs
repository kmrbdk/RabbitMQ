using Newtonsoft.Json;
using RabbitMQ.Client;
using Modules;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client.Events;
using System.Windows.Forms;
using RabbitMQforNETCore;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;

namespace RabbitMQ
{
    class QueueDeclare : IRabbitMQProcess
    {
        #region User variables
        internal delegate void delEventHandler(string Message);
        internal event delEventHandler MessageReceived;

        private SystemDefinitions _queue;
        private ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;

        public MessageCallBack _object;
        #endregion

        public OperationResult Login(SystemDefinitions Queue)
        {
            OperationResult op = new OperationResult();
            try
            {
                _queue = Queue;

                #region ConnectionFactory
                _factory = new ConnectionFactory
                {
                    UserName = _queue.UserName,
                    Password = _queue.Password,
                    HostName = _queue.HostName
                    //Uri = new Uri("amqp://quest:quest@localhost:5672")
                };
                #endregion

                #region Connection and channel
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(
                                        queue: _queue.QueueName,
                                        durable: _queue.Durable,
                                        exclusive: _queue.Exclusive,
                                        autoDelete: _queue.AutoDelete,
                                        arguments: _queue.Arguments
                                    );
                #endregion

                //Tam şu anda RabbitMQ Queues sekmesine bakılırsa ilgili Queue oluşmuş olmalı...
            }
            catch (Exception ex)
            {
                op.Result = false;
                op.Message = ex.Message;
            }

            return op;
        }
        public OperationResult Send(string Message)
        {
            OperationResult op = new OperationResult();
            try
            {
                _queue.Exchange = "";       //Yanlışlıkla Exchange tanımlanmış olabilir. QueueDeclare de bu kullanılmıyor.
                if (_channel.IsClosed)      //RabbitMQ ile bağlantı kopmuş ise tekrar yapalım.
                    Login(_queue);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                //properties.Headers = new Dictionary<string, object>();
                //properties.Headers.Add("Header1", "Deneme 1");
                //properties.Headers.Add("Header2", "Deneme 2");
                //properties.DeliveryMode = 2;
                //properties.Expiration = "36000000";


                //Mesaj dönüştürülüyor...
                byte[] body = Encoding.UTF8.GetBytes(Message);

                _channel.BasicPublish(exchange: _queue.Exchange, routingKey: _queue.QueueName, basicProperties: properties, body: body);
            }
            catch (Exception ex)
            {
                op.Result = false;
                op.Message = ex.Message;
            }

            return op;
        }
        public OperationResult Listen()
        {
            OperationResult op = new OperationResult();

            try
            {
                _queue.Exchange = "";       //Yanlışlıkla Exchange tanımlanmış olabilir. QueueDeclare de bu kullanılmıyor.
                if (_channel.IsClosed)      //RabbitMQ ile bağlantı kopmuş ise tekrar yapalım.
                    Login(_queue);

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += Consumer_Received;

                _channel.BasicConsume(queue: _queue.QueueName, autoAck: _queue.AutoAck, consumer: consumer);
            }
            catch (Exception ex)
            {
                op.Result = false;
                op.Message = ex.Message;
            }

            return op;
        }
        internal void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            string message;
            Byte[] body;
            try
            {
                body = e.Body.ToArray();
                message = Encoding.UTF8.GetString(body);

                //AutoAck = true verilmiş ise zaten teslim edildiği ana silineceği için hata alacaktır. Bu yüzden disable edildi. [Otomatik siliniyor]
                //_channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);

                MessageReceived(message);    //Event i dinleyen varsa iletelim.

                //Sisteme register olmuş ise ilgili nesneye mesajı iletelim.
                if (_object != null)
                {
                    string[] args = new string[] { message };

                    var method = ((object)_object.Object).GetType().GetMethod(_object.MethodName);
                    method.Invoke(_object.Object, args);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        public OperationResult Register(MessageCallBack Object)
        {
            OperationResult op = new OperationResult();

            try
            {
                if (Object != null)         //Gelen mesajların dönüş noktası istenmiş.
                    _object = Object;
            }
            catch (Exception ex)
            {
                op.Result = false;
                op.Message = ex.Message;
            }

            return op;
        }
    }
}
