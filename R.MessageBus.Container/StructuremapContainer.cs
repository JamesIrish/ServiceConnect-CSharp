﻿using System;
using System.Collections.Generic;
using System.Linq;
using R.MessageBus.Interfaces;
using StructureMap;
using StructureMap.Query;

namespace R.MessageBus.Container
{
    public class StructuremapContainer : IBusContainer
    {
        public IEnumerable<HandlerReference> GetHandlerTypes()
        {
            IEnumerable<InstanceRef> instances = ObjectFactory.Container.Model.AllInstances.Where(i => i.PluginType.Name == typeof(IMessageHandler<>).Name ||
                                                                                                       i.PluginType.Name == typeof(IStartProcessManager<>).Name);
            return instances.Where(instance => instance.ConcreteType != null && !string.IsNullOrEmpty(instance.ConcreteType.Name))
                            .Select(instance => new HandlerReference
            {
                MessageType = instance.PluginType.GetGenericArguments()[0],
                HandlerType = instance.ConcreteType
            });
        }

        public IEnumerable<HandlerReference> GetHandlerTypes(Type messageHandler)
        {
            return ObjectFactory.Container.Model.AllInstances.Where(i => i.PluginType == messageHandler).Select(instance => new HandlerReference
            {
                MessageType = instance.PluginType.GetGenericArguments()[0],
                HandlerType = instance.ConcreteType
            });
        }

        public object GetHandlerInstance(Type handlerType)
        {
            return ObjectFactory.GetInstance(handlerType);
        }

        public void ScanForHandlers()
        {
            ObjectFactory.Configure(x => x.Scan(y =>
            {
                y.AssembliesFromApplicationBaseDirectory();
                y.ConnectImplementationsToTypesClosing(typeof(IMessageHandler<>));
            }));
        }
    }
}