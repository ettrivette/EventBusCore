using EventBus.Abstractions.Events;
using System;
using System.Collections.Generic;

namespace EventBus.Abstractions.Subscriptions
{
    public interface ISubscriptionManager
    {
        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        bool HasSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        IList<Subscription> GetEventSubscriptions(string eventTypeName);
    }
}
