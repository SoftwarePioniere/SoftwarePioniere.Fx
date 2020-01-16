using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;

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

        [J("entity_id")]
        [J1("entity_id")]
        public string EntityId { get; set; }

        [J("entity_type")]
        [J1("entity_type")]
        public string EntityType { get; set; }

        [J("entity")]
        [J1("entity")]
        public string Entity { get; set; }

        [J("method")]
        [J1("method")]
        public string Method { get; set; }
        
        [J("reason")]
        [J1("reason")]
        public string Reason { get; set; }
        
        [J("tags")]
        [J1("tags")]
        public string[] Tags { get; set; }
    }
}
