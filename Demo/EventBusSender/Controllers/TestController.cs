using EventBus.Abstractions.Events;
using EventBusAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using EventBusAPI.Infrastructure.Events;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EventBusSender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IEventBus eventBus; 
        private readonly TestDbContext context;

        public TestController(IEventBus eventBus,
            TestDbContext context)
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

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            List<Person> people = await context.People.ToListAsync();

            return Ok(people);
        }
    }
}
