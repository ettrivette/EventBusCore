using EventBus.Abstractions;
using EventBus.Abstractions.Events;
using EventBus.Abstractions.Subscriptions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace EventBus.RabbitMq
{
    public class RabbitMqEventBus : IEventBus
    {
        private readonly ISubscriptionManager subscriptionManager;
        private readonly IRabbitMqConnectionManager connection;
        private readonly IServiceProvider serviceProvider;
        private readonly string exchangeName;
        private readonly string queueName;

        //A channel for handling incoming events
        private IModel consumerChannel;

        public RabbitMqEventBus(ISubscriptionManager subscriptionManager,
            IRabbitMqConnectionManager connection,
            IServiceProvider serviceProvider,
            string exchangeName,
            string queueName)
        {
            this.subscriptionManager = subscriptionManager;
            this.connection = connection;
            this.serviceProvider = serviceProvider;
            this.exchangeName = exchangeName;
            this.queueName = queueName;

            consumerChannel = InitializeConsumerChannel();
        }

        public Task Publish<T>(T @event) where T : IntegrationEvent
        {
            using (var model = connection.GetModel())
            {
                DeclareExchange(model);

                var properties = model.CreateBasicProperties();
                properties.Persistent = true;

                var data = JsonConvert.SerializeObject(@event);

                model.BasicPublish(exchange: exchangeName,
                    routingKey: typeof(T).GetEventName(),
                    basicProperties: properties,
                    body: System.Text.Encoding.UTF8.GetBytes(data));

                return Task.CompletedTask;
            }
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            if (subscriptionManager.HasSubscription<T, TH>() == false)
            {
                using (var model = connection.GetModel())
                {
                    DeclareExchange(model);

                    model.QueueBind(queue: queueName,
                        exchange: exchangeName,
                        routingKey: typeof(T).GetEventName());

                    BindConsumerQueue();
                }

                subscriptionManager.Subscribe<T, TH>();
            }
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            if (subscriptionManager.HasSubscription<T, TH>())
            {
                using (var model = connection.GetModel())
                {
                    DeclareExchange(model);

                    model.QueueUnbind(queue: queueName,
                        exchange: exchangeName,
                        routingKey: typeof(T).GetEventName());
                }

                subscriptionManager.Unsubscribe<T, TH>();
            }
        }

        private IModel InitializeConsumerChannel()
        {
            var model = connection.GetModel();

            DeclareExchange(model);

            //Declare the queue now so we don't have to create it later
            model.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

            model.CallbackException += (sender, e) =>
            {
                consumerChannel.Dispose();
                
                consumerChannel = InitializeConsumerChannel();
                BindConsumerQueue();
            };

            return model;
        }

        private void BindConsumerQueue()
        {
            if (consumerChannel != null)
            {
                AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(consumerChannel);

                consumer.Received += Consumer_Received;

                consumerChannel.BasicConsume(queue: queueName,
                    autoAck: false,
                    consumer: consumer);
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var subscriptions = subscriptionManager.GetEventSubscriptions(@event.RoutingKey);

            if (subscriptions.Any())
            {
                var data = System.Text.Encoding.UTF8.GetString(@event.Body.ToArray());

                var eventObject = JsonConvert.DeserializeObject(data, subscriptions.First().EventType);

                foreach (Subscription subscription in subscriptions)
                {
                    using (var childScope = serviceProvider.CreateScope())
                    {
                        var handler = ActivatorUtilities.CreateInstance(childScope.ServiceProvider, subscription.HandlerType);
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(subscription.EventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { eventObject });
                    }
                }
            }

            consumerChannel.BasicAck(@event.DeliveryTag, multiple: false);

            return;
        }

        private Action<IModel> DeclareExchange =>
            (mdl) =>
            {
                mdl.ExchangeDeclare(exchange: exchangeName,
                    type: ExchangeType.Direct);
            };
    }
}
