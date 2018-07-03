using SoftwarePioniere.ReadModel;

namespace SoftwarePioniere.Projections
{
    public class ProjectionStatus : Entity
    {
        public ProjectionStatus() : base("pr.s")
        {

        }

        public long? LastCheckPoint { get; set; }
    }
}
