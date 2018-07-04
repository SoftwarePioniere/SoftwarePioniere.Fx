﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Messaging.Notifications;

// ReSharper disable MemberCanBePrivate.Global

namespace SoftwarePioniere.DomainModel
{

    /// <summary>
    /// Base Saga with MessageBus
    /// </summary>
    public abstract class SagaBase : ISaga
    {
        /// <summary>
        /// MessageBus
        /// </summary>
        protected readonly IMessageBus Bus;
       
        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger Logger;

        protected SagaBase(ILoggerFactory loggerFactory, IMessageBus bus)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger(GetType());
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <summary>
        /// Handle Command and Send Succedded or Failed Notification
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected Task SubscribeCommand<T>(Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class, ICommand
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
            }, cancellationToken);
        }

        public abstract void Initialize(CancellationToken cancellationToken = default);
    }
}
