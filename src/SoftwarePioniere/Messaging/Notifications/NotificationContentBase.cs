// ReSharper disable MemberCanBePrivate.Global
namespace SoftwarePioniere.Messaging.Notifications
{
    public abstract class NotificationContentBase : INotificationContent
    {
        protected NotificationContentBase(string notificationType)
        {
            NotificationType = notificationType;
        }

        public string NotificationType { get; set; }
    }
}
