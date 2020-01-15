using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace WebApp.Controller
{
    public class KeyValueItem
    {
        [J("key")]
        public string Key { get; set; }

        [J("value")]
        public string Value { get; set; }
    }
}