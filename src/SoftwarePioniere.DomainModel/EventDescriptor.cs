using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel
{
    public struct EventDescriptor
    {
        public readonly IDomainEvent EventData;
        public readonly int Version;

        public EventDescriptor(IDomainEvent eventData, int version)
        {
            EventData = eventData;
            Version = version;
        }
    }


}