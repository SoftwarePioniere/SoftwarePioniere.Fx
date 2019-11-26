using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Messaging.Notifications;

namespace SoftwarePioniere.FakeDomain
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FakeNotificationHandler : IMessageHandler, IHandleMessage<NotificationMessage>
    {
        private readonly IMessageSubscriber _subscriber;
        public static IList<Guid> HandledByCommandFailedNotification { get; } = new List<Guid>();
        public static IList<Guid> HandledByCommandSucceededNotification { get; } = new List<Guid>();


        private readonly ILogger _logger;
        public FakeNotificationHandler(IMessageSubscriber subscriber, ILoggerFactory loggerFactory)
        {
            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
            _logger = loggerFactory.CreateLogger(GetType());

        }

        public Task HandleAsync(NotificationMessage message)
        {
            _logger.LogInformation("Handle NotificationMessage");


            if (message.NotificationType == CommandFailedNotification.TypeKey)
            {
                var not = JsonConvert.DeserializeObject<CommandFailedNotification>(message.Content);
                HandledByCommandFailedNotification.Add(not.CommandId);
            }

            if (message.NotificationType == CommandSucceededNotification.TypeKey)
            {
                var not = JsonConvert.DeserializeObject<CommandSucceededNotification>(message.Content);
                HandledByCommandSucceededNotification.Add(not.CommandId);
            }

            return Task.CompletedTask;
        }



        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _subscriber.SubscribeAsync<NotificationMessage>(HandleAsync, cancellationToken: cancellationToken);
        }
    }
}
