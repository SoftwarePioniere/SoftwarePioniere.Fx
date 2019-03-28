using System;

namespace SoftwarePioniere.Messaging
{
    public class AggregateDomainEventMessage<TAggregate, TDomainEvent> : MessageBase,
        IAggregateDomainEventMessage<TAggregate, TDomainEvent>
        where TDomainEvent : IDomainEvent
        where TAggregate : IAggregateRoot
    {
        public AggregateDomainEventMessage(Guid id, DateTime timeStampUtc, string userId, TDomainEvent domainEventContent, string aggregateId) : base(id, timeStampUtc, userId)
        {
            DomainEventContent = domainEventContent;
            AggregateId = aggregateId;
            DomainEventType = typeof(TDomainEvent).GetTypeShortName();
            AggregateType = typeof(TAggregate).GetTypeShortName();           
        }

        public string AggregateId { get; }
        public string AggregateType { get; }   
        public TDomainEvent DomainEventContent { get; }
        public string DomainEventType { get; }
    }

    public static class AggregateDomainEventMessageExtensions
    {
        public static AggregateDomainEventMessage<TAggregate, TDomainEvent>
            CreateAggregateDomainEventMessage<TAggregate, TDomainEvent>(this TDomainEvent domainEvent,
                TAggregate agg)
            where TDomainEvent : IDomainEvent
            where TAggregate : IAggregateRoot
        {
            var msg = new AggregateDomainEventMessage<TAggregate, TDomainEvent>(
                Guid.NewGuid(),
                domainEvent.TimeStampUtc,
                domainEvent.UserId,
                domainEvent,
                agg.Id
            );

            return msg;
        }

        public static object CreateTypedAggregateDomainEventMessage<TAggregate>(this IDomainEvent domainEvent,
            TAggregate aggregate)
            where TAggregate : IAggregateRoot
        {
            var typeArgument1 = typeof(TAggregate);
            var typeArgument2 = domainEvent.GetType();

            var genericClass = typeof(AggregateDomainEventMessage<,>);
            var constructedClass = genericClass.MakeGenericType(typeArgument1, typeArgument2);

            // public AggregateDomainEventMessage(Guid id, DateTime timeStampUtc, string userId, TDomainEvent domainEventContent, string aggregateId) : base(id, timeStampUtc, userId)                        
            var created = Activator.CreateInstance(constructedClass,
                Guid.NewGuid(), domainEvent.TimeStampUtc, domainEvent.UserId,
                domainEvent, aggregate.Id);

            return created;
        }
    }
}