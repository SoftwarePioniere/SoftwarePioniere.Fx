using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel
{
    public interface IDomainEventMessage : IMessage
    {
        string AggregateId { get; }
        string AggregateName { get; }
        IMessage DomainEvent { get; }
        string DomainEventType { get; }
    }
}