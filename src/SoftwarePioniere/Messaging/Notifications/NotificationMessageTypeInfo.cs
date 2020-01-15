using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;
namespace SoftwarePioniere.Messaging.Notifications
{
    public class NotificationMessageTypeInfo
    {
        [J("name")]
        [J1("name")]
        public string Name { get; set; }
        
        [J("full_name")]
        [J1("full_name")]
        public string FullName { get; set; }

        [J("type_key")]
        [J1("type_key")]
        public string TypeKey { get; set; }
    }
}