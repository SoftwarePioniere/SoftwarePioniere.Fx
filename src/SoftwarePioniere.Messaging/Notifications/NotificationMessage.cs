using System;
using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging.Notifications
{
    public class NotificationMessage : INotificationMessage
    {
        //public NotificationMessage(string notificationType)
        //{
        //    NotificationType = notificationType;
        //}

        [JsonProperty("content")]
        public string Content { get;  set;}

        [JsonProperty("notification_type")]
        [JsonRequired]
        public string NotificationType { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("timestamp_utc")]
        public DateTime TimeStampUtc { get; set; }
    }
}
