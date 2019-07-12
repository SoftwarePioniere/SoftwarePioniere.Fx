using System;
using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging
{
    public class MessageResponse : ResponseBase
    {
     
        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "message_id")]
        public Guid MessageId { get; set; }
    }

}
