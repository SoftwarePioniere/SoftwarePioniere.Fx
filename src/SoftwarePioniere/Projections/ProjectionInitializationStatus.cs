using Newtonsoft.Json;
using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{
    public class ProjectionInitializationStatus : Entity
    {
        public ProjectionInitializationStatus() : base("pr.i.s")
        {
        }

        [JsonProperty("projector_id")]
        public string ProjectorId { get; set; }
        
        [JsonProperty("stream_name")]
        public string StreamName { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_text")]
        public string StatusText { get; set; }

        public static string StatusNew = "NEW";
        public static string StatusReady = "READY";
        public static string StatusPending = "PENDING";

    }
}
