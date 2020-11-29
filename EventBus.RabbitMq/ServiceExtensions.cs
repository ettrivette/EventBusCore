using EventBus.Abstractions;
using EventBus.Abstractions.Events;
using EventBus.Abstractions.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace EventBus.RabbitMq
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRabbitMqEventBus(this IServiceCollection serviceProvider,
            Action<RabbitMQConfig> configuration)
        {
            RabbitMQConfig config = new RabbitMQConfig()
            {
                ExchangeName = "Shared",
                QueueName = "EventQueue"
            };

            configuration(config);

            serviceProvider.AddSingleton<ISubscriptionManager, InMemorySubscriptionManager>();
            
            serviceProvider.AddSingleton<IRabbitMqConnectionManager, PersistenRabbitMqConnectionManager>();
            
            serviceProvider.AddSingleton<IConnectionFactory>((services) =>
            {
                return new ConnectionFactory() 
                { 
                    HostName = config.ConnectionString,
                    UserName = ConnectionFactory.DefaultUser,
                    Password = ConnectionFactory.DefaultPass,
                    Port = AmqpTcpEndpoint.UseDefaultPort,
                    DispatchConsumersAsync = true
                };
            });

            serviceProvider.AddSingleton<IEventBus, RabbitMqEventBus>((services) =>
            {
                var eventBus = new RabbitMqEventBus(services.GetService<ISubscriptionManager>(), 
                    services.GetService<IRabbitMqConnectionManager>(), 
                    services,
                    config.ExchangeName,
                    config.QueueName);

                return eventBus;
            });

            return serviceProvider;
        }

        public class RabbitMQConfig
        {
            public string ConnectionString { get; set; }
            
            public string ExchangeName { get; set; }

            public string QueueName { get; set; }
        }
    }
}
