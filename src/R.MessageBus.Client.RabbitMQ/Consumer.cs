﻿using System;
using System.Collections.Generic;
using log4net;
using R.MessageBus.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ConsumerEventHandler = R.MessageBus.Interfaces.ConsumerEventHandler;

namespace R.MessageBus.Client.RabbitMQ
{
    public class Consumer : IDisposable, IConsumer
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ITransportSettings _transportSettings;
        private IConnection _connection;
        private IModel _model;
        private ConsumerEventHandler _consumerEventHandler;
        private string _errorExchange;
        private readonly int _retryDelay;
        private readonly int _maxRetries;
        private string _queueName;

        public Consumer(ITransportSettings transportSettings)
        {
            _transportSettings = transportSettings;

            _retryDelay = transportSettings.RetryDelay;
            _maxRetries = transportSettings.MaxRetries;
        }

        public void Event(IBasicConsumer consumer, BasicDeliverEventArgs args)
        {
            var success = _consumerEventHandler(args.Body);
            _model.BasicAck(args.DeliveryTag, false);

            if (!success)
            {
                int retryCount = 0;

                if (null == args.BasicProperties.Headers)
                    args.BasicProperties.Headers = new Dictionary<string, object>();

                if (args.BasicProperties.Headers.ContainsKey("RetryCount"))
                {
                    retryCount = (int)args.BasicProperties.Headers["RetryCount"];
                    args.BasicProperties.Headers.Remove("RetryCount");
                }

                if (retryCount < _maxRetries)
                {
                    retryCount++;
                    args.BasicProperties.Headers.Add("RetryCount", retryCount);

                    _model.BasicPublish(string.Empty, _queueName, args.BasicProperties, args.Body);
                }
                else
                {
                    Logger.ErrorFormat("Max number of retries exceeded. MessageId : {0}", args.BasicProperties.MessageId);

                    _model.BasicPublish(_errorExchange, string.Empty, args.BasicProperties, args.Body);
                }
            }
        }

        public void StartConsuming(ConsumerEventHandler messageReceived, string messageTypeName, string queueName)
        {
            _consumerEventHandler = messageReceived;
            // Initialize _queueName here rather than in Consumer.ctor from TransportSettings.Queue.Name
            // TransportSettings.Queue.Name may overriden in the Bus and queueName parameter might have a newer value 
            // than TransportSettings.Queue.Name would in Consumer.ctor
            _queueName = queueName; 
            
            var connectionFactory = new ConnectionFactory 
            {
                HostName = _transportSettings.Host,
                VirtualHost = "/",
                Protocol = Protocols.FromEnvironment(),
                Port = AmqpTcpEndpoint.UseDefaultPort
            };

            if (!string.IsNullOrEmpty(_transportSettings.Username))
            {
                connectionFactory.UserName = _transportSettings.Username;
            }

            if (!string.IsNullOrEmpty(_transportSettings.Password))
            {
                connectionFactory.Password = _transportSettings.Password;
            }

            _connection = connectionFactory.CreateConnection();

            _model = _connection.CreateModel();

            // WORK QUEUE
            string exchange = ConfigureExchange(messageTypeName);
            queueName = ConfigureQueue();

            if (!string.IsNullOrEmpty(exchange))
            {
                _model.QueueBind(queueName, exchange, string.Empty);
            }

            //RETRY QUEUE
            ConfigureRetryQueue();

            //ERROR QUEUE
            _errorExchange = ConfigureErrorExchange();
            var errorQueue = ConfigureErrorQueue();

            if (!string.IsNullOrEmpty(_errorExchange))
            {
                _model.QueueBind(errorQueue, _errorExchange, string.Empty, null);
            }

            var consumer = new EventingBasicConsumer();
            consumer.Received += Event;
            _model.BasicConsume(queueName, _transportSettings.NoAck, consumer);
        }

        private string ConfigureQueue()
        {
            var arguments = _transportSettings.Queue.Arguments;

            return _model.QueueDeclare(_queueName, _transportSettings.Queue.Durable, _transportSettings.Queue.Exclusive, _transportSettings.Queue.AutoDelete, arguments);
        }

        private void ConfigureRetryQueue()
        {
            // When message goes to retry queue, it falls-through to dead-letter exchange (after _retryDelay)
            // dead-letter exchange is of type "direct" and bound to the original queue.
            string retryDeadLetterExchangeName = _queueName + ".Retries.DeadLetter";
            _model.ExchangeDeclare(retryDeadLetterExchangeName, "direct");
            _model.QueueBind(_queueName, retryDeadLetterExchangeName, _queueName, null); // only redeliver to the original queue (use _queueName as routing key)

            var arguments = new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", retryDeadLetterExchangeName},
                {"x-message-ttl", _retryDelay}
            };

            string retryQueueName = _queueName + ".Retries";

            _model.QueueDeclare(retryQueueName, true, false, false, arguments);
        }

        private string ConfigureErrorQueue()
        {
            return _model.QueueDeclare("errors", true, false, false, null);
        }

        private string ConfigureExchange(string exchangeName)
        {
            _model.ExchangeDeclare(exchangeName, "fanout", true);

            return exchangeName;
        }

        private string ConfigureErrorExchange()
        {
            _model.ExchangeDeclare("errors", "direct");

            return "errors";
        }

        public void StopConsuming()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_connection != null)
                _connection.Close();
            if (_model != null)
                _model.Abort();
        }
    }
}