using EventBusAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EventBusReceiver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly TestDbContext context;

        public TestController(TestDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            List<Person> people = await context.People.ToListAsync();

            return Ok(people);
        }
    }
}
