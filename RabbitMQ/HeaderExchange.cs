using Modules;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQforNETCore.Models;
using System;
using System.Text;

namespace RabbitMQforNETCore.RabbitMQ
{
    public class HeaderExchange : IRabbitMQProcess
    {
        internal delegate void delEventHandler(string Message);
        internal event delEventHandler MessageReceived;

        private SystemDefinitions _queue;
        private ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;

        public OperationResult Close()
        {
            try
            {
                if (_channel == null || _connection == null)
                    return new OperationResult { Message = "", Result = false };

                _channel.Close();
                _connection.Close();
                return new OperationResult { Message = "", Result = true };
            }
            catch (Exception ex)
            {
                return new OperationResult { Message = ex.ToString(), Result = false };
            }
        }

        public OperationResult Listen(dynamic model = null)
        {
            OperationResult op = new OperationResult();

            try
            {
                if (_channel.IsClosed)      //RabbitMQ ile bağlantı kopmuş ise tekrar yapalım.
                    Login(_queue);

                var queueName = _channel.QueueDeclare().QueueName;

                _queue.RoutingKey = "";
                _channel.QueueBind(queue: queueName, exchange: _queue.Exchange, routingKey: _queue.RoutingKey, arguments: model);

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                var consumer = new EventingBasicConsumer(_channel);
                _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

                consumer.Received += (render, argument) =>
                {
                    string message = Encoding.UTF8.GetString(argument.Body.ToArray());
                    MessageReceived(message);
                    _channel.BasicAck(deliveryTag: argument.DeliveryTag, false);
                };

            }
            catch (Exception ex)
            {
                op.Result = false;
                op.Message = ex.Message;
            }

            return op;
        }

        public OperationResult Login(SystemDefinitions Queue)
        {
            OperationResult op = new OperationResult();
            try
            {
                _queue = Queue;

                #region ConnectionFactory
                _factory = new ConnectionFactory();
                #endregion

                #region Connection and channel
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(
                                        exchange: _queue.Exchange,
                                        durable: false,
                                        type: ExchangeType.Headers
                                    );

                #endregion

            }
            catch (Exception ex)
            {
                op.Result = false;
                op.Message = ex.Message;
            }

            return op;
        }

        public OperationResult Send(SendModel sendModel)
        {
            OperationResult op = new OperationResult();
            try
            {
                if (_channel.IsClosed)      //RabbitMQ ile bağlantı kopmuş ise tekrar yapalım.
                    Login(_queue);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.Headers = sendModel.Header; // header exchange için header bilgileri verildi. RoutingKey bilgisine ihtiyaç yoktur.

                _queue.RoutingKey = "";

                //Mesaj dönüştürülüyor...
                byte[] body = Encoding.UTF8.GetBytes(sendModel.Message);

                // bu method döngüye girebilir. routingKey elemanlı bir döngü olacaktır.
                _channel.BasicPublish(exchange: _queue.Exchange, routingKey: _queue.RoutingKey, basicProperties: properties, body: body);
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
