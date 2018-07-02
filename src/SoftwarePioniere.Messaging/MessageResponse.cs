using System;
using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging
{
    public class MessageResponse : ResponseBase
    {
        //[JsonProperty(PropertyName = "exception")]
        //public Exception Exception { get; set; }
        
        [JsonProperty(PropertyName = "request_id")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "message_id")]
        public Guid MessageId { get; set; }
    }
}
