using System;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Messaging.Notifications;

namespace SoftwarePioniere.DomainModel.Services
{
    /// <summary>
    /// Base Saga with MessageBus
    /// </summary>
    public abstract class SagaBase : Saga
    {
        protected readonly IMessageBus Bus;
      
        protected readonly ILogger Logger;

        protected SagaBase(ILoggerFactory loggerFactory, IMessageBus bus)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger(GetType());
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        protected virtual Task SubscribeCommand<T>(Func<T, Task> handler) where T : class, ICommand
        {
            return Bus.SubscribeAsync<T>(async msg =>
            {
                try
                {                    
                    await handler(msg);
                    await Bus.PublishAsync(CommandSucceededNotification.Create(msg));
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error on Executing Command {CommandType} {Command}", typeof(T), msg);
                    await Bus.PublishAsync(CommandFailedNotification.Create(msg, e));
                }
            });
        }
    }
}
