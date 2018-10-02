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
        
        [JsonProperty("projectors")]
        public ProjectionInitializationStatus[] Projectors { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}