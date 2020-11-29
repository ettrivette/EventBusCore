using EventBus.Abstractions.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBusAPI.Infrastructure.Events
{
    public class PersonAddedEvent
        : IntegrationEvent
    {
        public Person Person { get; set; }
    }
}
