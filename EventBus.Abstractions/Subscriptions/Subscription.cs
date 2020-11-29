using EventBus.Abstractions.Events;
using System;

namespace EventBus.Abstractions.Subscriptions
{
    public class Subscription
    {
        public static Subscription Create<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            return new Subscription()
            {
                EventType = typeof(T),
                HandlerType = typeof(TH)
            };
        }

        public Type EventType { get; set; }

        public Type HandlerType { get; set; }
    }
}
