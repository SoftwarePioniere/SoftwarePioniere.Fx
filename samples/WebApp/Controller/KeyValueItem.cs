using Newtonsoft.Json;

namespace WebApp.Controller
{
    public class KeyValueItem
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}