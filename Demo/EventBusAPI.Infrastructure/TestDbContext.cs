using Microsoft.EntityFrameworkCore;

namespace EventBusAPI.Infrastructure
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) 
            : base(options)
        {
        
        }

        public DbSet<Person> People { get; set; }
    }
}
