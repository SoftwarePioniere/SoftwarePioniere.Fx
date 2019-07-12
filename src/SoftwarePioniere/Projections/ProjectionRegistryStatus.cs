using Newtonsoft.Json;

namespace SoftwarePioniere.Projections
{
    public class ProjectionRegistryStatus
    {
        public ProjectionRegistryStatus()
        {
            Projectors = new ProjectionInitializationStatus[0];
            Status = ProjectionInitializationStatus.StatusNew;
        }
        
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("ready")]
        public int Ready { get; set; }

        [JsonProperty("pending")]
        public int Pending { get; set; }

        [JsonProperty("projectors")]
        public ProjectionInitializationStatus[] Projectors { get; set; }

       
    }
}