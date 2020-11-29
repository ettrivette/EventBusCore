using EventBus.Abstractions.Events;
using EventBusAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using EventBusAPI.Infrastructure.Events;

namespace EventBusSender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IEventBus eventBus;

        public TestController(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(Person item)
        {
            await eventBus.Publish<PersonAddedEvent>(new PersonAddedEvent()
            {
                Person = item,
                PublishedDate = System.DateTimeOffset.Now,
                Id = Guid.NewGuid()
            });

            return Ok();
        }
    }
}
