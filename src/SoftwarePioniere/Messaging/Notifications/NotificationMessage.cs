using System;

using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;


namespace SoftwarePioniere.Messaging.Notifications
{
    public class NotificationMessage : INotificationMessage
    {
        
        [J("content")]
        [J1("content")]
        public string Content { get; set; }

        [J("notification_type")]
        [J1("notification_type")]
        public string NotificationType { get; set; }

        [J("user_name")]
        [J1("user_name")]
        public string UserName { get; set; }

        [J("user_id")]
        [J1("user_id")]
        public string UserId { get; set; }

        [J("id")]
        [J1("id")]
        public Guid Id { get; set; }

        [J("timestamp_utc")]
        [J1("timestamp_utc")]
        public DateTime TimeStampUtc { get; set; }

        [J("tags")]
        [J1("tags")]
        public string[] Tags { get; set; }
    }
}