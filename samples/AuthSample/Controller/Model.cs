using J = Newtonsoft.Json.JsonPropertyAttribute;
//using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace AuthSample.Controller
{
    public class ApiInfo
    {
        [J("title")]
        public string Title1 { get; set; }

        [J("version")]
        public string Version1 { get; set; }
    }

    public class ClaimInfo
    {
        [J("type")]
        public string Type { get; set; }

        [J("value")]
        public string Value { get; set; }
    }
}
