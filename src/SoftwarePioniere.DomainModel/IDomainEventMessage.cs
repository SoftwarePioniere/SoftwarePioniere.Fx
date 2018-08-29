using SoftwarePioniere.Messaging;

namespace SoftwarePioniere.DomainModel
{
    public interface IDomainEventMessage : IMessage
    {
        string AggregateId { get; }
        string AggregateName { get; }
        string DomainEvent { get; }
        string DomainEventType { get; }
    }
}