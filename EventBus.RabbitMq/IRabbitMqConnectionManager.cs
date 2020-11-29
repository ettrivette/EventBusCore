using RabbitMQ.Client;
using System;

namespace EventBus.RabbitMq
{
    public interface IRabbitMqConnectionManager : IDisposable
    {
        bool IsConnected { get; }

        bool Connect();
        IModel GetModel();
    }
}