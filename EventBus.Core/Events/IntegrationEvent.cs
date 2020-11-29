using System;

namespace EventBus.Abstractions.Events
{
    public class IntegrationEvent
    {
        public Guid Id { get; set; }

        public DateTimeOffset PublishedDate { get; set; }
    }
}
