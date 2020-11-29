using EventBus.Abstractions.Events;
using System.Threading.Tasks;

namespace EventBusAPI.Infrastructure.Events
{
    public class PersonAddedEventHandler : IIntegrationEventHandler<PersonAddedEvent>
    {
        private readonly TestDbContext context;

        public PersonAddedEventHandler(TestDbContext context)
        {
            this.context = context;
        }

        public async Task Handle(PersonAddedEvent @event)
        {
            context.People.Add(@event.Person);

            await context.SaveChangesAsync();
        }
    }
}
