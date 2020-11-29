using EventBus.Abstractions.Events;
using EventBus.Abstractions.Subscriptions;
using System.Collections.Generic;
using System.Linq;

namespace EventBus.Abstractions
{
    public class InMemorySubscriptionManager : ISubscriptionManager
    {
        private readonly List<Subscription> subscriptions;

        public InMemorySubscriptionManager()
        {
            this.subscriptions = new List<Subscription>();
        }

        public IList<Subscription> GetEventSubscriptions(string eventTypeName)
        {
            var existingSubscriptions = subscriptions.Where(t => t.EventType.GetEventName() == eventTypeName)
                .Distinct()
                .ToList();

            return existingSubscriptions;
        }

        public bool HasSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            Subscription existing = subscriptions.FirstOrDefault(t => t.EventType == typeof(T)
                && t.HandlerType == typeof(TH));

            return existing != null;
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            if(!subscriptions.Any(t => t.EventType.Equals(typeof(T))
                && t.HandlerType.Equals(typeof(TH))))
            {
                subscriptions.Add(Subscription.Create<T, TH>());
            }
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            Subscription existing = subscriptions.FirstOrDefault(t => t.EventType == typeof(T)
                && t.HandlerType == typeof(TH));

            if (existing != null)
                subscriptions.Remove(existing);
        }
    }
}
