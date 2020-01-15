using System;

using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;

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
 
        [J("timestamp_utc")]
        [J1("timestamp_utc")]
        public DateTime TimeStampUtc { get; set; }
      
    }
}