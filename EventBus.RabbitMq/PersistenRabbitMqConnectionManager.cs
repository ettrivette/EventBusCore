using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using Polly;
using System.Net.Sockets;

namespace EventBus.RabbitMq
{
    public class PersistenRabbitMqConnectionManager
        : IRabbitMqConnectionManager
    {
        private readonly IConnectionFactory connectionFactory;
        private IConnection connection;
        private readonly ILogger<PersistenRabbitMqConnectionManager> logger;
        private readonly int retryCount;
        
        private bool disposed = false;
        private object objLock = new object();

        public PersistenRabbitMqConnectionManager(IConnectionFactory connectionFactory,
            ILogger<PersistenRabbitMqConnectionManager> logger,
            int retryCount = 5)
        {
            this.connectionFactory = connectionFactory;
            this.logger = logger;
            this.retryCount = retryCount;

            Connect();
        }

        public bool IsConnected =>
            connection != null && connection.IsOpen && !disposed;

        public bool Connect()
        {
            try
            {
                lock (objLock)
                {
                    var policy = Policy
                        .Handle<SocketException>()
                        .WaitAndRetry(retryCount, retryNumber => TimeSpan.FromSeconds(Math.Pow(2, retryNumber)));

                    policy.Execute(() =>
                    {
                        connection = connectionFactory.CreateConnection();
                    });

                    if (IsConnected)
                    {
                        connection.ConnectionBlocked += Connection_ConnectionBlocked;
                        connection.ConnectionShutdown += Connection_ConnectionShutdown;
                        connection.CallbackException += Connection_CallbackException;

                        return true;
                    }

                    logger.LogError("Failed to acquire connection to event bus.");

                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error establishing connection with RabbitMQ");
                return false;
            }
        }

        private void Connection_CallbackException(object sender, RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            if (disposed) return;

            logger.LogInformation("Callback exception encountered with RabbitMQ connection. Reconnecting...");

            Connect();
        }

        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (disposed) return;

            logger.LogInformation("Connection shutdown encountered with RabbitMQ connection. Reconnecting...");

            Connect();
        }

        private void Connection_ConnectionBlocked(object sender, RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        {
            if (disposed) return;

            logger.LogInformation("Connection blocked encountered with RabbitMQ connection. Reconnecting...");

            Connect();
        }

        public IModel GetModel()
        {
            if (IsConnected == false)
            {
                Connect();
            }
            
            return connection.CreateModel();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            this.disposed = true;

            try
            {
                connection.Dispose();
            }
            catch(Exception ex)
            {
                logger.LogCritical(ex, "Error disposing of RabbitMQ connection");
            }
        }
    }
}
