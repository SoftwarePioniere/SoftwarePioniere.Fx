using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;

using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{
    public class ProjectionInitializationStatus : Entity
    {
        public ProjectionInitializationStatus() : base("pr.i.s")
        {
        }

        [J("projector_id")]
        [J1("projector_id")]
        public string ProjectorId { get; set; }
        
        [J("stream_name")]
        [J1("stream_name")]
        public string StreamName { get; set; }
        
        [J("status")]
        [J1("status")]
        public string Status { get; set; }

        [J("status_text")]
        [J1("status_text")]
        public string StatusText { get; set; }

        public static string StatusNew = "NEW";
        public static string StatusReady = "READY";
        public static string StatusPending = "PENDING";

    }
}
