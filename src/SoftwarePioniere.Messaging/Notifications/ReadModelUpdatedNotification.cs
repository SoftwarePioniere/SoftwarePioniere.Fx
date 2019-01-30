

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging.Notifications
{
    /// <summary>
    /// Notification for ReadModel Updates
    /// </summary>
    public class ReadModelUpdatedNotification : NotificationContentBase
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const string TypeKey = "readmodel.updated";

        public const string MethodInsert = "INSERT";
        public const string MethodUpdate = "UPDATE";
        public const string MethodDelete = "DELETE";

        /// <inheritdoc />
        public ReadModelUpdatedNotification() : base(TypeKey)
        {            
        }

        [JsonProperty("entity_id")]
        public string EntityId { get; set; }

        [JsonProperty("entity_type")]
        public string EntityType { get; set; }

        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }
        
        [JsonProperty("reason")]
        public string Reason { get; set; }
        
        [JsonProperty("tags")]
        public string[] Tags { get; set; }
    }
}
