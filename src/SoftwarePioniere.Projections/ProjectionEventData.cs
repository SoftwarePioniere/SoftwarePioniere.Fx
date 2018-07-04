using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.Projections
{
    public class ProjectionEventData
    {
        public IDomainEvent EventData { get; set; }
        public long EventNumber { get; set; }

    }
}
