using System;
using System.Threading.Tasks;

namespace EventBus.Abstractions.Events
{
    public interface IEventBus
    {
        Task Publish<T>(T @event) where T : IntegrationEvent;
        void Subscribe<T, TH>() where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
        void Unsubscribe<T, TH>() where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
    }
}
