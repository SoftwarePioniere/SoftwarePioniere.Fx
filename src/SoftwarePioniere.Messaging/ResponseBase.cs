using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging
{
    public abstract class ResponseBase
    {
        [JsonProperty(PropertyName = "is_error")]
        public bool IsError => !string.IsNullOrEmpty(Error);
        
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        //[JsonProperty(PropertyName = "http_status_code")]
        //public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;
    }
}