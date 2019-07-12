namespace SoftwarePioniere.Messaging
{
    public interface IAggregateDomainEvent
    {
        string AggregateId { get; }
        string AggregateType { get; }
        string DomainEventType { get; }
        string DomainEventContent { get; }
    }

    public interface IAggregateDomainEventMessage : IAggregateDomainEvent, IMessage
    {

    }

    // ReSharper disable once UnusedTypeParameter
    //public interface IAggregateDomainEventMessage<TAggregate, out TDomainEvent> : IAggregateDomainEvent, IMessage
    //    where TDomainEvent : IDomainEvent
    //    where TAggregate : IAggregateRoot
    //{       
    //    TDomainEvent DomainEventContent { get; }      
    //}
}