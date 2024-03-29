﻿using Modules;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQforNETCore.Models;
using System;
using System.Text;

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

        //public MessageCallBack _object;
        #endregion

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

                _channel.QueueDeclare(
                                       queue: _queue.QueueName,
                                       durable: false,
                                       exclusive: false,
                                       autoDelete: false,
                                       arguments: null
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
        public OperationResult Send(SendModel sendModel)
        {
            OperationResult op = new OperationResult();
            try
            {
                _queue.Exchange = "";
                if (_channel.IsClosed)      //RabbitMQ ile bağlantı kopmuş ise tekrar yapalım.
                    Login(_queue);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                //Mesaj dönüştürülüyor...
                byte[] body = Encoding.UTF8.GetBytes(sendModel.Message);

                _channel.BasicPublish(exchange: _queue.Exchange, routingKey: _queue.QueueName, basicProperties: properties, body: body);
            }
            catch (Exception ex)
            {
                op.Result = false;
                op.Message = ex.Message;
            }

            return op;
        }
        public OperationResult Listen(dynamic model = null)
        {
            OperationResult op = new OperationResult();

            try
            {
                _queue.Exchange = "";
                if (_channel.IsClosed)      //RabbitMQ ile bağlantı kopmuş ise tekrar yapalım.
                    Login(_queue);

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (render, argument) =>
                {
                    string message = Encoding.UTF8.GetString(argument.Body.ToArray());
                    MessageReceived(message);
                };

                _channel.BasicConsume(queue: _queue.QueueName, autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                op.Result = false;
                op.Message = ex.Message;
            }

            return op;
        }
        //internal void Consumer_Received(object sender, BasicDeliverEventArgs e)
        //{
        //    string message;
        //    Byte[] body;
        //    try
        //    {
        //        body = e.Body.ToArray();
        //        message = Encoding.UTF8.GetString(body);

        //        //AutoAck = true verilmiş ise zaten teslim edildiği ana silineceği için hata alacaktır. Bu yüzden disable edildi. [Otomatik siliniyor]
        //        //_channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);

        //        MessageReceived(message);    //Event i dinleyen varsa iletelim.

        //        //Sisteme register olmuş ise ilgili nesneye mesajı iletelim.
        //        if (_object != null)
        //        {
        //            string[] args = new string[] { message };

        //            var method = ((object)_object.Object).GetType().GetMethod(_object.MethodName);
        //            method.Invoke(_object.Object, args);
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine(exception);
        //    }
        //}
        //public OperationResult Register(MessageCallBack Object)
        //{
        //    OperationResult op = new OperationResult();

        //    try
        //    {
        //        if (Object != null)         //Gelen mesajların dönüş noktası istenmiş.
        //            _object = Object;
        //    }
        //    catch (Exception ex)
        //    {
        //        op.Result = false;
        //        op.Message = ex.Message;
        //    }

        //    return op;
        //}
    }
}
