using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging.Notifications
{
    public class NotificationMessageTypeInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("full_name")]
        public string FullName { get; set; }
        [JsonProperty("type_key")]
        public string TypeKey { get; set; }
    }
}