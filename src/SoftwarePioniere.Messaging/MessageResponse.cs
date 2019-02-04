using System;
using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging
{
    public class MessageResponse : ResponseBase
    {
        public const string RequestIdKey = "RequestId";

        public const string TraceIdentifierKey = "TraceIdentifier";
        
        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "message_id")]
        public Guid MessageId { get; set; }
    }

    public static class MessageResponseExtensions
    {
        public static void SetTraceIdentifier(this MessageResponse response, string value)
        {
            response.AddProperty(MessageResponse.TraceIdentifierKey, value);
        }

        public static void SetRequestId(this MessageResponse response, string value)
        {
            response.AddProperty(MessageResponse.RequestIdKey, value);
        }

        public static string GetTraceIdentifier(this MessageResponse response)
        {
            if (response == null)
                return null;

            if (response.Properties.ContainsKey(MessageResponse.TraceIdentifierKey))
            {
                return response.Properties[MessageResponse.TraceIdentifierKey];
            }

            return null;
        }

        public static string GetRequestId(this MessageResponse response)
        {
            if (response == null)
                return null;

            if (response.Properties.ContainsKey(MessageResponse.RequestIdKey))
            {
                return response.Properties[MessageResponse.RequestIdKey];
            }

            return null;
        }
    }
}
