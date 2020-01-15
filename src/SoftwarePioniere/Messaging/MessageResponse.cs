using System;
using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;


namespace SoftwarePioniere.Messaging
{
    public class MessageResponse : ResponseBase
    {

        [J("user_id")]
        [J1("user_id")]
        public string UserId { get; set; }

        [J("message_id")]
        [J1("message_id")]
        public Guid MessageId { get; set; }
    }

}
