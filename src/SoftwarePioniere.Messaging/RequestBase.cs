using System;
using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging
{
    /// <summary>
    /// Base Request
    /// </summary>
    public class RequestBase
    {
        /// <summary>
        /// TimeStamp from the client in UTC
        /// </summary>
        [JsonRequired]
        [JsonProperty(PropertyName = "timestamp_utc")]
        public DateTime TimeStampUtc { get; set; }
      
    }
}