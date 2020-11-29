# EventBus Core
An event bus abstraction framework with a RabbitMQ implementation.

### Usage (RabbitMQ):
Start by registering the appropriate services:
```
services.AddRabbitMqEventBus((config) =>
{
    config.ConnectionString = Configuration["RabbitMQ:ConnectionString"];
    config.ExchangeName = "Test_Broker";
    config.QueueName = "Test_Queue";
});
```

The configuration has the following settings:
1. Connection String - RabbitMQ Uri format connection string
2. Exchange Name - Name of the exchange to use. Use empty string for the default exchange
3. Queue Name - Name to use on the queue
4. Enable Async - True or false on whether to enable async

After services are registered, event subscriptions should be added on the client side. The best way to do this
is via the Configure method in Startup:

```
var eventBus = app.ApplicationServices.GetService<IEventBus>();

eventBus.Subscribe<PersonAddedEvent, PersonAddedEventHandler>();
```

It is not necessary to register the event handler with the service collection. However, any dependency the handler needs must be registered
with the service collection.

Upcoming Feature List:
- RabbitMQ publishing strategies - support for all different types of messaging strategies
- Azure ServiceBus implementation