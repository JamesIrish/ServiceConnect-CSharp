﻿using System;
using System.Collections.Generic;

namespace R.MessageBus.Interfaces
{
    public interface IConfiguration
    {
        Type Container { get; set; }
        Type ConsumerType { get; set; }
        Type ProducerType { get; set; }
        Type ProcessManagerFinder { get; set; }
        bool ScanForMesssageHandlers { get; set; }
        string PersistenceStoreConnectionString { get; set; }
        string PersistenceStoreDatabaseName { get; set; }
        ITransportSettings TransportSettings { get; set; }
        IDictionary<string, string> QueueMappings { get; set; }

        /// <summary>
        /// Adds a message queue mapping. 
        /// </summary>
        /// <param name="messageType">Type of message</param>
        /// <param name="queue">Queue to send the message to</param>
        void AddQueueMapping(Type messageType, string queue);

        /// <summary>
        /// Load configuration from file path an initialize Transport Settings
        /// </summary>
        /// <param name="configFilePath"></param>
        /// <param name="endPoint"></param>
        void LoadSettings(string configFilePath = null, string endPoint = null);

        /// <summary>
        /// Sets the consumer type.
        /// </summary>
        /// <typeparam name="T">The type must be a class that implements IConsumer.</typeparam>
        void SetConsumer<T>() where T : class, IConsumer;

        /// <summary>
        /// Sets the publisher type.
        /// </summary>
        /// <typeparam name="T">The type must be a class that implements IPublisher.</typeparam>
        void SetProducer<T>() where T : class, IProducer;

        /// <summary>
        /// Sets the container.
        /// </summary>
        /// <typeparam name="T">The type must be a class that implements IBusContainer.</typeparam>
        void SetContainer<T>() where T : class, IBusContainer;

        /// <summary>
        /// Sets the process manager finder
        /// </summary>
        /// <typeparam name="T">The type must be a class that implements IProcessManagerFinder</typeparam>
        void SetProcessManagerFinder<T>() where T : class, IProcessManagerFinder;

        /// <summary>
        /// Sets QueueName
        /// </summary>
        void SetQueueName(string queueName);

        /// <summary>
        /// Gets queue name.
        /// </summary>
        /// <returns></returns>
        string GetQueueName();

        /// <summary>
        /// Gets an instance of the consumer.
        /// </summary>
        /// <returns></returns>
        IConsumer GetConsumer();

        /// <summary>
        /// Gets an instance of the publisher.
        /// </summary>
        /// <returns></returns>
        IProducer GetProducer();

        /// <summary>
        /// Gets an instance of the container.
        /// </summary>
        /// <returns></returns>
        IBusContainer GetContainer();

        /// <summary>
        /// Gets an instance of the ProcessManagerFinder
        /// </summary>
        /// <returns></returns>
        IProcessManagerFinder GetProcessManagerFinder();
    }
}