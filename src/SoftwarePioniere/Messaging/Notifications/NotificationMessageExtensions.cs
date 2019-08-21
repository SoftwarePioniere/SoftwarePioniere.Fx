using System;
using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging.Notifications
{
    public static class NotificationMessageExtensions
    {
        public static NotificationMessage CreateNotificationMessage(this INotificationContent c, IMessage msg)
        {
            return new NotificationMessage
            {
                Content = JsonConvert.SerializeObject(c),
                Id = Guid.NewGuid(),
                NotificationType = c.NotificationType,
                TimeStampUtc = msg.TimeStampUtc,
                UserId = msg.UserId
            };
        }
    }
}