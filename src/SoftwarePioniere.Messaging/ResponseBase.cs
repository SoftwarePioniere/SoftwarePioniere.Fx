using System.Collections.Generic;
using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging
{
    public abstract class ResponseBase
    {
        protected ResponseBase()
        {
            Properties = new Dictionary<string, string>();
        }

        [JsonProperty(PropertyName = "is_error")]
        public bool IsError => !string.IsNullOrEmpty(Error);

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }

       
    }

    public static class ResponseExtensions
    {
        public static void AddProperty(this ResponseBase response, string key, string value)
        {
            if (response.Properties.ContainsKey(key))
                response.Properties.Remove(key);

            response.Properties.Add(key, value);
        }
    }
}