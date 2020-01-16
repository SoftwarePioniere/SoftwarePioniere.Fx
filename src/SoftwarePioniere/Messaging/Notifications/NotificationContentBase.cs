using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace SoftwarePioniere.Messaging.Notifications
{
    public abstract class NotificationContentBase : INotificationContent
    {
        protected NotificationContentBase(string notificationType)
        {
            NotificationType = notificationType;
        }

        [J("notification_type")]
        [J1("notification_type")]
        public string NotificationType { get; set; }
    }
}
