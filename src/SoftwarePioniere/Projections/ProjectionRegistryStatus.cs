using J = Newtonsoft.Json.JsonPropertyAttribute;
using J1 = System.Text.Json.Serialization.JsonPropertyNameAttribute;


namespace SoftwarePioniere.Projections
{
    public class ProjectionRegistryStatus
    {
        public ProjectionRegistryStatus()
        {
            Projectors = new ProjectionInitializationStatus[0];
            Status = ProjectionInitializationStatus.StatusNew;
        }
        
        [J("status")]
        [J1("status")]
        public string Status { get; set; }

        [J("total")]
        [J1("total")]
        public int Total { get; set; }

        [J("ready")]
        [J1("ready")]
        public int Ready { get; set; }

        [J("pending")]
        [J1("pending")]
        public int Pending { get; set; }

        [J("projectors")]
        [J1("projectors")]
        public ProjectionInitializationStatus[] Projectors { get; set; }

       
    }
}