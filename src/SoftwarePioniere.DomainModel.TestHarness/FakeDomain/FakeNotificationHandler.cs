using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoftwarePioniere.Messaging;
using SoftwarePioniere.Messaging.Notifications;

namespace SoftwarePioniere.DomainModel.FakeDomain
{
    public class FakeNotificationHandler : IMessageHandler, IHandleMessage<NotificationMessage>
    {
        public static IList<Guid> HandledByCommandFailedNotification { get; } = new List<Guid>();
        public static IList<Guid> HandledByCommandSucceededNotification { get; } = new List<Guid>();


        private readonly ILogger _logger;
        public FakeNotificationHandler(IMessageSubscriber subscriber, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            subscriber.SubscribeAsync<NotificationMessage>(HandleAsync);
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
    }
}
