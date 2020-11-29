using System.Threading.Tasks;

namespace EventBus.Abstractions.Events
{
    public interface IIntegrationEventHandler<T> where T : IntegrationEvent
    {
        Task Handle(T @event);
    }
}
