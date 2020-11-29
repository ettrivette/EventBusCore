using EventBusAPI.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using EventBus.RabbitMq;
using EventBus.Abstractions.Events;
using EventBusAPI.Infrastructure.Events;

namespace EventBusReceiver
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TestDbContext>(config =>
            {
                config.UseInMemoryDatabase("Test");
            });

            services.AddControllers();

            services.AddRabbitMqEventBus((config) =>
            {
                config.ConnectionString = "localhost";
                config.ExchangeName = "Test_Broker";
                config.QueueName = "Test_Queue";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var eventBus = app.ApplicationServices.GetService<IEventBus>();

            eventBus.Subscribe<PersonAddedEvent, PersonAddedEventHandler>();
        }
    }
}
